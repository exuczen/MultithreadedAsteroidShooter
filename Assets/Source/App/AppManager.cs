using System.Collections;
using UnityEngine;
using MustHave.DesignPatterns;
using MustHave.UI;

public class AppManager : Singleton<AppManager>
{
    [SerializeField] private bool debugGameObjects = default;
    [SerializeField] private bool debugSprites = default;
    [SerializeField] private SpriteRenderer markerPrefab = default;
    [SerializeField] private DebugPanel debugPanel = default;
    [SerializeField] private MainPanel mainPanel = default;
    [SerializeField] private CameraDriver cameraDriver = default;
    [SerializeField] private Spaceship spaceship = default;
    [SerializeField] private ThreadGrid threadGrid = default;
    [SerializeField] private AsteroidCreator asteroidCreator = default;

    private bool running = default;
    private bool stopRequested = default;
    private Player player = default;

    public static bool DebugGameObjects { get => _instance.debugGameObjects; }
    public static bool DebugSprites { get => _instance.debugSprites; }
    public static SpriteRenderer MarkerPrefab { get => _instance.markerPrefab; }
    public static ThreadGrid ThreadGrid { get => _instance.threadGrid; }

    public static SpriteRenderer CreateMarker(Vector3 position, Transform parent, Color color)
    {
        SpriteRenderer marker = Instantiate(_instance.markerPrefab, position, Quaternion.identity, parent);
        marker.gameObject.SetActive(true);
        marker.color = color;
        return marker;
    }

    protected override void OnAwake()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0; // Other values will cause ignote targetFrameRate

        player = spaceship.GetComponent<Player>();

        debugPanel.gameObject.SetActive(true);

        mainPanel.onRectTransformDimensionsChange = asteroidCreator.RefreshCameraBounds;
    }

    private void Update()
    {
        if (running)
        {
            threadGrid.WaitForAllThreads();
            threadGrid.PreSyncThreads();

            if (!debugGameObjects)
            {
                asteroidCreator.RemoveAsteroidGameObjectsOutOfView();
                spaceship.RemoveMissileGameObjectsOutOfView();
            }

            threadGrid.SyncThreads();

            spaceship.UpdateSpaceship();
            spaceship.UpdateShooting();
            cameraDriver.FollowTarget(spaceship.transform);
            threadGrid.UpdateBounds(spaceship.RawSpaceship.Velocity * Time.deltaTime);

            threadGrid.SyncStopRequest();

            if (!stopRequested)
                threadGrid.ResumeThreads();
            else
                running = false;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    private void Start()
    {
        asteroidCreator.RefreshCameraBounds();
        asteroidCreator.CreateAsteroids();
        if (threadGrid.CellCount > 0)
            threadGrid.UpdateBounds(Vector2.zero);
        else
            threadGrid.CreateThreadCells(asteroidCreator.SpawnGridSize);
        threadGrid.AddAsteroidsToThreadCells(asteroidCreator.RawAsteroids);
        threadGrid.AddBodyToThreadCell(spaceship.RawSpaceship);
        threadGrid.StartThreads();
        stopRequested = false;
        running = true;

        debugPanel.SetDebugText(
            "total asteroids: " + asteroidCreator.RawAsteroids.Count
        );
    }

    public void AddPlayerPoints(int points)
    {
        player.AddPoints(points);
        mainPanel.ScoreVaule = player.Score.ToString();
    }

    public void StartFailRoutine()
    {
        StartCoroutine(FailRoutine());
    }

    private IEnumerator FailRoutine()
    {
        stopRequested = true;
        threadGrid.RequestStop();
        yield return new WaitWhile(() => running);
        yield return new WaitForSeconds(0.5f);
        mainPanel.FailText.gameObject.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        mainPanel.FailText.gameObject.SetActive(false);
        mainPanel.RestartButton.gameObject.SetActive(true);
    }

    public void RestartLevel()
    {
        StartCoroutine(RestartLevelRoutine());
    }

    private IEnumerator RestartLevelRoutine()
    {
        yield return new WaitForEndOfFrame();
        threadGrid.Clear();
        asteroidCreator.Clear();
        yield return new WaitForEndOfFrame();
        player.ResetScore();
        spaceship.ResetSpaceship();
        spaceship.DestroyMissiles();
        cameraDriver.ResetCameraPosition();
        Start();
    }
}