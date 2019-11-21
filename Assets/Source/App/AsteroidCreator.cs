using System.Collections.Generic;
using UnityEngine;
using MustHave.Utilities;
using MustHave;
using RawPhysics;

public class AsteroidCreator : MonoBehaviour
{
    [SerializeField] private Asteroid debugAsteroidPrefab = default;
    [SerializeField] private Asteroid asteroidPrefab = default;
    [SerializeField] private AsteroidExplosion asteroidExplosionPrefab = default;
    [SerializeField] private Transform asteroidContainer = default;
    [SerializeField] private Transform asteroidPool = default;
    [SerializeField] private Transform explosionContainer = default;
    [SerializeField] private Grid spawnGrid = default;
    [SerializeField] private int spawnGridXYSize = default;

    private const float ANGULAR_VELOCITY_MAX = 360f * Mathf.Deg2Rad;
    private const float ANGULAR_VALOCITY_MIN = 15f * Mathf.Deg2Rad;
    private const float LINEAR_VELOCITY_MAX = 6f;
    private const float LINEAR_VELOCITY_MIN = 2f;
    private const int ASTEROID_POOL_CAPACITY = 30;

    private ThreadGrid threadGrid = default;
    private float asteroidColliderRadius = default;
    private float debugAsteroidColliderRadius = default;
    private Vector2 spawnGridSize = default;
    private Bounds2 cameraBounds = default;

    private readonly List<RawAsteroid> rawAsteroids = new List<RawAsteroid>();
    public Vector2 SpawnGridSize => spawnGridSize;
    public Transform AsteroidContainer => asteroidContainer;
    public int AsteroidGameObjectsCount => asteroidContainer.childCount;
    public List<RawAsteroid> RawAsteroids => rawAsteroids;

    private void Awake()
    {
        int cellSize = (int)spawnGrid.cellSize.x;
        spawnGrid.cellSize = new Vector2(cellSize, cellSize);
        spawnGridSize = new Vector2(cellSize * spawnGridXYSize, cellSize * spawnGridXYSize);

        RefreshCameraBounds();

        CircleCollider2D asteroidPrefabCollider = asteroidPrefab.GetComponent<CircleCollider2D>();
        CircleCollider2D debugAsteroidPrefabCollider = debugAsteroidPrefab.GetComponent<CircleCollider2D>();

        asteroidPrefabCollider.gameObject.layer = Layer.Default;
        asteroidPrefabCollider.enabled = false;
        asteroidColliderRadius = asteroidPrefabCollider.radius * asteroidPrefabCollider.transform.localScale.x;

        debugAsteroidPrefabCollider.gameObject.layer = Layer.Default;
        debugAsteroidPrefabCollider.enabled = false;
        debugAsteroidColliderRadius = debugAsteroidPrefabCollider.radius * debugAsteroidPrefabCollider.transform.localScale.x;

        Destroy(asteroidPrefabCollider);
        Destroy(debugAsteroidPrefabCollider);
    }

    private void Start()
    {
        threadGrid = AppManager.ThreadGrid;
    }

    public void RemoveAsteroidGameObjectsOutOfView()
    {
        List<Asteroid> list = new List<Asteroid>();
        foreach (Transform child in asteroidContainer)
        {
            list.Add(child.GetComponent<Asteroid>());
        }
        List<Asteroid> outOfViewList = list.FindAll(asteroid => !asteroid.RawAsteroid.isInCameraView);
        foreach (Asteroid asteroid in outOfViewList)
        {
            asteroid.RawAsteroid.RemoveGameObject();
        }
    }

    public GameObject PickAsteroidGameObjectFromPool(RawAsteroid rawAsteroid)
    {
        int lastIndex = asteroidPool.childCount - 1;
        if (lastIndex >= 0)
        {
            Transform transform = asteroidPool.GetChild(lastIndex);
            transform.GetComponent<Asteroid>().RawAsteroid = rawAsteroid;
            transform.gameObject.SetActive(true);
            transform.SetParent(asteroidContainer);
            transform.position = rawAsteroid.Position;
            transform.rotation = rawAsteroid.Rotation;
            return transform.gameObject;
        }
        else
            return CreateAsteroidGameObject(rawAsteroid, asteroidContainer).gameObject;
    }

