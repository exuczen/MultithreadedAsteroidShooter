using System.Collections;
using UnityEngine;
using DC;

public class AppManager : Singleton<AppManager>
{
    [SerializeField]
    private DebugPanel debugPanel;

    [SerializeField]
    private MainPanel mainPanel;

    [SerializeField]
    private CameraDriver cameraDriver;

    [SerializeField]
    private Spaceship spaceship;

    [SerializeField]
    private ThreadGrid threadGrid;

    [SerializeField]
    private AsteroidCreator asteroidCreator;

    private bool running;

    private bool stopRequested;

    private Player player;

    public ThreadGrid ThreadGrid { get => threadGrid; }

    public AsteroidCreator AsteroidCreator { get => asteroidCreator; }

    public MainPanel MainPanel { get => mainPanel; }

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

            asteroidCreator.AddAsteroidsToRespawnInThreadCells(threadGrid);
#if !DEBUG_GAME_OBJECTS
            asteroidCreator.DestroyAsteroidGameObjectsOutOfView();
#endif
            threadGrid.UpdateBounds();
            threadGrid.SyncThreads();
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
        asteroidCreator.CreateAsteroids();
        threadGrid.AddAsteroidsToThreadCells(asteroidCreator);
        threadGrid.StartThreads();
        stopRequested = false;
        running = true;
        debugPanel.SetDebugText(
            "total asteroids: " + asteroidCreator.TotalAsteroidsCount
            //+ "\nactive asteroids: " + asteroidCreator.GetActiveAsteroidsCount()
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
        spaceship.Respawn();
        cameraDriver.ResetCameraPosition();
        Start();
    }
}