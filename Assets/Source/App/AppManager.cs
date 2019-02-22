using System.Collections;
using System.Collections.Generic;
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

    private bool restartLevelRequested;

    private Player player;

    public ThreadGrid ThreadGrid { get => threadGrid; set => threadGrid = value; }
    public AsteroidCreator AsteroidCreator { get => asteroidCreator; set => asteroidCreator = value; }

    protected override void OnAwake()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0; // Other values will cause ignote targetFrameRate

        player = spaceship.GetComponent<Player>();

        debugPanel.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (running && Const.Multithreading)
        {
            threadGrid.WaitForAllThreads();
            threadGrid.PreSyncThreads();

            asteroidCreator.AddAsteroidsToRespawnInThreadCells(threadGrid);

            threadGrid.UpdateBounds();
            threadGrid.SyncThreads();
            threadGrid.SyncStopRequest();

            if (!restartLevelRequested)
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
        restartLevelRequested = false;
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

    public void RestartLevel()
    {
        StartCoroutine(RestartLevelRoutine());
    }

    public IEnumerator RestartLevelRoutine()
    {
        yield return new WaitForSeconds(1f);
        restartLevelRequested = true;
        threadGrid.RequestStop();
        yield return new WaitWhile(() => running);
        threadGrid.Clear();
        asteroidCreator.Clear();
        yield return new WaitForEndOfFrame();
        mainPanel.ResetScoreText();
        player.ResetScore();
        spaceship.Respawn();
        cameraDriver.ResetCameraPosition();
        Start();
    }
}