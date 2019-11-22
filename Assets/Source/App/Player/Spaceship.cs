//#define DEBUG_TRIANGLE

using MustHave.Utilities;
using MustHave;
using RawPhysics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spaceship : MonoBehaviour
{
    [SerializeField, Range(1.0f, 10.0f)] private float linearVelocityMagnitude = 1f;
    [SerializeField, Range(1.0f, 5.0f)] private float angularVelocityMagnitude = 1f;
    [SerializeField] private bool indestructible = default;
    [SerializeField] private SpriteRenderer spriteRenderer = default;
    [SerializeField] private Animator jetAnimator = default;
    [SerializeField] private Missile missilePrefab = default;
    [SerializeField] private Transform missileContainer = default;
    [SerializeField] private Transform missileLauncher = default;
    [SerializeField] private SpaceshipExplosion explosionPrefab = default;

    private const float LINEAR_VELOCITY = 10f;
    private const float ANGULAR_VELOCITY = 2.8f;

    private RawSpaceship rawSpaceship = default;
    private RawTriangleCollider2D rawCollider = default;
    private Vector2 missileBoxColliderSize = default;
    private Bounds2 missileBounds = default;
    private float lastShotTime = default;
#if DEBUG_TRIANGLE
    private SpriteRenderer[] markers = default;
#endif

    private Vector2 SpriteSize { get => spriteRenderer.size; }
    public SpriteRenderer SpriteRenderer { get => spriteRenderer; }
    public bool Indestructible { get => indestructible; }
    public RawSpaceship RawSpaceship { get => rawSpaceship; }

    private void Awake()
    {
        missilePrefab.gameObject.SetActive(false);
        missileBoxColliderSize = missilePrefab.Size * missilePrefab.transform.lossyScale;
        float missileMaxDim = Mathf.Max(missileBoxColliderSize.x, missileBoxColliderSize.y);
        missileBounds = new Bounds2(Vector2.zero, new Vector2(missileMaxDim, missileMaxDim));

        Bounds2 spriteBounds = Bounds2.BoundsToBounds2(spriteRenderer.bounds);
        Vector2 p0 = transform.position;
        Vector2 p1 = spriteBounds.Center + new Vector2(-spriteBounds.Extents.x, -spriteBounds.Extents.y);
        Vector2 p2 = spriteBounds.Center + new Vector2(spriteBounds.Extents.x, -spriteBounds.Extents.y);
        Vector2 p3 = spriteBounds.Center + new Vector2(0, spriteBounds.Extents.y);
        Vector2[] r = new Vector2[] { p1 - p0, p2 - p0, p3 - p0 };
        float spaceshipMaxDim = 0f;
        for (int i = 0; i < 3; i++)
        {
            spaceshipMaxDim = Mathf.Max(spaceshipMaxDim, r[i].magnitude);
        }
        spaceshipMaxDim *= 2f;

#if DEBUG_TRIANGLE
        markers = new SpriteRenderer[4];
        markers[0] = AppManager.CreateMarker(p0, null, Color.blue);
        markers[1] = AppManager.CreateMarker(p1, null, Color.red);
        markers[2] = AppManager.CreateMarker(p2, null, Color.red);
        markers[3] = AppManager.CreateMarker(p3, null, Color.red);
#endif

        rawSpaceship = new RawSpaceship(this, transform.position, transform.eulerAngles, new Bounds2(Vector2.zero, new Vector2(spaceshipMaxDim, spaceshipMaxDim)));
        rawSpaceship.SetCollider(rawCollider = new RawTriangleCollider2D(r[0], r[1], r[2]));
        rawSpaceship.SetGameObject();
    }

    public void UpdateSpaceship()
    {
        float inputY = Input.GetAxis("Vertical");
        float inputX = Input.GetAxis("Horizontal");

        float angularVelocity = -inputX * ANGULAR_VELOCITY * angularVelocityMagnitude;
        Vector2 linearVelocity = inputY * transform.up * LINEAR_VELOCITY * linearVelocityMagnitude;

        rawSpaceship.SetVelocities(linearVelocity, angularVelocity);

        jetAnimator.gameObject.SetActive(inputY > 0f);
#if DEBUG_TRIANGLE
        markers[0].transform.position = rawSpaceship.Position;
        for (int i = 0; i < 3; i++)
        {
            markers[i + 1].transform.position = rawCollider.Verts[i];
        }
#endif
    }

    public void UpdateShooting()
    {
        if (gameObject.activeSelf && Time.time - lastShotTime >= 0.5f)
        {
            Shoot();
            lastShotTime = Time.time;
        }
    }

    private void Shoot()
    {
        Vector2 linearVelocity = rawSpaceship.Velocity;
        float angularVelocity = rawSpaceship.AngularVelocity;
        float velocitySign = Mathf.Sign(Vector2.Dot(linearVelocity, transform.up));

        missileLauncher.localRotation = Quaternion.Euler(0, 0, angularVelocity * Mathf.Rad2Deg * Time.deltaTime);
        Vector2 missileRay = missilePrefab.transform.localPosition;
        Vector2 missileLauncherRay = missileLauncher.localPosition.y * missileLauncher.transform.lossyScale.y * transform.up;
        Vector2 rotationVelocity = Vector3.Cross(missileLauncherRay, new Vector3(0, 0, angularVelocity));
        Vector2 velocity = (velocitySign > 0f ? linearVelocity : Vector2.zero) + 3f * SpriteSize.y * (Vector2)transform.up - rotationVelocity;
        missileLauncher.up = velocity;

        RawMissile rawMissile = new RawMissile(this, missileLauncher.position + (missileRay.y * missileLauncher.up), missileLauncher.eulerAngles, missileBounds);
        rawMissile.SetCollider(new RawBoxCollider2D(missileBoxColliderSize));
        rawMissile.SetVelocities(velocity, 0f);
        rawMissile.SetGameObject();
        rawMissile.SetLifeTime(Time.time, 3f);
        AppManager.ThreadGrid.AddBodyToThreadCell(rawMissile);
    }

    public GameObject CreateMissileGameObject(RawMissile rawMissile)
    {
        Missile missile = missilePrefab.CreateInstance(missileContainer, rawMissile);
        return missile.gameObject;
    }

    public void DestroyMissileGameObject(Transform transform)
    {
        Destroy(transform.gameObject);
    }

    public void RemoveMissileGameObjectsOutOfView()
    {
        List<Missile> list = new List<Missile>();
        foreach (Transform child in missileContainer)
        {
            list.Add(child.GetComponent<Missile>());
        }
        List<Missile> outOfViewList = list.FindAll(missile => !missile.RawMissile.isInCameraView);
        foreach (Missile missile in outOfViewList)
        {
            missile.RawMissile.RemoveGameObject();
        }
    }

    public void DestroyMissiles()
    {
        missileContainer.DestroyAllChildren();
    }

    public void ResetSpaceship()
    {
        rawSpaceship.Reset();
        gameObject.SetActive(true);
    }

    public void StartExplosion(Vector2 position)
    {
        explosionPrefab.CreateInstance(position);
    }

    private IEnumerator MakeIndestructibleRoutine(float duration)
    {
        bool indestructiblePrev = indestructible;
        indestructible = true;
        yield return new WaitForSeconds(duration);
        indestructible = indestructiblePrev;
    }

    private void OnEnable()
    {
        if (!indestructible)
            StartCoroutine(MakeIndestructibleRoutine(2f));
    }
}

public class RawSpaceship : RawBody2D
{
    private readonly Spaceship spaceship = default;

    public RawSpaceship(Spaceship spaceship, Vector2 position, Vector3 eulerAngles, Bounds2 bounds) : base(position, eulerAngles, bounds)
    {
        this.spaceship = spaceship;
    }

    protected override GameObject GetGameObjectInstance(out SpriteRenderer spriteRenderer)
    {
        spriteRenderer = spaceship.SpriteRenderer;
        return spaceship.gameObject;
    }

    protected override void RemoveGameObjectInstance()
    {
        if (spaceship.Indestructible)
            AppManager.ThreadGrid.AddBodyToThreadCell(this);
        else
        {
            spaceship.StartExplosion(transform.position);
            spaceship.gameObject.SetActive(false);
        }
    }
}
