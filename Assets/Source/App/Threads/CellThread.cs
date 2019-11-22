using RawPhysics;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using MustHave;

public class CellThread : SyncedThread
{
    public const float BODY_RESPAWN_INTERVAL = 1f;

    private readonly List<RawBody2D> bodies = new List<RawBody2D>();
    private readonly List<RawBody2D> bodiesOutOfBounds = new List<RawBody2D>();
    private readonly List<RawBody2D> bodiesToRespawn = new List<RawBody2D>();
    private readonly List<RawBody2D> bodiesOutOfTime = new List<RawBody2D>();

    private readonly ThreadGrid grid = default;
    private readonly ThreadCell cell = default;
    private Bounds2 cellBounds = default;
    private Bounds2 cameraBounds = default;
    private CollisionGrid collGrid = default;

    private Vector2 gridSize = default;
    private Vector2 gridBottomLeft = default;
    private Vector2Int gridXYCount = default;
    private bool running = default;

    public CollisionGrid CollGrid { get => collGrid; set => collGrid = value; }

    public void UpdateBounds()
    {
        cellBounds = cell.Bounds;
        cameraBounds = cell.CameraBounds;
        gridBottomLeft = grid.BottomLeft;
    }

    public CellThread(ThreadCell cell, ThreadGrid grid, out EventWaitHandle threadWait) : base(out threadWait)
    {
        this.cell = cell;
        this.grid = grid;
        gridSize = grid.Size;
        gridXYCount = grid.XYCount;
        UpdateBounds();
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
        float respawnTime = currTime + BODY_RESPAWN_INTERVAL;
        for (int i = 0; i < collidedPairs.Count; i++)
        {
            CollidedPair pair = collidedPairs[i];
            if (pair.bodyA.Respawnable)
                AddRemovedBodyToRespawn(pair.bodyA, respawnTime);
            if (pair.bodyB.Respawnable)
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

        for (int i = 0; i < bodiesOutOfTime.Count; i++)
        {
            bodiesOutOfTime[i].RemoveGameObject();
        }
        bodiesOutOfTime.Clear();
    }

    public override void SyncThread()
    {
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
        running = cell.enabled && cell.gameObject.activeInHierarchy;
    }

    private void UpdateBodyOutOfSpacetime(float time, RawBody2D body)
    {
        if (body.LifeTime > 0f && time - body.LifeStartTime >= body.LifeTime)
        {
            bodiesOutOfTime.Add(body);
        }
        else
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
    }

    protected override void UpdateThread(float time, float deltaTime)
    {
        if (!running)
            return;

        for (int i = 0; i < bodies.Count; i++)
        {
            RawBody2D body = bodies[i];
            body.UpdateMotion(deltaTime);
            UpdateBodyOutOfSpacetime(time, body);
        }
        for (int i = 0; i < bodiesOutOfTime.Count; i++)
        {
            RemoveBody(bodiesOutOfTime[i]);
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

    public void Clear()
    {
        bodies.Clear();
        bodiesOutOfBounds.Clear();
        bodiesOutOfTime.Clear();
        bodiesToRespawn.Clear();
        collGrid.Clear();
    }
}