    public void ReturnAsteroidGameObjectToPool(Transform asteroid)
    {
        if (asteroidPool.childCount < ASTEROID_POOL_CAPACITY)
        {
            asteroid.SetParent(asteroidPool, false);
            asteroid.gameObject.SetActive(false);
        }
        else
        {
            GameObject.Destroy(asteroid.gameObject);
        }
    }

    private Asteroid CreateAsteroidGameObject(Transform parent)
    {
        Asteroid asteroidPrefab = AppManager.DebugSprites ? debugAsteroidPrefab : this.asteroidPrefab;
        return asteroidPrefab.CreateInstance(parent);
    }

    private Asteroid CreateAsteroidGameObject(RawAsteroid rawAsteroid, Transform parent)
    {
        Asteroid asteroidPrefab = AppManager.DebugSprites ? debugAsteroidPrefab : this.asteroidPrefab;
        return asteroidPrefab.CreateInstance(parent, rawAsteroid);
    }

    public void CreateAsteroids()
    {
        float asteroidColliderRadius = AppManager.DebugSprites ? debugAsteroidColliderRadius : this.asteroidColliderRadius;
        RawCirclesCollision2D.basicDiameterSquared = 4f * asteroidColliderRadius * asteroidColliderRadius;

        debugAsteroidPrefab.gameObject.SetActive(true);
        asteroidPrefab.gameObject.SetActive(true);

        for (int i = 0; i < ASTEROID_POOL_CAPACITY; i++)
        {
            CreateAsteroidGameObject(asteroidPool);
        }

        Vector2 cellSize = spawnGrid.cellSize;
        Vector3Int cellPosition = Vector3Int.zero;
        Vector3 asteroidEuler = Vector3.zero;

        Bounds2 asteroidBounds = new Bounds2(Vector2.zero, new Vector2(2 * asteroidColliderRadius, 2 * asteroidColliderRadius));
        int halfXYCount = spawnGridXYSize >> 1;
        for (int y = -halfXYCount; y < halfXYCount; y++)
        {
            for (int x = -halfXYCount; x < halfXYCount; x++)
            {
                cellPosition.Set(x, y, 0);
                Vector3 asteroidPos = spawnGrid.GetCellCenterWorld(cellPosition);
                RawAsteroid rawAsteroid = new RawAsteroid(this, asteroidPos, asteroidEuler, asteroidBounds, asteroidColliderRadius);
                rawAsteroid.SetRandomVelocities(LINEAR_VELOCITY_MIN, LINEAR_VELOCITY_MAX, ANGULAR_VALOCITY_MIN, ANGULAR_VELOCITY_MAX);
                rawAsteroids.Add(rawAsteroid);
            }
        }
        int xyCount = halfXYCount << 1;
        int halfXCountInView = Mathf.CeilToInt(cameraBounds.Extents.x / cellSize.x);
        int halfYCountInView = Mathf.CeilToInt(cameraBounds.Extents.y / cellSize.y);
        for (int y = -halfYCountInView; y < halfYCountInView; y++)
        {
            int yIndex = (halfXYCount + y) * xyCount;
            for (int x = -halfXCountInView; x < halfXCountInView; x++)
            {
                int index = yIndex + halfXYCount + x;
                rawAsteroids[index].SetGameObject();
            }
        }

        debugAsteroidPrefab.gameObject.SetActive(false);
        asteroidPrefab.gameObject.SetActive(false);
    }

    public void RespawnAsteroid(RawAsteroid asteroid)
    {
        asteroid.SetRandomVelocities(LINEAR_VELOCITY_MIN, LINEAR_VELOCITY_MAX, ANGULAR_VALOCITY_MIN, ANGULAR_VELOCITY_MAX);
        asteroid.SetRandomPositionInBounds(threadGrid.Bounds, cameraBounds);
    }

    public void StartExplosion(Vector2 pos)
    {
        AsteroidExplosion explosion = Instantiate(asteroidExplosionPrefab, explosionContainer, false);
        explosion.transform.position = pos;
        explosion.Play();
    }

    public void RefreshCameraBounds()
    {
        if (Camera.main)
            cameraBounds = Camera.main.GetOrthographicBounds2();
    }

    public void Clear()
    {
        rawAsteroids.Clear();

        asteroidPool.DestroyAllChildren();

        asteroidContainer.DestroyAllChildren();

        explosionContainer.DestroyAllChildren();

    }
}
