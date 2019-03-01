using System;
using UnityEngine;

public abstract class RawBody2D
{
    public int threadCellIndex;
    public int collCellIndex;
    public bool isInCameraView;

    private Vector2 position;
    private Vector2 velocity;
    private Vector2Int positionInt;
    private Vector3 eulerAngles;
    private float angularVelocity;

    protected Transform transform;
    private SpriteRenderer spriteRenderer;
#if DEBUG_RIGID_BODY
    protected Rigidbody2D rigidBody;
    protected Collider2D collider;
    protected int layer;
#endif

    protected Bounds2 bounds;
    private Vector2Int sizeInt;
    private Vector2Int halfSizeInt;
    private int radiusInt;
    private Color spriteColor;
    protected float respawnTime;
    

    public Bounds2 Bounds { get => bounds; }
    public float RespawnTime { get => respawnTime; set => respawnTime = value; }
    public Quaternion Rotation { get => Quaternion.Euler(eulerAngles); }
#if DEBUG_RIGID_BODY
    public int Layer { get => layer; set { layer = value; } }
#endif

    public RawBody2D(Vector2 position, Vector3 eulerAngles, Bounds2 bounds, float radius)
    {
        this.position = position;
        this.eulerAngles = eulerAngles;
        threadCellIndex = -1;
        collCellIndex = -1;
        isInCameraView = false;
        spriteColor = Color.black;
        SetSizeData(bounds, radius);
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

    public Vector2Int PositionInt { get => positionInt; }

    public void SetVelocities(Vector2 velocity, float angularVelocity)
    {
        this.velocity = velocity;
        this.angularVelocity = angularVelocity;
    }

    private void SetSizeData(Bounds2 bounds, float radiusNormalized)
    {
        this.bounds = bounds;
        bounds.Center = position;
        sizeInt.x = (int)(bounds.Size.x * Const.FloatToIntFactor);
        sizeInt.y = (int)(bounds.Size.y * Const.FloatToIntFactor);
        halfSizeInt.x = sizeInt.x >> 1;
        halfSizeInt.y = sizeInt.y >> 1;
        radiusInt = (int)(radiusNormalized * Mathf.Max(bounds.Extents.x, bounds.Extents.y) * Const.FloatToIntFactor);
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
        bounds.Center = position;
        positionInt.x = (int)(position.x * Const.FloatToIntFactor);
        positionInt.y = (int)(position.y * Const.FloatToIntFactor);
    }

    public void SetGameObjectData()
    {
        if (!transform)
            CreateGameObject();

        transform.position = position;
        transform.eulerAngles = eulerAngles;
        spriteRenderer.color = spriteColor;
#if DEBUG_RIGID_BODY
        if (layer != transform.gameObject.layer)
        {
            transform.gameObject.layer = layer;
            bool collisionLayer = layer == CollisionCell.CollisionLayer;
            rigidBody.simulated = collisionLayer;
            collider.enabled = collisionLayer;
        }
#endif
    }

    public void SetSpriteColor(Color c)
    {
        spriteColor = c;
    }

    public void DestroyGameObject()
    {
        if (transform)
        {
            GameObject.Destroy(transform.gameObject);
        }
        transform = null;
        spriteRenderer = null;
#if DEBUG_RIGID_BODY
        rigidBody = null;
        collider = null;
#endif
        isInCameraView = false;
    }

    protected abstract GameObject CreateGameObjectInstance();

    public void CreateGameObject()
    {
        GameObject gameObject = CreateGameObjectInstance();
        transform = gameObject.transform;
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
#if DEBUG_RIGID_BODY
        rigidBody = transform.GetComponent<Rigidbody2D>();
        collider = transform.GetComponent<Collider2D>();
#endif
    }

    public abstract void Explode(Vector2 explosionPos);

    public abstract void Respawn();

}
