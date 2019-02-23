using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class CellThread : SyncedThread
{
    private List<SpriteBody2D> bodies = new List<SpriteBody2D>();

    private List<SpriteBody2D> bodiesOutOfBounds = new List<SpriteBody2D>();

    private List<SpriteBody2D> bodiesToRespawn = new List<SpriteBody2D>();

    private ThreadGrid grid;

    private ThreadCell cell;

    private Bounds cellBounds;

    private CollisionGrid collGrid;

    public CollisionGrid CollGrid { set => collGrid = value; }

    protected Vector2 gridSize;
    protected Vector2 gridBottomLeft;
    protected Vector2Int gridXYCount;

    private bool running;

    private void CopyGridParams()
    {
        cellBounds = cell.Bounds;
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
        CopyGridParams();
    }

    public override void PreSyncThread()
    {
        float currTime = Time.time;
        int bodyIndex = 0;
        while (bodyIndex < bodiesToRespawn.Count && bodiesToRespawn[bodyIndex].RespawnTime <= currTime)
        {
            SpriteBody2D body = bodiesToRespawn[bodyIndex];
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
            SpriteBody2D body = bodiesOutOfBounds[i];
            grid.AddBodyToThreadCell(body, body.threadCellIndex);
        }
        bodiesOutOfBounds.Clear();
    }

    public override void SyncThread()
    {
        for (int i = 0; i < bodies.Count; i++)
        {
            bodies[i].SetMainThreadData();
        }
        running = cell.enabled && cell.gameObject.activeInHierarchy;
        //for (int i = 0; i < collGrid.Cells.Length; i++)
        //{
        //    Debug.Log(GetType() + "[" + i + "].BodyCount: " + collGrid.Cells[i].BodyCount);
        //}
        CopyGridParams();
    }

    private int UpdateBodyOutOfBounds(SpriteBody2D body, int bodyIndex)
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

            Vector2 cellSize = cellBounds.size;
            int cellX = ((int)(ray.x / cellSize.x) + gridXYCount.x) % gridXYCount.x;
            int cellY = ((int)(ray.y / cellSize.y) + gridXYCount.y) % gridXYCount.y;
            int cellIndex = cellY * gridXYCount.x + cellX;
            body.threadCellIndex = cellIndex;

            bodies.RemoveAt(bodyIndex);
            collGrid.RemoveBodyFromCell(body);
            bodiesOutOfBounds.Add(body);
            bodyIndex--;
        }
        return bodyIndex;
    }

    protected override void UpdateThread(float deltaTime)
    {
        if (!running)
            return;
        for (int i = 0; i < bodies.Count; i++)
        {
            SpriteBody2D body = bodies[i];
            body.UpdateMotionData(deltaTime);
            i = UpdateBodyOutOfBounds(body, i);
        }
        collGrid.UpdateGridInBounds(cellBounds, bodiesOutOfBounds);
        collGrid.UpdateCollisions(cellBounds);
        RemoveCollidedBodies();
    }

    public void AddBody(SpriteBody2D body, bool addToCollGrid = true)
    {
        body.threadCellIndex = cell.Index;
        bodies.Add(body);
        if (addToCollGrid)
            collGrid.AddBodyToCell(body);
    }

    private void AddRemovedBodyToRespawn(SpriteBody2D body, float respawnTime)
    {
        if (!bodiesToRespawn.Contains(body))
        {
            bodiesToRespawn.Add(body);
            body.RespawnTime = respawnTime;
        }
    }

    private void RemoveBody(SpriteBody2D body)
    {
        bodies.Remove(body);
        collGrid.RemoveBodyFromCell(body);
    }

    public void AddBodyToRespawn(SpriteBody2D body)
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
