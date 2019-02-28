using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using DC;

public class ThreadGrid : CustomGrid
{
    [SerializeField]
    private ThreadCell threadCellPrefab;

    [SerializeField]
    private CollisionGrid collGridPrefab;

    [SerializeField]
    private CollisionCell collCellPrefab;

    private ThreadCell[] threadCells;

    private EventWaitHandle[] cellThreadHandles;

    public int CellsCount { get { return threadCells.Length; } }

    private void Awake()
    {
        enabled = false;
    }

    public void AddAsteroidsToThreadCells(AsteroidCreator asteroidCreator)
    {
        //Debug.Log(GetType() + ".AddAsteroidsToThreadCells: SystemInfo.processorCount=" + SystemInfo.processorCount);
        int threadsCount = SystemInfo.processorCount;
        //threadsCount = 4;

        int xCount; // cols count
        int yCount; // rows count

        if (threadsCount > 1)
        {
            yCount = Mathf.FloorToInt(Mathf.Sqrt(threadsCount));
            xCount = threadsCount / yCount;
        }
        else
        {
            xCount = yCount = 1;
        }
        Vector2 size = asteroidCreator.SpawnGridSize;
        if ((size.x > size.y && xCount < yCount) || (size.x < size.y && xCount > yCount))
        {
            int tmp = xCount;
            xCount = yCount;
            yCount = tmp;
        }
        //Debug.Log(GetType() + ".AddAsteroidsToThreadCells: " + threadsCount + " " + xCount + " " + yCount);
        SetParams(size, new Vector2Int(xCount, yCount));
        bounds.center = transform.position = (Vector2)Camera.main.transform.position;
        enabled = Const.Multithreading;
        threadCellPrefab.gameObject.SetActive(enabled);
        if (!enabled)
        {
            return;
        }
        else
        {
            threadCells = new ThreadCell[threadsCount];
            cellThreadHandles = new EventWaitHandle[threadsCount];
            
            int collCellSize = asteroidCreator.CollCellSize;
            for (int y = 0; y < yCount; y++)
            {
                for (int x = 0; x < xCount; x++)
                {
                    int cellIndex = y * xCount + x;
                    ThreadCell cell = threadCellPrefab.CreateInstance(this, new Vector2Int(x, y), out cellThreadHandles[cellIndex]);
                    cell.CreateCollisionGrid(collCellSize, collGridPrefab, collCellPrefab);
                    cell.name = "ThreadCell" + cellIndex;
                    threadCells[cellIndex] = cell;
                }
            }
            List<RawAsteroid> asteroids = asteroidCreator.RawAsteroids;

            for (int i = 0; i < asteroids.Count; i++)
            {
                AddBodyToThreadCell(asteroids[i]);
            }
        }
        threadCellPrefab.gameObject.SetActive(false);
    }

    public void WaitForAllThreads()
    {
        WaitHandle.WaitAll(cellThreadHandles);
    }

    public void PreSyncThreads()
    {
        for (int j = 0; j < threadCells.Length; j++)
        {
            threadCells[j].PreSyncThread();
        }
    }

    public void UpdateBounds()
    {
        Bounds cameraBounds = Camera.main.GetOrthographicBoundsWithOffset(0.1f);
        bounds.center = transform.position = cameraBounds.center;

        for (int j = 0; j < threadCells.Length; j++)
        {
            threadCells[j].UpdateBounds(cameraBounds);
        }
    }

    public void SyncThreads()
    {
        for (int j = 0; j < threadCells.Length; j++)
        {
            threadCells[j].SyncThread();
        }
    }

    public void ResumeThreads()
    {
        for (int j = 0; j < threadCells.Length; j++)
        {
            threadCells[j].ResumeThread();
        }
    }

    public void StartThreads()
    {
        if (threadCells != null)
        {
            for (int j = 0; j < threadCells.Length; j++)
            {
                threadCells[j].StartThread();
            }
        }
    }

    public void RequestStop()
    {
        if (threadCells != null)
        {
            for (int j = 0; j < threadCells.Length; j++)
            {
                threadCells[j].RequestStop();
            }
        }
    }

    public void SyncStopRequest()
    {
        for (int j = 0; j < threadCells.Length; j++)
        {
            threadCells[j].SyncStopRequest();
        }
    }

    public void AddBodyToRespawnInThreadCell(RawBody2D body)
    {
        int cellIndex = GetCellIndex(body.Position);
        threadCells[cellIndex].AddBodyToRespawn(body);
    }

    public void AddBodyToThreadCell(RawBody2D body)
    {
        int cellIndex = GetCellIndex(body.Position);
        threadCells[cellIndex].AddBody(body);
    }

    public void AddBodyToThreadCell(RawBody2D body, int cellIndex)
    {
        threadCells[cellIndex].AddBody(body);
    }

    public void Clear()
    {
        //for (int i = 0; i < threadCells.Length; i++)
        //{
        //    threadCells[i].ClearThreadData();
        //}
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        threadCells = null;
        cellThreadHandles = null;
    }
}