using System.Collections.Generic;
using UnityEngine;
using MustHave.Utilities;
using MustHave;
using RawPhysics;

public class AsteroidCreator : MonoBehaviour
{
    [SerializeField]
    private Asteroid debugAsteroidPrefab;

    [SerializeField]
    private Asteroid asteroidPrefab;

    [SerializeField]
    private AsteroidExplosion asteroidExplosionPrefab;

    [SerializeField]
    private Transform asteroidContainer;

    [SerializeField]
    private Transform asteroidPool;

    [SerializeField]
    private Transform explosionContainer;

    [SerializeField]
    private Grid spawnGrid;

    private ThreadGrid threadGrid;

    //private const int spawnGridHalfXYCount = 40;
    //private const int spawnGridHalfXYCount = 60;
    private const int spawnGridHalfXYCount = 80;
    //private const int spawnGridHalfXYCount = 128;
    //private const int spawnGridHalfXYCount = 256;

    private const int spawnInCollCellCount = 10;

    private const float angularVelocityMax = 360f * Mathf.Deg2Rad;

    private const float angularVelocityMin = 15f * Mathf.Deg2Rad;

    private const float linearVelocityMax = 6f;

    private const float linearVelocityMin = 2f;

    private const int asteroidPoolCapacity = 30;

    private float asteroidColliderRadius;

    private float debugAsteroidColliderRadius;

    private Vector2 spawnGridSize;

    private List<RawAsteroid> rawAsteroids = new List<RawAsteroid>();

    public Vector2 SpawnGridSize => spawnGridSize;

    public Transform AsteroidContainer => asteroidContainer;

    public int AsteroidGameObjectsCount => asteroidContainer.childCount;

    public List<RawAsteroid> RawAsteroids => rawAsteroids;

    private Bounds2 cameraBounds;

    private void Awake()
    {
        int cellSize = (int)spawnGrid.cellSize.x;
        spawnGrid.cellSize = new Vector2(cellSize, cellSize);
        spawnGridSize = new Vector2(cellSize * spawnGridHalfXYCount * 2, cellSize * spawnGridHalfXYCount * 2);

        RefreshCameraBounds();

        CircleCollider2D asteroidPrefabCollider = asteroidPrefab.GetComponent<CircleCollider2D>();
        CircleCollider2D debugAsteroidPrefabCollider = debugAsteroidPrefab.GetComponent<CircleCollider2D>();

        asteroidPrefabCollider.gameObject.layer = Const.LayerDefault;
        asteroidPrefabCollider.enabled = false;
        asteroidColliderRadius = asteroidPrefabCollider.radius * asteroidPrefabCollider.transform.localScale.x;

        debugAsteroidPrefabCollider.gameObject.layer = Const.LayerDefault;
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
        //asteroidContainer.GetComponentsInChildren(true, list);
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
        if (asteroidPool.childCount < asteroidPoolCapacity)
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
        return asteroidPrefab.CreateInstance(this, parent);
    }

    private Asteroid CreateAsteroidGameObject(RawAsteroid rawAsteroid, Transform parent)
    {
        //Debug.LogWarning(GetType() + ".CreateAsteroidGameObject");
        Asteroid asteroidPrefab = AppManager.DebugSprites ? debugAsteroidPrefab : this.asteroidPrefab;
        return asteroidPrefab.CreateInstance(this, parent, rawAsteroid);
    }

    public void CreateAsteroids()
    {
        float asteroidColliderRadius = AppManager.DebugSprites ? debugAsteroidColliderRadius : this.asteroidColliderRadius;
        RawCirclesCollision2D.basicDiameterSquared = 4f * asteroidColliderRadius * asteroidColliderRadius;

        debugAsteroidPrefab.gameObject.SetActive(true);
        asteroidPrefab.gameObject.SetActive(true);

        for (int i = 0; i < asteroidPoolCapacity; i++)
        {
            CreateAsteroidGameObject(asteroidPool);
        }

        Vector2 cellSize = spawnGrid.cellSize;
        Vector3Int cellPosition = Vector3Int.zero;
        Vector3 asteroidEuler = Vector3.zero;
        //Bounds2 asteroidSpriteBounds = Bounds2.BoundsToBounds2(asteroidPrefab.GetComponent<SpriteRenderer>().bounds);
        //float asteroidSpriteMaxDim = Mathf.Max(asteroidSpriteBounds.Size.x, asteroidSpriteBounds.Size.y);

        Bounds2 asteroidBounds = new Bounds2(Vector2.zero, new Vector2(2 * asteroidColliderRadius, 2 * asteroidColliderRadius));
        int halfXYCount = spawnGridHalfXYCount;
        for (int y = -halfXYCount; y < halfXYCount; y++)
        {
            for (int x = -halfXYCount; x < halfXYCount; x++)
            {
                cellPosition.Set(x, y, 0);
                Vector3 asteroidPos = spawnGrid.GetCellCenterWorld(cellPosition);
                RawAsteroid rawAsteroid = new RawAsteroid(this, asteroidPos, asteroidEuler, asteroidBounds, asteroidColliderRadius);
                rawAsteroid.SetRandomVelocities(linearVelocityMin, linearVelocityMax, angularVelocityMin, angularVelocityMax);
                rawAsteroids.Add(rawAsteroid);
            }
        }
        int xyCount = halfXYCount << 1;
        int halfXCountInView = Mathf.CeilToInt(cameraBounds.Extents.x / cellSize.x);
        int halfYCountInView = Mathf.CeilToInt(cameraBounds.Extents.y / cellSize.y);
        //Debug.Log(GetType() + "."+halfXCountInView+" "+halfYCountInView);
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
        asteroid.SetRandomVelocities(linearVelocityMin, linearVelocityMax, angularVelocityMin, angularVelocityMax);
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
