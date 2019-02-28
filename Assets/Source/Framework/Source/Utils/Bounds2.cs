using System;
using UnityEngine;

public struct Bounds2 : IEquatable<Bounds2>
{
    private Vector2 size;
    private Vector2 extents;
    private Vector2 center;
    private Vector2 min;
    private Vector2 max;

    public Bounds2(Vector2 center, Vector2 size)
    {
        this.center = center;
        this.size = size;
        extents = new Vector2(size.x / 2f, size.y / 2f);
        min = center - extents;
        max = center + extents;
    }

    public Vector2 Size
    {
        get => size;
        set {
            size = value;
            extents = new Vector2(size.x / 2f, size.y / 2f);
            min = center - extents;
            max = center + extents;
        }
    }

    public Vector2 Extents
    {
        get => extents;
        set {
            extents = value;
            size = new Vector2(extents.x * 2f, extents.y * 2f);
            min = center - extents;
            max = center + extents;
        }
    }

    public Vector2 Center
    {
        get => center;
        set {
            center = value;
            min = center - extents;
            max = center + extents;
        }
    }

    public Vector2 Min
    {
        get => min;
        set {
            min = value;
            max = min + size;
            center = min + extents;
        }
    }

    public Vector2 Max
    {
        get => min;
        set {
            max = value;
            min = max - size;
            center = max - extents;
        }
    }

    public bool Contains(Vector2 pos)
    {
        float dx = pos.x - center.x;
        float dy = pos.y - center.y;
        return Mathf.Abs(dx) <= extents.x && Mathf.Abs(dy) <= extents.y;
    }

    public bool Equals(Bounds2 other)
    {
        return center == other.center && size == other.size;
    }

    public static Bounds2 BoundsToBounds2(Bounds bounds)
    {
        return new Bounds2(bounds.center, bounds.size);
    }
}