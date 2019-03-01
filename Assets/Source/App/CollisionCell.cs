using System;
using System.Collections.Generic;
using UnityEngine;
using DC;

public class CollisionCell : CustomCell
{
    public const int CollisionLayer = Const.LayerAsteroid;

    public const int DefaultLayer = Const.LayerDefault;

    private static int colorIndex = 0;

    private List<RawBody2D> bodies = new List<RawBody2D>();

    private List<RawBody2D> bodiesOutOfBounds = new List<RawBody2D>();

    private List<RawBody2D> bodiesInCameraView = new List<RawBody2D>();

    private CollisionGrid collGrid;

    private Color color;

    public int BodyCount { get => bodies.Count; }

    public List<RawBody2D> BodiesInCameraView { get => bodiesInCameraView; }

    public CollisionCell CreateInstance(CustomCell parentCell, CollisionGrid grid, Vector2Int cellXY, Color[] colors)
    {
        int xCount = grid.XYCount.x;
        int yCount = grid.XYCount.y;
        int x = cellXY.x;
        int y = cellXY.y;
        int pCellX = parentCell.XY.x;
        int pCellY = parentCell.XY.y;
        int pX = parentCell.XY.x - Math.Max(1, parentCell.Grid.XYCount.x >> 1) + 1;
        int pY = parentCell.XY.y - Math.Max(1, parentCell.Grid.XYCount.y >> 1) + 1;

        CollisionCell cell = CreateInstance<CollisionCell>(grid, cellXY);
        cell.color = colors[(colorIndex++) % colors.Length];
        cell.collGrid = grid;
        cell.isMiddle =
            (pX == 0 && pY == 0 && x == xCount - 1 && y == yCount - 1) ||
            (pX == 1 && pY == 0 && x == 0 && y == yCount - 1) ||
            (pX == 0 && pY == 1 && x == xCount - 1 && y == 0) ||
            (pX == 1 && pY == 1 && x == 0 && y == 0);
        if (cell.isMiddle)
        {
            cell.GetComponent<SpriteRenderer>().color = Color.black;
            //cell.GetComponent<SpriteRenderer>().enabled = true;
        }
        //parentCell.IsMiddle = (pX == 0 && pY == 0) || (pX == 1 && pY == 0) || (pX == 0 && pY == 1) || (pX == 1 && pY == 1);
        //if (parentCell.IsMiddle)
        //{
        //    parentCell.GetComponent<SpriteRenderer>().color = Color.black;
        //    //parentCell.GetComponent<SpriteRenderer>().enabled = true;
        //}
        return cell;
    }

    public void RemoveBody(RawBody2D body)
    {
        body.SetSpriteColor(Color.black);
        bodies.Remove(body);
        body.collCellIndex = -1;
    }

    public void AddBody(RawBody2D body)
    {
        //Debug.LogWarning(GetType() + ".AddBody: cellIndex=" + index + " color=" + color + " isMiddle=" + isMiddle);
        body.SetSpriteColor(AppManager.DebugSprites ? color : Color.white);
        body.collCellIndex = index;
#if DEBUG_RIGID_BODY
        body.Layer = isMiddle ? CollisionLayer : DefaultLayer;
#endif
        bodies.Add(body);
    }

    public void AddBodiesInCameraView()
    {
        if (isMiddle)
        {
            bodiesInCameraView.Clear();
            Bounds2 cameraBounds = collGrid.CameraBounds;
            for (int i = 0; i < bodies.Count; i++)
            {
                RawBody2D body = bodies[i];
                body.isInCameraView = cameraBounds.Contains(body.Position);
                if (body.isInCameraView)
                {
                    bodiesInCameraView.Add(body);
                }
            }
        }
    }

    public void AddBodiesOutOfCellBounds()
    {
        for (int i = 0; i < bodies.Count; i++)
        {
            RawBody2D body = bodies[i];
            if (!bounds.Contains(body.Position))
            {
                bodiesOutOfBounds.Add(body);
            }
        }
    }

    public void ReassignBodiesOutOfCellBounds()
    {
        for (int i = 0; i < bodiesOutOfBounds.Count; i++)
        {
            RawBody2D body = bodiesOutOfBounds[i];
            RemoveBody(body);
            collGrid.AddBodyToCell(body);
        }
        bodiesOutOfBounds.Clear();
    }

    public void UpdateCollisions()
    {
        for (int i = 0; i < bodies.Count; i++)
        {
            RawBody2D bodyA = bodies[i];
            for (int j = i + 1; j < bodies.Count; j++)
            {
                RawBody2D bodyB = bodies[j];
                if (bodyA.BoundsOverlap(bodyB, out int dx, out int dy) &&
                    bodyA.RadiusOverlap(bodyB, dx, dy))
                {
                    collGrid.AddCollidedPair(bodies[i], bodies[j]);
                }
            }
        }
    }

    public void GetBodiesInCameraView(List<RawBody2D> list)
    {
        if (isMiddle)
        {
            list.AddRange(bodiesInCameraView);
        }
    }

}
