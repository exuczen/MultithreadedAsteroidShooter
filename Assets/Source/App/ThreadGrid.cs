using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using DC;
using RawPhysics;

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

    public void CreateThreadCells(Vector2 gridSize, float collCellSize)
    {
        bool middleAnchor = true;
        //bool middleAnchor = false;
        int threadsCount;
        int xCount, yCount;
        if (middleAnchor)
        {
            threadsCount = 9;
            xCount = 3; // cols count
            yCount = 3; // rows count
            collCellSize = gridSize.x / 15;
            //collCellSize = gridSize.x / 9;
        }
        else
        {
            threadsCount = SystemInfo.processorCount;
            if (threadsCount > 1)
            {
                yCount = Mathf.FloorToInt(Mathf.Sqrt(threadsCount));
                xCount = threadsCount / yCount;
            }
            else
            {
                xCount = yCount = 1;
            }
            Vector2 size = gridSize;
            if ((size.x > size.y && xCount < yCount) || (size.x < size.y && xCount > yCount))
            {
                int tmp = xCount;
                xCount = yCount;
                yCount = tmp;
            }
        }
        //Debug.Log(GetType() + ".AddAsteroidsToThreadCells: " + threadsCount + " " + xCount + " " + yCount);
        SetParams(gridSize, new Vector2Int(xCount, yCount));
        bounds.Center = transform.position = (Vector2)Camera.main.transform.position;
        //return;
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

        //threadCellPrefab.gameObject.SetActive(false);
        //EventWaitHandle[] tempHandles = new EventWaitHandle[threadsCount - 1];
        //for (int i = 0; i < tempHandles.Length; i++)
        //{
        //    tempHandles[i] = cellThreadHandles[i];
        //}
        //cellThreadHandles = tempHandles;
        //ThreadCell[] tempCells = new ThreadCell[threadsCount - 1];
        //for (int i = 0; i < tempCells.Length; i++)
        //{
        //    tempCells[i] = threadCells[i];
        //}
        //threadCells = tempCells;
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