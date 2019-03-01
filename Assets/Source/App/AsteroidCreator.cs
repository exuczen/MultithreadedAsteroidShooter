using System.Collections.Generic;
using UnityEngine;
using DC;

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

    private List<RawAsteroid> asteroidsToRespawn = new List<RawAsteroid>();

    //private const int spawnGridHalfXYCount = 40;
    private const int spawnGridHalfXYCount = 80;
    //private const int spawnGridHalfXYCount = 128;
    //private const int spawnGridHalfXYCount = 256;

    private const int spawnInCollCellCount = 10;

    private const float angularVelocityMax = 360f;

    private const float angularVelocityMin = 15f;

    private const float linearVelocityMax = 6f;

    private const float linearVelocityMin = 2f;

    private const int asteroidPoolCapacity = 30;

    private int collCellSize;

    private Vector2 spawnGridSize;

    private List<RawAsteroid> rawAsteroids = new List<RawAsteroid>();

    public Vector2 SpawnGridSize => spawnGridSize;

    public int CollCellSize => collCellSize;

    public Transform AsteroidContainer => asteroidContainer;

    public int TotalAsteroidsCount => asteroidContainer.childCount;

    public List<RawAsteroid> RawAsteroids => rawAsteroids;

    private Bounds2 cameraBounds;

    private void Awake()
    {
        threadGrid = AppManager.Instance.ThreadGrid;

        int cellSize = (int)spawnGrid.cellSize.x;
        collCellSize = cellSize * spawnInCollCellCount;
        spawnGrid.cellSize = new Vector2(cellSize, cellSize);
        spawnGridSize = new Vector2(cellSize * spawnGridHalfXYCount * 2, cellSize * spawnGridHalfXYCount * 2);

        RefreshCameraBounds();
    }

    private void Start()
    {
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
        Asteroid asteroidPrefab = AppManager.DebugSprites ? debugAsteroidPrefab : this.asteroidPrefab;
        asteroidPrefab.gameObject.SetActive(true);
        asteroidPrefab.gameObject.layer = Const.LayerDefault;
        Rigidbody2D prefabRigidBody = asteroidPrefab.GetComponent<Rigidbody2D>();
        Collider2D prefabCollider = asteroidPrefab.GetComponent<Collider2D>();
        prefabRigidBody.simulated = false;
        prefabCollider.enabled = false;
        //prefabRigidBody.simulated = true;
        //prefabCollider.enabled = true;
        //Destroy(prefabRigidBody);
        //Destroy(prefabCollider);

        for (int i = 0; i < asteroidPoolCapacity; i++)
        {
            CreateAsteroidGameObject(asteroidPool);
        }

        Vector2 cellSize = spawnGrid.cellSize;
        Vector3Int cellPosition = Vector3Int.zero;
        Vector3 asteroidEuler = Vector3.zero;
        Bounds2 asteroidBounds = Bounds2.BoundsToBounds2(asteroidPrefab.GetComponent<SpriteRenderer>().bounds);
        int halfXYCount = spawnGridHalfXYCount;
        for (int y = -halfXYCount; y < halfXYCount; y++)
        {
            for (int x = -halfXYCount; x < halfXYCount; x++)
            {
                cellPosition.Set(x, y, 0);
                Vector3 asteroidPos = spawnGrid.GetCellCenterWorld(cellPosition);
                RawAsteroid rawAsteroid = new RawAsteroid(this, asteroidPos, asteroidEuler, asteroidBounds, 0.65f);
                rawAsteroid.SetRandomVelocities(linearVelocityMin, linearVelocityMax, angularVelocityMin, angularVelocityMax);
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

        prefabRigidBody.simulated = false;
        prefabCollider.enabled = false;
        this.debugAsteroidPrefab.gameObject.SetActive(false);
        this.asteroidPrefab.gameObject.SetActive(false);
    }

    public void AddAsteroidsToRespawnInThreadCells(ThreadGrid threadGrid)
    {
        for (int i = 0; i < asteroidsToRespawn.Count; i++)
        {
            threadGrid.AddBodyToRespawnInThreadCell(asteroidsToRespawn[i]);
        }
        asteroidsToRespawn.Clear();
    }

    public void AddAsteroidToRespawn(RawAsteroid asteroid)
    {
        asteroid.RespawnTime = Time.time + Const.BodyRespawnInterval;
        asteroidsToRespawn.Add(asteroid);
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

        asteroidsToRespawn.Clear();
    }
}
