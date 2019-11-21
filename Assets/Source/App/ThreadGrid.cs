using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using MustHave;
using MustHave.Utilities;
using RawPhysics;

public class ThreadGrid : CustomGrid
{
    [SerializeField] private ThreadCell threadCellPrefab = default;
    [SerializeField] private CollisionGrid collGridPrefab = default;
    [SerializeField] private CollisionCell collCellPrefab = default;

    private ThreadCell[] threadCells = default;
    private EventWaitHandle[] cellThreadHandles = default;

    public int CellCount { get { return threadCells != null ? threadCells.Length : 0; } }

    private void Awake()
    {
        enabled = false;
    }

    public void CreateThreadCells(Vector2 gridSize)
    {
        int threadsCount;
        int xCount, yCount;
        int logicalProcessorCount = SystemInfo.processorCount;
        if (logicalProcessorCount > 4)
        {
            xCount = yCount = 5;
        }
        else if (logicalProcessorCount >= 2)
        {
            xCount = yCount = 3;
        }
        else
        {
            xCount = yCount = 1;
        }
        threadsCount = xCount * yCount;
        float collCellSize = gridSize.x / 15;
        SetParams(gridSize, new Vector2Int(xCount, yCount));
        bounds.Center = transform.position = (Vector2)Camera.main.transform.position;
        threadCellPrefab.gameObject.SetActive(true);
        threadCells = new ThreadCell[threadsCount];
        cellThreadHandles = new EventWaitHandle[threadsCount];

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
    }

    public void AddAsteroidsToThreadCells(List<RawAsteroid> asteroids)
    {
        for (int i = 0; i < asteroids.Count; i++)
        {
            AddBodyToThreadCell(asteroids[i]);
        }
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

    public void UpdateBounds(Vector2 dr)
    {
        Bounds2 cameraBounds = Camera.main.GetOrthographicBounds2WithOffset(0.1f);
        cameraBounds.Center += dr;
        bounds.Center = transform.position = cameraBounds.Center;

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
        for (int i = 0; i < threadCells.Length; i++)
        {
            threadCells[i].Clear();
        }
    }
}