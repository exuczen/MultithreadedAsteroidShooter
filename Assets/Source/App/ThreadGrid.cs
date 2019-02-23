using DC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

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
        size = asteroidCreator.SpawnGridSize;
        if ((size.x > size.y && xCount < yCount) || (size.x < size.y && xCount > yCount))
        {
            int tmp = xCount;
            xCount = yCount;
            yCount = tmp;
        }
        //Debug.Log(GetType() + ".AddAsteroidsToThreadCells: " + threadsCount + " " + xCount + " " + yCount);
        SetParams(size, new Vector2Int(xCount, yCount));
        UpdateBounds();
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
            Transform asteroids = asteroidCreator.AsteroidContainer;

            //Debug.Log(GetType() + ". " + Const.FloatToIntFactor + " " + Const.FloatToIntFactorLog2);
            for (int i = 0; i < asteroids.childCount; i++)
            {
                Transform asteroidTransform = asteroids.GetChild(i);
                SpriteBody2D body = asteroidTransform.GetComponent<SpriteBody2D>();
                body.Position = asteroidTransform.position;
                int cellIndex = GetCellIndex(body.Position);
                threadCells[cellIndex].AddBody(body);
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
        Vector2 camPos = Camera.main.transform.position;
        transform.position = camPos;
        BottomLeft = camPos - size / 2f;
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

    public void AddBodyToRespawnInThreadCell(SpriteBody2D body)
    {
        int cellIndex = GetCellIndex(body.Position);
        threadCells[cellIndex].AddBodyToRespawn(body);
    }

    public void AddBodyToThreadCell(SpriteBody2D body)
    {
        int cellIndex = GetCellIndex(body.Position);
        threadCells[cellIndex].AddBody(body);
    }

    public void AddBodyToThreadCell(SpriteBody2D body, int cellIndex)
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