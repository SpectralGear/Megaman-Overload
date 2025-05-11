using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingBullet : DirectionalBulletBehaviour
{
    [SerializeField] float turnSpeed = 1;
    [SerializeField] bool returnAfterHit = false;
    public float detectionDistance = 10f;
    public float detectionAngle = 90f; // Half-cone in degrees
    private Camera mainCam;
    private float timer = 0;
    private GameObject currentTarget;
    private EnemyHealth currentTargetHealth;
    public Transform returnPoint;
    void Start()
    {
        mainCam = Camera.main;
        AcquireNewTarget();
    }
    void Update()
    {
        // If there's no target or it died, reacquire
        if (currentTarget == null || currentTargetHealth == null || currentTargetHealth.dead)
        {
            AcquireNewTarget();
        }
        timer+=Time.deltaTime;
        if (timer>2.8f){hitTarget=true;}
    }
    void AcquireNewTarget()
    {
        currentTarget = null;
        currentTargetHealth = null;

        EnemyHealth[] enemies = FindObjectsOfType<EnemyHealth>();
        Vector2 origin = transform.position;
        Vector2 forward = Quaternion.Euler(0, 0, transform.eulerAngles.z) * Vector2.right;

        EnemyHealth bestTarget = null;
        float bestScore = float.MaxValue;

        foreach (EnemyHealth enemy in enemies)
        {
            if (enemy == null || enemy.dead)
                continue;

            Vector2 toEnemy = (Vector2)enemy.transform.position - origin;
            float dist = toEnemy.magnitude;

            if (dist > detectionDistance)
                continue;

            if (!IsOnScreen(enemy.transform.position))
                continue;

            float angle = Vector2.Angle(forward, toEnemy);
            if (angle > detectionAngle)
                continue;

            float score = dist + (angle * 0.1f); // angle bias
            if (score < bestScore)
            {
                bestScore = score;
                bestTarget = enemy;
            }
        }

        if (bestTarget != null)
        {
            currentTarget = bestTarget.gameObject;
            currentTargetHealth = bestTarget;
            Debug.Log("Target acquired: " + currentTarget.name);
        }
    }
    bool IsOnScreen(Vector3 worldPos)
    {
        Vector3 view = mainCam.WorldToViewportPoint(worldPos);
        return view.z > 0 && view.x > 0 && view.x < 1 && view.y > 0 && view.y < 1;
    }
    protected override void Move()
    {
        Vector3 targetPosition = currentTarget&&!(hitTarget&&returnAfterHit)?currentTarget.transform.position:(returnPoint?returnPoint.position:Vector2.zero);
        Vector2 direction = targetPosition - transform.position;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float currentAngle = transform.eulerAngles.z;
        float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, turnSpeed * Time.fixedDeltaTime);
        transform.rotation = Quaternion.Euler(0f, 0f, newAngle);
        base.Move();
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Buster buster = collision.GetAny<Buster>();
            if (buster&&hitTarget){buster.armLaunched=false;Destroy(gameObject);}
        }
    }
}
