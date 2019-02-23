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

    private List<Asteroid> asteroidsToRespawn = new List<Asteroid>();

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

    private List<Asteroid> respawnList = new List<Asteroid>();

    public Vector2 SpawnGridSize { get => spawnGridSize; }

    public int CollCellSize { get => collCellSize; }

    public Transform AsteroidContainer { get => asteroidContainer; }

    public int TotalAsteroidsCount => asteroidContainer.childCount;

    private Vector2 cameraHalfSize;

    private void Awake()
    {
        threadGrid = AppManager.Instance.ThreadGrid;

        int cellSize = (int)spawnGrid.cellSize.x;
        collCellSize = cellSize * spawnInCollCellCount;
        spawnGrid.cellSize = new Vector2(cellSize, cellSize);
        spawnGridSize = new Vector2(cellSize * spawnGridHalfXYCount * 2, cellSize * spawnGridHalfXYCount * 2);
    }

    private void Start()
    {
        RefreshCameraSize();
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

        Vector3Int cellPosition = Vector3Int.zero;
        int halfXYCount = spawnGridHalfXYCount;
        for (int y = -halfXYCount; y < halfXYCount; y++)
        {
            for (int x = -halfXYCount; x < halfXYCount; x++)
            {
                cellPosition.Set(x, y, 0);
                Vector3 asteroidPos = spawnGrid.GetCellCenterWorld(cellPosition);
                Asteroid asteroid = asteroidPrefab.CreateInstance(this, asteroidContainer, asteroidPos);
                asteroid.SetRandomVelocities(linearVelocityMin, linearVelocityMax, angularVelocityMin, angularVelocityMax);
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

    public void AddAsteroidToRespawn(Asteroid asteroid)
    {
        if (asteroid.gameObject.activeSelf)
        {
            asteroid.RespawnTime = Time.time + Const.BodyRespawnInterval;
            asteroidsToRespawn.Add(asteroid);
            asteroid.gameObject.SetActive(false);
        }
    }

    public void RespawnAsteroid(Asteroid asteroid)
    {
        asteroid.SetRandomVelocities(linearVelocityMin, linearVelocityMax, angularVelocityMin, angularVelocityMax);
        asteroid.SetRandomPositionInBounds(threadGrid.Bounds, cameraHalfSize);
        asteroid.gameObject.SetActive(true);
    }

    public void StartExplosion(Vector2 pos)
    {
        AsteroidExplosion explosion = Instantiate(asteroidExplosionPrefab, explosionContainer, false);
        explosion.transform.position = pos;
        explosion.Play();
    }

    public void RefreshCameraSize()
    {
        Camera camera = Camera.main;
        if (camera)
        {
            cameraHalfSize = new Vector2(camera.orthographicSize * Screen.width / Screen.height, camera.orthographicSize);
        }
    }

    public void Clear()
    {
        foreach (Transform child in asteroidContainer)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in explosionContainer)
        {
            Destroy(child.gameObject);
        }
        asteroidsToRespawn.Clear();
    }
}
