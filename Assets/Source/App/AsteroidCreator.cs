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

    private int collCellSize;

    private Vector2 spawnGridSize;

    private List<RawAsteroid> rawAsteroids = new List<RawAsteroid>();

    public Vector2 SpawnGridSize => spawnGridSize;

    public int CollCellSize => collCellSize;

    public Transform AsteroidContainer => asteroidContainer;

    public int TotalAsteroidsCount => asteroidContainer.childCount;

    public List<RawAsteroid> RawAsteroids => rawAsteroids;

    private Bounds cameraBounds;

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

    public void DestroyAsteroidGameObjectsOutOfView()
    {
        List<Asteroid> list = new List<Asteroid>();
        //asteroidContainer.GetComponentsInChildren(true, list);
        for (int i = 0; i < asteroidContainer.childCount; i++)
        {
            list.Add(asteroidContainer.GetChild(i).GetComponent<Asteroid>());
        }
        List<Asteroid> outOfViewList = list.FindAll(asteroid => !asteroid.RawAsteroid.isInCameraView);
        foreach (Asteroid asteroid in outOfViewList)
        {
            asteroid.RawAsteroid.DestroyGameObject();
        }
    }

    public Asteroid CreateAsteroidGameObject(RawAsteroid rawAsteroid)
    {
        Asteroid asteroidPrefab = Const.DebugSprites ? debugAsteroidPrefab : this.asteroidPrefab;
        return asteroidPrefab.CreateInstance(this, asteroidContainer, rawAsteroid);
    }

    public void CreateAsteroids()
    {
        Asteroid asteroidPrefab = Const.DebugSprites ? debugAsteroidPrefab : this.asteroidPrefab;
        asteroidPrefab.gameObject.SetActive(true);
        asteroidPrefab.gameObject.layer = Const.Multithreading ? Const.LayerDefault : Const.LayerAsteroid;
        Rigidbody2D prefabRigidBody = asteroidPrefab.GetComponent<Rigidbody2D>();
        Collider2D prefabCollider = asteroidPrefab.GetComponent<Collider2D>();
        prefabRigidBody.simulated = !Const.Multithreading;
        prefabCollider.enabled = !Const.Multithreading;
        //prefabRigidBody.simulated = true;
        //prefabCollider.enabled = true;
        //Destroy(prefabRigidBody);
        //Destroy(prefabColliders);

        Vector2 cellSize = spawnGrid.cellSize;
        Vector3Int cellPosition = Vector3Int.zero;
        Vector3 asteroidEuler = Vector3.zero;
        Bounds asteroidBounds = asteroidPrefab.GetComponent<SpriteRenderer>().bounds;
        int halfXYCount = spawnGridHalfXYCount;
        for (int y = -halfXYCount; y < halfXYCount; y++)
        {
            for (int x = -halfXYCount; x < halfXYCount; x++)
            {
                cellPosition.Set(x, y, 0);
                Vector3 asteroidPos = spawnGrid.GetCellCenterWorld(cellPosition);
                RawAsteroid rawAsteroid = new RawAsteroid(this, asteroidPos, asteroidEuler, asteroidBounds, 0.65f);
                rawAsteroid.SetRandomVelocities(linearVelocityMin, linearVelocityMax, angularVelocityMin, angularVelocityMax);
                //rawAsteroid.CreateGameObject();
                rawAsteroids.Add(rawAsteroid);
            }
        }
        int xyCount = halfXYCount << 1;
        int halfXCountInView = Mathf.CeilToInt(cameraBounds.extents.x / cellSize.x);
        int halfYCountInView = Mathf.CeilToInt(cameraBounds.extents.y / cellSize.y);
        for (int y = -halfYCountInView; y < halfYCountInView; y++)
        {
            int yIndex = (halfXYCount + y) * xyCount;
            for (int x = -halfXCountInView; x < halfXCountInView; x++)
            {
                int index = yIndex + halfXYCount + x;
                rawAsteroids[index].CreateGameObject();
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
            cameraBounds = Camera.main.GetOrthographicBounds();
    }

    public void Clear()
    {
        rawAsteroids.Clear();

        asteroidContainer.DestroyAllChildren();

        explosionContainer.DestroyAllChildren();

        asteroidsToRespawn.Clear();
    }
}
