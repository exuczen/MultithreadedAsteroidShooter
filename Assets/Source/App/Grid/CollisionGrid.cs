﻿using RawPhysics;
using System.Collections.Generic;
using UnityEngine;
using MustHave;

public class CollisionGrid : CustomGrid
{
    private readonly Color[] cellColors = new Color[] { Color.red, Color.green, Color.blue, Color.yellow, Color.cyan, Color.white };

    protected List<CollidedPair> collidedPairs = new List<CollidedPair>();
    private CollisionCell[] cells = default;
    private Bounds2 cameraBounds = default;
    private int circleCollidersCount = default;

    public List<CollidedPair> CollidedPairs { get => collidedPairs; }
    public Bounds2 CameraBounds { get => cameraBounds; set => cameraBounds = value; }
    public CollisionCell[] Cells { get => cells; }
    public int CircleCollidersCount { get => circleCollidersCount; }

    public CollisionGrid CreateInstance(CustomCell parentCell, CollisionCell collCellPrefab, float cellSize)
    {
        Bounds2 bounds = parentCell.Bounds;
        float eps = 0.0001f;
        int xCount = Mathf.Max(1, (int)((bounds.Size.x + eps) / cellSize));
        int yCount = Mathf.Max(1, (int)((bounds.Size.y + eps) / cellSize));
        Vector2Int xyCount = new Vector2Int(xCount, yCount);
        CollisionGrid grid = CreateInstance<CollisionGrid>(parentCell.transform, bounds, xyCount);
        grid.cells = new CollisionCell[xCount * yCount];
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
        if (body.collCellIndex < 0)
            body.collCellIndex = GetCellIndex(body.Position);
        if (body.collCellIndex >= 0)
            cells[body.collCellIndex].RemoveBody(body);
    }

    private void UpdateBounds(Bounds2 parentBounds, Bounds2 cameraBounds)
    {
        this.cameraBounds = cameraBounds;
        bounds = parentBounds;
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].UpdateBounds();
        }
    }

    public void UpdateGridInBounds(Bounds2 parentBounds, Bounds2 cameraBounds)
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
        circleCollidersCount = 0;
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].UpdateCollisions();
            circleCollidersCount += cells[i].CircleCollidersCount;
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

    public void Clear()
    {
        collidedPairs.Clear();
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].Clear();
        }
    }
}
