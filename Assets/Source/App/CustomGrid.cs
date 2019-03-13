using Mindpower;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CustomGrid : MonoBehaviour
{
    protected Bounds2 bounds;
    protected Vector2 cellSize;
    protected Vector2Int xyCount;

    public Vector2 BottomLeft
    {
        get => bounds.Min;
        set {
            bounds.Min = value;
        }
    }

    public Vector2Int XYCount => xyCount;

    public Vector2 Size => bounds.Size;

    public Vector2 Center => bounds.Center;

    public Vector2 CellSize => cellSize;

    public Bounds2 Bounds => bounds;

    public T CreateInstance<T>(Transform parent, Bounds2 bounds, Vector2Int xyCount) where T : CustomGrid
    {
        T grid = Instantiate(this as T, parent, false);
        grid.SetParams(bounds, xyCount);
        return grid;
    }

    private void SetParams(Bounds2 bounds, Vector2Int xyCount)
    {
        //Debug.Log(GetType() + ".SetParams " + bounds + " " + xyCount);
        this.xyCount = xyCount;
        this.bounds = bounds;
        cellSize = new Vector2(Size.x / xyCount.x, Size.y / xyCount.y);
        Bounds2 spriteBounds = Bounds2.BoundsToBounds2(GetComponent<SpriteRenderer>().sprite.bounds);
        Vector3 parentLossyScale = transform.parent ? transform.parent.lossyScale : Vector3.one;
        transform.localScale = bounds.Size / (parentLossyScale * (Vector2)spriteBounds.Size);
    }

    public void SetParams(Vector2 size, Vector2Int xyCount)
    {
        SetParams(new Bounds2(Vector2.zero, size), xyCount);
    }

    public int GetCellIndex(Vector2 position)
    {
        float rayX = position.x - bounds.Min.x;
        float rayY = position.y - bounds.Min.y;
        int cellX = Mathf.Clamp((int)(rayX / cellSize.x), 0, xyCount.x - 1);
        int cellY = Mathf.Clamp((int)(rayY / cellSize.y), 0, xyCount.y - 1);
        //Debug.LogWarning(GetType() + "." + " " + BottomLeft + " " + position + " " + rayX / cellSize.x + " " + rayY / cellSize.y);
        //Debug.LogWarning(GetType() + ".GetCellIndex: " + " " + rayX + " " + rayY + " " + " " + cellSize.x + " " + cellSize.y + " " + cellX + " " + cellY);
        return cellY * xyCount.x + cellX;
    }
}
