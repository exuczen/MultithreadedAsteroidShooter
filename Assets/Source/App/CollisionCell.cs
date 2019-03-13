using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using RawPhysics;
using Mindpower;

public class CollisionCell : CustomCell
{
    public const int DefaultLayer = Const.LayerDefault;

    private static int colorIndex = 0;

    private List<RawBody2D> bodies = new List<RawBody2D>();

    private List<RawBody2D> bodiesOutOfBounds = new List<RawBody2D>();

    private List<RawBody2D> bodiesInCameraView = new List<RawBody2D>();

    private Dictionary<RawColliderShape2D, List<RawCollider2D>> collidersDict = new Dictionary<RawColliderShape2D, List<RawCollider2D>>();

    private List<RawCircleCollider2D> circleColliders = new List<RawCircleCollider2D>();

    private List<RawBoxCollider2D> boxColliders = new List<RawBoxCollider2D>();

    private List<RawTriangleCollider2D> triangleColliders = new List<RawTriangleCollider2D>();

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

        CollisionCell cell = CreateInstance<CollisionCell>(grid, cellXY);
        cell.color = colors[(colorIndex++) % colors.Length];
        cell.collGrid = grid;
        if (xCount % 2 == 1 && yCount % 2 == 1)
        {
            int midX = Math.Max(0, (xCount - 1) >> 1);
            int midY = Math.Max(0, (yCount - 1) >> 1);
            int midPX = Math.Max(0, (parentCell.Grid.XYCount.x - 1) >> 1);
            int midPY = Math.Max(0, (parentCell.Grid.XYCount.y - 1) >> 1);
            cell.isMiddle = pCellX == midPX && pCellY == midPY && x == midX && y == midY;
            //Debug.LogWarning(GetType() + "." + midX + " " + midY + " " + " " + midPX + " " + midPY + " " + x + " " + y + " " + pCellX + " " + pCellY + " " + cell.isMiddle);
        }
        else if (xCount % 2 == 0 && yCount % 2 == 0)
        {
            int pX = parentCell.XY.x - Math.Max(1, parentCell.Grid.XYCount.x >> 1) + 1;
            int pY = parentCell.XY.y - Math.Max(1, parentCell.Grid.XYCount.y >> 1) + 1;
            cell.isMiddle =
            (pX == 0 && pY == 0 && x == xCount - 1 && y == yCount - 1) ||
            (pX == 1 && pY == 0 && x == 0 && y == yCount - 1) ||
            (pX == 0 && pY == 1 && x == xCount - 1 && y == 0) ||
            (pX == 1 && pY == 1 && x == 0 && y == 0);
            //parentCell.IsMiddle = (pX == 0 && pY == 0) || (pX == 1 && pY == 0) || (pX == 0 && pY == 1) || (pX == 1 && pY == 1);
        }
        //Debug.LogWarning(GetType() + ".isMiddle=" + isMiddle);
        if (cell.isMiddle)
        {
            cell.GetComponent<SpriteRenderer>().color = Color.black;
            //cell.GetComponent<SpriteRenderer>().enabled = true;
        }
        //if (parentCell.IsMiddle)
        //{
        //    parentCell.GetComponent<SpriteRenderer>().color = Color.black;
        //    //parentCell.GetComponent<SpriteRenderer>().enabled = true;
        //}

