using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CustomGrid : MonoBehaviour
{
    protected Vector2 size;
    protected Vector2 cellSize;
    protected Vector2Int xyCount;
    protected Vector2 bottomLeft;

    public Vector2 BottomLeft
    {
        get => bottomLeft;
        set {
            bottomLeft = value;
        }
    }
    public Vector2Int XYCount { get => xyCount; }
    public Vector2 Size
    {
        get => size;
        set {
            size = value;
        }
    }
    public Vector2 CellSize { get => cellSize; }

    public T CreateInstance<T>(Transform parent, Bounds bounds, Vector2Int xyCount) where T : CustomGrid
    {
        T grid = Instantiate(this as T, parent, false);
        grid.SetParams(bounds, xyCount);
        return grid;
    }

    private void SetParams(Bounds bounds, Vector2Int xyCount)
    {
        //Debug.Log(GetType() + ".SetParams " + bounds + " " + xyCount);
        this.xyCount = xyCount;
        size = bounds.size;
        cellSize = new Vector2(size.x / xyCount.x, size.y / xyCount.y);
        BottomLeft = bounds.min;
        Bounds spriteBounds = GetComponent<SpriteRenderer>().sprite.bounds;
        Vector3 parentLossyScale = transform.parent ? transform.parent.lossyScale : Vector3.one;
        transform.localScale = bounds.size / (parentLossyScale * (Vector2)spriteBounds.size);
    }

    public void SetParams(Vector2 size, Vector2Int xyCount)
    {
        SetParams(new Bounds(Vector2.zero, size), xyCount);
    }


    public int GetCellIndex(Vector2 position)
    {
        float rayX = position.x - bottomLeft.x;
        float rayY = position.y - bottomLeft.y;
        int cellX = (int)(rayX / cellSize.x);
        int cellY = (int)(rayY / cellSize.y);
        if (cellX >= 0 && cellX < xyCount.x && cellY >= 0 && cellY < xyCount.y)
        {
            return cellY * xyCount.x + cellX;
        }
        else
        {
            //Debug.LogWarning(GetType() + "." + " " + bottomLeft + " " + position + " " + rayX / cellSize.x + " " + rayY / cellSize.y);
            //Debug.LogWarning(GetType() + ".GetCellIndex: " + " " + rayX + " " + rayY + " " + " " + cellSize.x + " " + cellSize.y + " " + cellX + " " + cellY);
            return -1;
        }
    }
}
