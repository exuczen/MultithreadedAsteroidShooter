﻿//#define DEBUG_TRIANGLE

using DC;
using RawPhysics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spaceship : MonoBehaviour
{
    [SerializeField, Range(1.0f, 10.0f)]
    private float linearVelocityMagnitude = 1f;

    [SerializeField, Range(1.0f, 5.0f)]
    private float angularVelocityMagnitude = 1f;

    [SerializeField]
    private bool indestructible;

    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private Animator jetAnimator;

    [SerializeField]
    private Missile missilePrefab;

    [SerializeField]
    private Transform missileContainer;

    [SerializeField]
    private SpaceshipExplosion explosion;

    private const float linearVelocityFactor = 10f;

    private const float angularVelocityFactor = 2.8f;

    private RawSpaceship rawSpaceship;

    private RawTriangleCollider2D rawCollider;

    private Vector2 missileBoxColliderSize;

    private Bounds2 missileBounds;

    private float lastShotTime;

    private SpriteRenderer[] markers;

    //private Coroutine shootRoutine;

    private Vector2 SpriteSize { get => spriteRenderer.size; }

    public SpriteRenderer SpriteRenderer { get => spriteRenderer; }

    public bool Indestructible { get => indestructible; }

    public RawSpaceship RawSpaceship { get => rawSpaceship; }

    private void Awake()
    {
        missilePrefab.gameObject.SetActive(false);

        BoxCollider2D missileBoxCollider = missilePrefab.GetComponent<BoxCollider2D>();
        missileBoxCollider.enabled = false;
        missileBoxColliderSize = missileBoxCollider.size * missileBoxCollider.transform.lossyScale;
        float missileMaxDim = Mathf.Max(missileBoxColliderSize.x, missileBoxColliderSize.y);
        missileBounds = new Bounds2(Vector2.zero, new Vector2(missileMaxDim, missileMaxDim));
        Destroy(missileBoxCollider);

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
        //rawSpaceship.SetCollider(new RawBoxCollider2D(boxCollider.size * transform.lossyScale));
        rawSpaceship.SetGameObject();

        //shootRoutine = StartCoroutine(ShootRoutine());
    }

    public void UpdateSpaceship()
    {
        float inputY = Input.GetAxis("Vertical");
        float inputX = Input.GetAxis("Horizontal");
        //Debug.LogWarning(GetType() + ".Update: " + inputX + " " + inputY);

        float angularVelocity = -inputX * angularVelocityFactor * angularVelocityMagnitude;
        Vector2 linearVelocity = inputY * transform.up * linearVelocityFactor * linearVelocityMagnitude;

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

        Transform missileLauncher = missilePrefab.transform.parent;
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

    //public void StopShootRoutine()
    //{
    //    if (shootRoutine != null)
    //        StopCoroutine(shootRoutine);
    //    shootRoutine = null;
    //}

    //private IEnumerator ShootRoutine()
    //{
    //    while (gameObject.activeSelf)
    //    {
    //        yield return new WaitForSeconds(0.5f);
    //        Shoot();
    //    }
    //}

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
        explosion.gameObject.SetActive(false);
        gameObject.SetActive(true);
        //shootRoutine = StartCoroutine(ShootRoutine());
    }

    public void StartExplosion(Vector2 pos)
    {
        explosion.gameObject.SetActive(true);
        explosion.transform.position = pos;
        explosion.Play();
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

    //private void OnTriggerEnter2D(Collider2D collider)
    //{
    //}
}

public class RawSpaceship : RawBody2D
{
    private Spaceship spaceship;

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
            //spaceship.StopShootRoutine();
            spaceship.StartExplosion(transform.position);
            spaceship.gameObject.SetActive(false);
        }
    }
}
