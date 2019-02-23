using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionCell : CustomCell
{
    public const int CollisionLayer = Const.LayerAsteroid;

    public const int DefaultLayer = Const.LayerDefault;

    private static int colorIndex = 0;

    private List<SpriteBody2D> bodies = new List<SpriteBody2D>();

    private CollisionGrid collGrid;

    private Color color;

    public int BodyCount { get => bodies.Count; }

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

    public void RemoveBody(SpriteBody2D body)
    {
        body.SetSpriteColor(Color.black);
        bodies.Remove(body);
        body.collCellIndex = -1;
    }

    public void AddBody(SpriteBody2D body)
    {
        //Debug.LogWarning(GetType() + ".AddBody: cellIndex=" + index + " color=" + color);
        body.SetSpriteColor(Const.DebugSprites ? color : Color.white);
        body.collCellIndex = index;
        body.Layer = isMiddle ? CollisionLayer : DefaultLayer;
        bodies.Add(body);
    }

    public void UpdateBodiesOutOfCellBounds(List<SpriteBody2D> bodiesOutOfBounds)
    {
        UpdateBounds();
        //return;
        //Bounds2Int scaledBounds = bounds.GetScaledBounds(Const.FloatToIntFactorLog2);
        for (int i = 0; i < bodies.Count; i++)
        {
            SpriteBody2D body = bodies[i];
            if (!bounds.Contains(body.Position))
            {
                RemoveBody(body);
                collGrid.AddBodyToCell(body);
            }
        }
    }

    public void UpdateCollisions()
    {
        for (int i = 0; i < bodies.Count; i++)
        {
            SpriteBody2D bodyA = bodies[i];
            for (int j = i + 1; j < bodies.Count; j++)
            {
                SpriteBody2D bodyB = bodies[j];
                if (bodyA.BoundsOverlap(bodyB, out int dx, out int dy) &&
                    bodyA.RadiusOverlap(bodyB, dx, dy))
                {
                    collGrid.AddCollidedPair(bodies[i], bodies[j]);
                }
            }
        }
    }
}
