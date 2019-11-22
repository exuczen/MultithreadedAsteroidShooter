using System;
using UnityEngine;

namespace MustHave
{
    public struct Bounds2Int : IEquatable<Bounds2Int>
    {
        private Vector2Int size;
        private Vector2Int extents;
        private Vector2Int center;
        private Vector2Int min;
        private Vector2Int max;

        public Bounds2Int(Vector2Int center, Vector2Int size)
        {
            this.center = center;
            this.size = size;
            extents = new Vector2Int(size.x >> 1, size.y >> 1);
            min = center - extents;
            max = center + extents;
        }

        public Bounds2Int GetScaledBounds(int bitShift)
        {
            Bounds2Int bounds = new Bounds2Int {
                size = new Vector2Int(size.x << bitShift, size.y << bitShift),
                extents = new Vector2Int(extents.x << bitShift, extents.y << bitShift),
                center = new Vector2Int(center.x << bitShift, center.y << bitShift),
                min = new Vector2Int(min.x << bitShift, min.y << bitShift),
                max = new Vector2Int(max.x << bitShift, max.y << bitShift)
            };
            return bounds;
        }

        public Vector2Int Size
        {
            get { return size; }
            set {
                size = value;
                extents = new Vector2Int(size.x >> 1, size.y >> 1);
                min = center - extents;
                max = center + extents;
            }
        }
        public Vector2Int Extents
        {
            get { return extents; }
            set {
                extents = value;
                size = new Vector2Int(extents.x << 1, extents.y << 1);
                min = center - extents;
                max = center + extents;
            }
        }
        public Vector2Int Center
        {
            get { return center; }
            set {
                center = value;
                min = center - extents;
                max = center + extents;
            }
        }
        public Vector2Int Min
        {
            get { return min; }
            set {
                min = value;
                max = min + size;
                center = min + extents;
            }
        }

        public Vector2Int Max
        {
            get { return min; }
            set {
                max = value;
                min = max - size;
                center = max - extents;
            }
        }

        public bool Contains(Vector2Int scaledIntPos)
        {
            int dx = scaledIntPos.x - center.x;
            int dy = scaledIntPos.y - center.y;
            return Math.Abs(dx) <= extents.x && Math.Abs(dy) <= extents.y;
        }

        public bool Contains(Vector2 pos)
        {
            float dx = pos.x - center.x;
            float dy = pos.y - center.y;
            return Mathf.Abs(dx) <= extents.x && Mathf.Abs(dy) <= extents.y;
        }


        public bool Equals(Bounds2Int other)
        {
            return center == other.center && size == other.size;
        }
    }
}