        foreach (RawColliderShape2D shape in RawPhysics2D.ColliderShapes)
        {
            cell.collidersDict.Add(shape, new List<RawCollider2D>());
        }

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
        bodies.Add(body);
    }

    public void AddBodiesInCameraView()
    {
        Bounds2 cameraBounds = collGrid.CameraBounds;
        if (isMiddle || bounds.Size.y < cameraBounds.Size.y)
        {
            bodiesInCameraView.Clear();
            //Debug.LogWarning(GetType() + "." + cameraBounds.Size);
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

    private void UpdateCollisions<T1, T2>(List<T1> collidersA, List<T2> collidersB, Func<T1, T2, Vector2, bool> overlap)
        where T1 : RawCollider2D where T2 : RawCollider2D
    {
        for (int i = 0; i < collidersA.Count; i++)
        {
            T1 colliderA = collidersA[i];
            RawBody2D bodyA = colliderA.Body;
            //bodyA.SetSpriteColor(Color.white);

            for (int j = 0; j < collidersB.Count; j++)
            {
                T2 colliderB = collidersB[j];
                RawBody2D bodyB = colliderB.Body;
                if (bodyA.BoundsOverlap(bodyB, out Vector2 bodiesRay) &&
                    overlap(colliderA, colliderB, bodiesRay))
                {
                    //bodyA.SetSpriteColor(Color.red);
                    collGrid.AddCollidedPair(bodyA, bodyB);
                }
            }
        }
    }

    private void UpdateCollisions<T1>(List<T1> colliders, Func<T1, T1, Vector2, bool> overlap)
        where T1 : RawCollider2D
    {
        //Debug.LogWarning(GetType() + "." + colliders.Count + " " + colliders.Count * (colliders.Count - 1) / 2);
        for (int i = 0; i < colliders.Count; i++)
        {
            T1 colliderA = colliders[i];
            RawBody2D bodyA = colliderA.Body;
            for (int j = i + 1; j < colliders.Count; j++)
            {
                T1 colliderB = colliders[j];
                RawBody2D bodyB = colliderB.Body;
                if (bodyA.BoundsOverlap(bodyB, out Vector2 bodiesRay) &&
                    overlap(colliderA, colliderB, bodiesRay)
                    )
                {
                    collGrid.AddCollidedPair(bodyA, bodyB);
                }
            }
        }
    }

    private List<T> GetCollidersOfType<T>(RawColliderShape2D shape) where T : RawCollider2D
    {
        return collidersDict[shape].Cast<T>().ToList();

        //List<RawCollider2D> rawColliders = collidersDict[shape];
        //List<T> colliders = new List<T>();
        //foreach (var rawCollider in rawColliders)
        //{
        //    colliders.Add(rawCollider as T);
        //}
        //return colliders;
    }

    private void ClearColliders()
    {
        circleColliders.Clear();
        boxColliders.Clear();
        triangleColliders.Clear();
        foreach (var kvp in collidersDict)
        {
            kvp.Value.Clear();
        }
    }

    public void UpdateCollisions()
    {
        ClearColliders();
        for (int i = 0; i < bodies.Count; i++)
        {
            RawBody2D body = bodies[i];
            collidersDict[body.ColliderShape].Add(body.Collider);
        }
        circleColliders = GetCollidersOfType<RawCircleCollider2D>(RawColliderShape2D.Circle);

        UpdateCollisions(circleColliders, RawPhysics2D.CirclesWithBasicRadiusOverlap);

        if (isMiddle)
        {
            triangleColliders = GetCollidersOfType<RawTriangleCollider2D>(RawColliderShape2D.Triangle);
            boxColliders = GetCollidersOfType<RawBoxCollider2D>(RawColliderShape2D.Box);
            //Debug.LogWarning(GetType() + "." + triangleColliders.Count);

            UpdateCollisions(triangleColliders, circleColliders, RawPhysics2D.TriangleCircleOverlap);
            UpdateCollisions(boxColliders, circleColliders, RawPhysics2D.BoxCircleOverlap);
        }
    }

    public void GetBodiesInCameraView(List<RawBody2D> list)
    {
        Bounds2 cameraBounds = collGrid.CameraBounds;
        if (isMiddle || bounds.Size.y < cameraBounds.Size.y)
        {
            list.AddRange(bodiesInCameraView);
        }
    }

    public void Clear()
    {
        bodies.Clear();
        bodiesOutOfBounds.Clear();
        bodiesInCameraView.Clear();
        ClearColliders();
    }
}
