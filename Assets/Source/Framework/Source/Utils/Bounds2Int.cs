using System;
using UnityEngine;

public struct Bounds2Int : IEquatable<Bounds2Int>
{
    private Vector2Int size;
    private Vector2Int halfSize;
    private Vector2Int center;
    private Vector2Int min;
    private Vector2Int max;

    public Bounds2Int(Vector2Int center, Vector2Int size)
    {
        this.center = center;
        this.size = size;
        halfSize = new Vector2Int(size.x >> 1, size.y >> 1);
        min = center - halfSize;
        max = center + halfSize;
    }

    public Bounds2Int GetScaledBounds(int bitShift)
    {
        Bounds2Int bounds = new Bounds2Int
        {
            size = new Vector2Int(size.x << bitShift, size.y << bitShift),
            halfSize = new Vector2Int(halfSize.x << bitShift, halfSize.y << bitShift),
            center = new Vector2Int(center.x << bitShift, center.y << bitShift),
            min = new Vector2Int(min.x << bitShift, min.y << bitShift),
            max = new Vector2Int(max.x << bitShift, max.y << bitShift)
        };
        return bounds;
    }

    public Vector2Int Size
    {
        get => size;
        set {
            size = value;
            halfSize = new Vector2Int(size.x >> 1, size.y >> 1);
            min = center - halfSize;
            max = center + halfSize;
        }
    }
    public Vector2Int HalfSize
    {
        get => halfSize;
        set {
            halfSize = value;
            size = new Vector2Int(halfSize.x << 1, halfSize.y << 1);
            min = center - halfSize;
            max = center + halfSize;
        }
    }
    public Vector2Int Center
    {
        get => center;
        set {
            center = value;
            min = center - halfSize;
            max = center + halfSize;
        }
    }
    public Vector2Int Min
    {
        get => min;
        set {
            min = value;
            max = min + size;
            center = min + halfSize;
        }
    }

    public Vector2Int Max
    {
        get => min;
        set {
            max = value;
            min = max - size;
            center = max - halfSize;
        }
    }

    public bool Contains(Vector2Int scaledIntPos)
    {
        int dx = scaledIntPos.x - center.x;
        int dy = scaledIntPos.y - center.y;
        return Math.Abs(dx) <= halfSize.x && Math.Abs(dy) <= halfSize.y;
    }

    public bool Contains(Vector2 pos)
    {
        float dx = pos.x - center.x;
        float dy = pos.y - center.y;
        return Mathf.Abs(dx) <= halfSize.x && Mathf.Abs(dy) <= halfSize.y;
    }


    public bool Equals(Bounds2Int other)
    {
        return center == other.center && size == other.size;
    }
}