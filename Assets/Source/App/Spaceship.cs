using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spaceship : MonoBehaviour
{
    [SerializeField, Range(1.0f, 10.0f)]
    private float linearVelocity = 1f;

    [SerializeField, Range(1.0f, 5.0f)]
    private float angularVelocity = 1f;

    [SerializeField]
    private bool indestructible;

    [SerializeField]
    private Animator jetAnimator;

    [SerializeField]
    private Missile missilePrefab;

    [SerializeField]
    private Transform missileContainer;

    [SerializeField]
    private SpaceshipExplosion explosion;

    private const float linearVelocityFactor = 10f;

    private const float angularVelocityFactor = 160f;

    private Rigidbody2D rigidBody2D;

    private SpriteRenderer spriteRenderer;

    private CameraDriver cameraDriver;

    private float lastShotTime;

    private Vector2 SpriteSize { get => spriteRenderer.size; }

    private Coroutine shootRoutine;

    private void Start()
    {
        rigidBody2D = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        cameraDriver = Camera.main.GetComponent<CameraDriver>();
        missilePrefab.gameObject.SetActive(false);
        shootRoutine = StartCoroutine(ShootRoutine());
    }

    private void FixedUpdate()
    {
        float inputY = Input.GetAxis("Vertical");
        float inputX = Input.GetAxis("Horizontal");
        //Debug.LogWarning(GetType() + ".Update: " + inputX + " " + inputY);
        if (Mathf.Abs(inputX) > 0f)
        {
            rigidBody2D.angularVelocity = -inputX * angularVelocityFactor * angularVelocity;
            //transform.Rotate(0, 0, -inputX * angularVelocityScaler * angularVelocity * Time.fixedDeltaTime);
        }
        else
        {
            rigidBody2D.angularVelocity = 0;
        }
        if (inputY > 0f)
        {
            rigidBody2D.velocity = transform.up * inputY * linearVelocityFactor * linearVelocity;
            //transform.Translate(0, inputY * linearVelocityScaler * linearVelocity * Time.fixedDeltaTime, 0, Space.Self);
            jetAnimator.gameObject.SetActive(true);
        }
        else
        {
            rigidBody2D.velocity = Vector2.zero;
            jetAnimator.gameObject.SetActive(false);
        }
        cameraDriver.FollowTarget(transform);
    }


    private IEnumerator ShootRoutine()
    {
        while (gameObject.activeSelf)
        {
            yield return new WaitForSeconds(0.5f);
            //yield return new WaitForFixedUpdate();
            Transform missileLauncher = missilePrefab.transform.parent;
            missileLauncher.localRotation = Quaternion.Euler(0, 0, rigidBody2D.angularVelocity * Time.fixedDeltaTime);
            //Vector2 missileLauncherRay = transform.TransformVector(missileLauncher.localPosition);
            Vector2 missileLauncherRay = missileLauncher.localPosition.y * transform.up;
            Vector2 rotationVelocity = Vector3.Cross(missileLauncherRay, new Vector3(0, 0, rigidBody2D.angularVelocity * Mathf.Deg2Rad));
            Vector2 velocity = rigidBody2D.velocity + 4f * SpriteSize.y * (Vector2)transform.up - rotationVelocity;
            missileLauncher.up = velocity;

            Missile missile = Instantiate(missilePrefab, missileLauncher, false);
            missile.gameObject.SetActive(true);
            missile.transform.SetParent(missileContainer, true);
            missile.GetComponent<Rigidbody2D>().velocity = velocity;
            missile.transform.up = velocity;
            //Time.timeScale = 0.1f;
        }
    }

    public void Respawn()
    {
        transform.position = Vector2.zero;
        transform.rotation = Quaternion.identity;
        rigidBody2D.velocity = Vector2.zero;
        explosion.gameObject.SetActive(false);
        gameObject.SetActive(true);
        shootRoutine = StartCoroutine(ShootRoutine());
    }

    public void StartExplosion(Vector2 pos)
    {
        explosion.gameObject.SetActive(true);
        explosion.transform.position = pos;
        explosion.Play();
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        //Debug.Log(GetType() + ".OnTriggerEnter2D: " + collider.gameObject);
        if (!indestructible)
        {
            StartExplosion((transform.position + collider.transform.position) / 2f);
            StopCoroutine(shootRoutine);
            gameObject.SetActive(false);
        }
    }

}