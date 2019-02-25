using System;
using UnityEngine;

public abstract class RawBody2D : MonoBehaviour
{
    [HideInInspector]
    public int threadCellIndex;
    [HideInInspector]
    public int collCellIndex;

    private Vector2 position;
    private Vector2 velocity;
    private Vector2Int positionInt;
    private Vector3 eulerAngles;
    private float angularVelocity;
    private SpriteRenderer spriteRenderer;
    protected Rigidbody2D rigidBody;
    protected new Collider2D collider;
    protected Bounds bounds;
    private Vector2Int sizeInt;
    private Vector2Int halfSizeInt;
    private int radiusInt;
    private Color spriteColor;
    protected float respawnTime;
    protected int layer;

    public Bounds Bounds { get => bounds; }
    public float RespawnTime { get => respawnTime; set => respawnTime = value; }
    public Collider2D Collider { get => collider; }
    public Rigidbody2D RigidBody { get => rigidBody; }
    public int Layer { get => layer; set { layer = value; } }

    public T CreateInstance<T>(Transform parent, Vector3 position, float r) where T : RawBody2D
    {
        T body = Instantiate(this as T, position, Quaternion.identity, parent);
        body.Initialize(r);
        return body;
    }

    public void Initialize(float r)
    {
        layer = Const.LayerDefault;
        rigidBody = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
        spriteRenderer = transform.GetComponent<SpriteRenderer>();
        SetMotionDataFromTransform();
        SetSpriteData(r);
    }

    public Vector2 Position
    {
        get => position;
        set {
            position = value;
            positionInt.x = (int)(position.x * Const.FloatToIntFactor);
            positionInt.y = (int)(position.y * Const.FloatToIntFactor);
        }
    }

    public Vector2Int PositionInt
    {
        get => positionInt;
    }
    
    public void ResetData()
    {
        spriteColor = spriteRenderer.color = Color.black;
        threadCellIndex = -1;
        collCellIndex = -1;
    }

    public void SetVelocities(Vector2 velocity, float angularVelocity)
    {
        this.velocity = velocity;
        this.angularVelocity = angularVelocity;
    }

    public void SetMotionDataFromTransform()
    {
        position = transform.position;
        eulerAngles.z = transform.eulerAngles.z;
    }

    public void SetSpriteData(float radiusNormalized)
    {
        bounds = spriteRenderer.bounds;
        bounds.size *= 1f;
        bounds.center = position;
        spriteColor = spriteRenderer.color;
        sizeInt.x = (int)(bounds.size.x * Const.FloatToIntFactor);
        sizeInt.y = (int)(bounds.size.y * Const.FloatToIntFactor);
        halfSizeInt.x = sizeInt.x >> 1;
        halfSizeInt.y = sizeInt.y >> 1;
        radiusInt = (int)(radiusNormalized * Mathf.Max(bounds.extents.x, bounds.extents.y) * Const.FloatToIntFactor);
    }

    public bool RadiusOverlap(RawBody2D body, int dxInt, int dyInt)
    {
        int radiusIntSum = radiusInt + body.radiusInt;
        return dxInt * dxInt + dyInt * dyInt <= radiusIntSum * radiusIntSum;
    }

    public bool BoundsOverlap(RawBody2D body, out int dxInt, out int dyInt)
    {
        //dxInt = 0;
        //dyInt = 0;
        //return bounds.Intersects(body.bounds);
        dxInt = body.positionInt.x - positionInt.x;
        dyInt = body.positionInt.y - positionInt.y;
        return
            Math.Abs(dxInt) < body.halfSizeInt.x + halfSizeInt.x &&
            Math.Abs(dyInt) < body.halfSizeInt.y + halfSizeInt.y;
    }

    public void UpdateMotionData(float deltaTime)
    {
        position += velocity * deltaTime;
        eulerAngles.z += angularVelocity * deltaTime;
        bounds.center = position;
        positionInt.x = (int)(position.x * Const.FloatToIntFactor);
        positionInt.y = (int)(position.y * Const.FloatToIntFactor);
    }

    public void SetMainThreadData()
    {
        transform.position = position;
        transform.eulerAngles = eulerAngles;
        spriteRenderer.color = spriteColor;
        if (layer != gameObject.layer)
        {
            gameObject.layer = layer;
            bool collisionLayer = layer == CollisionCell.CollisionLayer;
            rigidBody.simulated = collisionLayer;
            collider.enabled = collisionLayer;
        }
    }

    public void SetSpriteColor(Color c)
    {
        spriteColor = c;
    }

    public abstract void Explode(Vector2 explosionPos);

    public abstract void Respawn();
}
