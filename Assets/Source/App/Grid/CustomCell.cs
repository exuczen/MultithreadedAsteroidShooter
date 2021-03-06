﻿using MustHave;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CustomCell : MonoBehaviour
{
    protected Vector2Int xy = default;
    protected int index = default;
    protected Bounds2 bounds = default;
    protected CustomGrid grid = default;
    protected bool isMiddle = default;

    public Bounds2 Bounds { get => bounds; set => bounds = value; }
    public int Index { get => index; set => index = value; }
    public bool IsMiddle { get => isMiddle; set => isMiddle = value; }
    public Vector2Int XY { get => xy; set => xy = value; }
    public CustomGrid Grid { get => grid; }

    public T CreateInstance<T>(CustomGrid grid, Vector2Int cellXY) where T : CustomCell
    {
        T cell = Instantiate(this as T, grid.transform, false);
        cell.Initialize(grid, cellXY);
        return cell;
    }

    private void Initialize(CustomGrid grid, Vector2Int cellXY)
    {
        this.grid = grid;
        this.xy = cellXY;
        this.index = cellXY.y * grid.XYCount.x + cellXY.x;
        this.UpdateBounds();
        Bounds2 spriteBounds = Bounds2.BoundsToBounds2(GetComponent<SpriteRenderer>().sprite.bounds);
        Vector3 parentLossyScale = transform.parent ? transform.parent.lossyScale : Vector3.one;
        transform.localScale = bounds.Size / (parentLossyScale * (Vector2)spriteBounds.Size);
        transform.position = new Vector3(bounds.Center.x, bounds.Center.y, 0f);
    }

    public void UpdateBounds()
    {
        float bottomLeftCellCenterX = grid.BottomLeft.x + (grid.CellSize.x / 2f);
        float bottomLeftCellCenterY = grid.BottomLeft.y + (grid.CellSize.y / 2f);
        bounds.Center = new Vector2(
                bottomLeftCellCenterX + xy.x * grid.CellSize.x,
                bottomLeftCellCenterY + xy.y * grid.CellSize.y);
        bounds.Size = grid.CellSize;
    }
}
