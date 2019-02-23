using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CustomCell : MonoBehaviour
{
    protected Vector2Int xy;

    protected int index;

    protected Bounds bounds;

    protected CustomGrid grid;

    protected bool isMiddle;

    public Bounds Bounds { get => bounds; set => bounds = value; }

    public int Index { get => index; set => index = value; }

    public bool IsMiddle { get => isMiddle; set => isMiddle = value; }

    public Vector2Int XY { get => xy; set => xy = value; }

    public CustomGrid Grid { get => grid; }

    public T CreateInstance<T>(CustomGrid grid, Vector2Int cellXY) where T : CustomCell
    {
        T cell = Instantiate(this as T, grid.transform, false);
        cell.Initialize(grid, cellXY);
        //Debug.Log(GetType() + ".CreateInstance" + grid.BottomLeft + " " + grid.Size + " " + grid.CellSize);
        return cell;
    }

    private void Initialize(CustomGrid grid, Vector2Int cellXY)
    {
        this.grid = grid;
        this.xy = cellXY;
        this.index = cellXY.y * grid.XYCount.x + cellXY.x;
        this.UpdateBounds();
        Bounds spriteBounds = GetComponent<SpriteRenderer>().sprite.bounds;
        Vector3 parentLossyScale = transform.parent ? transform.parent.lossyScale : Vector3.one;
        transform.localScale = bounds.size / (parentLossyScale * (Vector2)spriteBounds.size);
        transform.position = new Vector3(bounds.center.x, bounds.center.y, 0f);
    }

    protected void UpdateBounds()
    {
        float bottomLeftCellCenterX = grid.BottomLeft.x + (grid.CellSize.x / 2f);
        float bottomLeftCellCenterY = grid.BottomLeft.y + (grid.CellSize.y / 2f);
        bounds.center = new Vector2(
                bottomLeftCellCenterX + xy.x * grid.CellSize.x,
                bottomLeftCellCenterY + xy.y * grid.CellSize.y);
        bounds.size = grid.CellSize;
    }
}
