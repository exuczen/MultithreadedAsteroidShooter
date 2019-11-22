using MustHave;
using RawPhysics;
using System.Threading;
using UnityEngine;

public class ThreadCell : CustomCell
{
    private CellThread thread = default;
    private Bounds2 cameraBounds = default;

    public Bounds2 CameraBounds { get => cameraBounds; set => cameraBounds = value; }
    public int CircleCollidersCount { get => thread.CollGrid.CircleCollidersCount; }

    public ThreadCell CreateInstance(ThreadGrid threadGrid, Vector2Int cellXY, out EventWaitHandle threadWait)
    {
        ThreadCell cell = CreateInstance<ThreadCell>(threadGrid, cellXY);
        cell.thread = new CellThread(cell, threadGrid, out threadWait);
        return cell;
    }

    public void CreateCollisionGrid(float cellSize, CollisionGrid collGridPrefab, CollisionCell collCellPrefab)
    {
        thread.CollGrid = collGridPrefab.CreateInstance(this, collCellPrefab, cellSize);
    }

    public void StartThread()
    {
        thread.StartThread();
    }

    public void PreSyncThread()
    {
        thread.PreSyncThread();
    }

    public void UpdateBounds(Bounds2 cameraBounds)
    {
        bounds.Center = transform.position;
        this.cameraBounds = cameraBounds;
        thread.UpdateBounds();
    }

    public void SyncThread()
    {
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

    public void AddBody(RawBody2D body)
    {
        thread.AddBody(body);
    }

    public void ResumeThread()
    {
        thread.ResumeThread();
    }

    public void Clear()
    {
        thread.Clear();
    }
}
