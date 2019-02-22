using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ThreadCell : CustomCell
{
    private CellThread thread;

    private CollisionGrid collGrid;

    public ThreadCell CreateInstance(ThreadGrid threadGrid, Vector2Int cellXY, out EventWaitHandle threadWait)
    {
        ThreadCell cell = CreateInstance<ThreadCell>(threadGrid, cellXY);
        cell.thread = new CellThread(cell, threadGrid, out threadWait);
        return cell;
    }

    public void CreateCollisionGrid(int cellSize, CollisionGrid collGridPrefab, CollisionCell collCellPrefab)
    {
        collGrid = thread.CollGrid = collGridPrefab.CreateInstance(this, collCellPrefab, cellSize);
    }

    public void StartThread()
    {
        thread.StartThread();
    }

    public void PreSyncThread()
    {
        thread.PreSyncThread();
    }

    public void SyncThread()
    {
        bounds.center = transform.position;
        thread.SyncThread();
    }

    public void RequestStop()
    {
        thread.RequestStopThread();
    }

    public void SyncStopRequest()
    {
        thread.SyncStopRequest();
    }

    public void AddBodyToRespawn(SpriteBody2D body)
    {
        thread.AddBodyToRespawn(body);
    }

    public void AddBody(SpriteBody2D body)
    {
        thread.AddBody(body);
    }

    public void ResumeThread()
    {
        thread.ResumeThread();
    }

    //public void ClearThreadData()
    //{
    //    thread.ClearThreadData();
    //}

}
