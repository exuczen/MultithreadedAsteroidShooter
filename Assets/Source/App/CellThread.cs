using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class CellThread : SyncedThread
{
    private List<RawBody2D> bodies = new List<RawBody2D>();

    private List<RawBody2D> bodiesOutOfBounds = new List<RawBody2D>();

    private List<RawBody2D> bodiesToRespawn = new List<RawBody2D>();

    private ThreadGrid grid;

    private ThreadCell cell;

    private Bounds2 cellBounds;

    private Bounds2 cameraBounds;

    private CollisionGrid collGrid;

    public CollisionGrid CollGrid { set => collGrid = value; }

    private Vector2 gridSize;
    private Vector2 gridBottomLeft;
    private Vector2Int gridXYCount;

    private bool running;

    private void CopyParentData()
    {
        cellBounds = cell.Bounds;
        cameraBounds = cell.CameraBounds;
        gridSize = grid.Size;
        gridBottomLeft = grid.BottomLeft;
        gridXYCount = grid.XYCount;
        //Debug.LogWarning(GetType() + ".**");
        //Debug.LogWarning(GetType() + "." + gridSize + " " + grid.ScaledSize);
        //Debug.LogWarning(GetType() + "." + gridBottomLeft + " " + grid.ScaledBottomLeft);
    }

    public CellThread(ThreadCell cell, ThreadGrid grid, out EventWaitHandle threadWait) : base(out threadWait)
    {
        this.cell = cell;
        this.grid = grid;
        CopyParentData();
    }

    public override void PreSyncThread()
    {
        float currTime = Time.time;
        int bodyIndex = 0;
        while (bodyIndex < bodiesToRespawn.Count && bodiesToRespawn[bodyIndex].RespawnTime <= currTime)
        {
            RawBody2D body = bodiesToRespawn[bodyIndex];
            body.Respawn();
            grid.AddBodyToThreadCell(body);
            bodyIndex++;
        }
        int respawnedBodiesCount = bodyIndex;
        bodiesToRespawn.RemoveRange(0, respawnedBodiesCount);

        List<CollidedPair> collidedPairs = collGrid.CollidedPairs;
        float respawnTime = currTime + Const.BodyRespawnInterval;
        for (int i = 0; i < collidedPairs.Count; i++)
        {
            CollidedPair pair = collidedPairs[i];
            AddRemovedBodyToRespawn(pair.bodyA, respawnTime);
            AddRemovedBodyToRespawn(pair.bodyB, respawnTime);
            pair.Destroy();
        }
        collidedPairs.Clear();

        for (int i = 0; i < bodiesOutOfBounds.Count; i++)
        {
            RawBody2D body = bodiesOutOfBounds[i];
            grid.AddBodyToThreadCell(body, body.threadCellIndex);
        }
        bodiesOutOfBounds.Clear();
    }

    public override void SyncThread()
    {
#if DEBUG_GAME_OBJECTS
        if (AppManager.DebugGameObjects)
        {
            for (int i = 0; i < bodies.Count; i++)
            {
                bodies[i].SetGameObjectData();
            }
        }
        else
        {
            collGrid.GetBodiesInCameraView(out List<RawBody2D> bodiesInCameraView);
            for (int i = 0; i < bodiesInCameraView.Count; i++)
            {
                bodiesInCameraView[i].SetGameObjectData();
            }
        }
#else
        collGrid.GetBodiesInCameraView(out List<RawBody2D> bodiesInCameraView);
        for (int i = 0; i < bodiesInCameraView.Count; i++)
        {
            bodiesInCameraView[i].SetGameObjectData();
        }
#endif
        running = cell.enabled && cell.gameObject.activeInHierarchy;
        //for (int i = 0; i < collGrid.Cells.Length; i++)
        //{
        //    Debug.Log(GetType() + "[" + i + "].BodyCount: " + collGrid.Cells[i].BodyCount);
        //}
        CopyParentData();
    }

    private void UpdateBodyOutOfBounds(RawBody2D body)
    {
        Vector2 position = body.Position;
        if (!cellBounds.Contains(position))
        {
            Vector2 ray = position - gridBottomLeft;
            if (ray.x < 0 || ray.x >= gridSize.x)
            {
                float dx = -Mathf.Sign(ray.x) * gridSize.x;
                position.x += dx;
                ray.x += dx;
            }
            if (ray.y < 0 || ray.y >= gridSize.y)
            {
                float dy = -Mathf.Sign(ray.y) * gridSize.y;
                position.y += dy;
                ray.y += dy;
            }
            body.Position = position;

            Vector2 cellSize = cellBounds.Size;
            int cellX = ((int)(ray.x / cellSize.x) + gridXYCount.x) % gridXYCount.x;
            int cellY = ((int)(ray.y / cellSize.y) + gridXYCount.y) % gridXYCount.y;
            int cellIndex = cellY * gridXYCount.x + cellX;
            body.threadCellIndex = cellIndex;

            bodiesOutOfBounds.Add(body);
        }
    }

    protected override void UpdateThread(float deltaTime)
    {
        if (!running)
            return;
        for (int i = 0; i < bodies.Count; i++)
        {
            RawBody2D body = bodies[i];
            body.UpdateMotionData(deltaTime);
            UpdateBodyOutOfBounds(body);
        }
        for (int i = 0; i < bodiesOutOfBounds.Count; i++)
        {
            RemoveBody(bodiesOutOfBounds[i]);
        }
        collGrid.UpdateGridInBounds(cellBounds, cameraBounds);
        collGrid.UpdateCollisions();
        RemoveCollidedBodies();
    }

    public void AddBody(RawBody2D body, bool addToCollGrid = true)
    {
        body.threadCellIndex = cell.Index;
        bodies.Add(body);
        if (addToCollGrid)
            collGrid.AddBodyToCell(body);
    }

    private void AddRemovedBodyToRespawn(RawBody2D body, float respawnTime)
    {
        if (!bodiesToRespawn.Contains(body))
        {
            bodiesToRespawn.Add(body);
            body.RespawnTime = respawnTime;
        }
    }

    private void RemoveBody(RawBody2D body)
    {
        bodies.Remove(body);
        collGrid.RemoveBodyFromCell(body);
    }

    public void AddBodyToRespawn(RawBody2D body)
    {
        RemoveBody(body);
        AddRemovedBodyToRespawn(body, body.RespawnTime);
    }

    private void RemoveCollidedBodies()
    {
        List<CollidedPair> collidedPairs = collGrid.CollidedPairs;
        for (int i = 0; i < collidedPairs.Count; i++)
        {
            CollidedPair pair = collidedPairs[i];
            RemoveBody(pair.bodyA);
            RemoveBody(pair.bodyB);
        }
    }

    //public void ClearThreadData()
    //{
    //    bodies.Clear();
    //    bodiesOutOfBounds.Clear();
    //    bodiesToRespawn.Clear();
    //    grid = null;
    //    cell = null;
    //    collGrid = null;
    //}
}
