using System.Collections.Generic;
using UnityEngine;

public class CollisionGrid : CustomGrid
{
    private readonly Color[] cellColors = new Color[] { Color.red, Color.green, Color.blue, Color.yellow, Color.cyan, Color.white };

    private CollisionCell[] cells;

    protected List<CollidedPair> collidedPairs = new List<CollidedPair>();

    private Bounds cameraBounds;

    public List<CollidedPair> CollidedPairs { get => collidedPairs; }

    public Bounds CameraBounds { get => cameraBounds; set => cameraBounds = value; }

    public CollisionCell[] Cells { get => cells; }

    public CollisionGrid CreateInstance(CustomCell parentCell, CollisionCell collCellPrefab, int cellSize)
    {
        Bounds bounds = parentCell.Bounds;
        //Debug.Log(GetType() + ".CollisionGrid: " + bounds.center + " " + bounds.size + " " + cellSize + " " + parentCell.MiddleAnchor);
        int xCount = Mathf.Max(1, (int)(bounds.size.x / cellSize));
        int yCount = Mathf.Max(1, (int)(bounds.size.y / cellSize));
        Vector2Int xyCount = new Vector2Int(xCount, yCount);
        CollisionGrid grid = CreateInstance<CollisionGrid>(parentCell.transform, bounds, xyCount);
        grid.cells = new CollisionCell[xCount * yCount];
        //Debug.Log(GetType() + ".CollisionGrid: " + grid.BottomLeft + " " + grid.Size + " " + grid.cellSize + " " + grid.xyCount);
        for (int y = 0; y < yCount; y++)
        {
            for (int x = 0; x < xCount; x++)
            {
                int cellIndex = y * xCount + x;
                grid.cells[cellIndex] = collCellPrefab.CreateInstance(parentCell, grid, new Vector2Int(x, y), cellColors);
            }
        }
        return grid;
    }

    public void AddCollidedPair(RawBody2D bodyA, RawBody2D bodyB)
    {
        collidedPairs.Add(new CollidedPair(bodyA, bodyB));
    }

    public bool AddBodyToCell(RawBody2D body)
    {
        //return;
        int cellIndex = GetCellIndex(body.Position);
        if (cellIndex >= 0)
        {
            cells[cellIndex].AddBody(body);
            return true;
        }
        return false;
    }

    public void RemoveBodyFromCell(RawBody2D body)
    {
        //return;
        if (body.collCellIndex < 0)
            body.collCellIndex = GetCellIndex(body.Position);
        if (body.collCellIndex >= 0)
            cells[body.collCellIndex].RemoveBody(body);
    }

    private void UpdateBounds(Bounds parentBounds, Bounds cameraBounds)
    {
        this.cameraBounds = cameraBounds;
        bounds = parentBounds;
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].UpdateBounds();
        }
    }

    public void UpdateGridInBounds(Bounds parentBounds, Bounds cameraBounds)
    {
        UpdateBounds(parentBounds, cameraBounds);
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].AddBodiesInCameraView();
            cells[i].AddBodiesOutOfCellBounds();
        }
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].ReassignBodiesOutOfCellBounds();
        }
    }

    public void UpdateCollisions()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].UpdateCollisions();
        }
    }

    public void GetBodiesInCameraView(out List<RawBody2D> list)
    {
        list = new List<RawBody2D>();
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].GetBodiesInCameraView(list);
        }
    }
}
