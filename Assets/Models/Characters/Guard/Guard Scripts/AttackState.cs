using UnityEngine;

public class AttackState : BaseState
{
    private float losePlayerTimer;
    public float waitbeforeSearchTime = 5f;
    public float rotationSpeed = 10f;

    [Header("Chase")]
    [Tooltip("Guard closes distance while chasing at this speed.")]
    public float chaseSpeed = 2f;
    [Tooltip("Guard stops advancing once within this distance and just shoots.")]
    public float attackRange = 4f;

    [Header("Fire Rate")]
    public float shotsPerSecond = 1.5f;
    private float fireTimer;

    // Resources.Load() was being called on every single shot from every
    // guard - it hits the asset database each time. Loaded once and shared
    // across every AttackState instance instead.
    private static GameObject bulletPrefab;

    public override void Enter()
    {
        guard.SetVocalState(Guard.GuardVocalState.Spotted);
        if (GuardSpeechManager.Instance != null)
            GuardSpeechManager.Instance.PlaySpottedBark(guard);


        guard.Agent.isStopped = false;
        guard.Agent.speed = chaseSpeed;
        losePlayerTimer = 0f;
        fireTimer = 0f;

        guard.SetGunActive(true);
    }

    public override void Exit()
    {
        guard.Agent.isStopped = false;
        guard.UpdateAnimationParameters(false, false, false);

        guard.SetGunActive(false);

        guard.detection = 0f;
    }

    public override void Perform()
    {
        if (guard.CanSeePlayer())
        {
            losePlayerTimer = 0;

            fireTimer += Time.deltaTime;

            LookAtPlayer();

            // Threshold check only - sqrMagnitude avoids a sqrt every frame
            // while the guard is attacking.
            float sqrDistanceToPlayer = (guard.PlayerTransform.position - guard.transform.position).sqrMagnitude;

            if (sqrDistanceToPlayer > attackRange * attackRange)
            {
                // Close the gap while still shooting on the way in.
                guard.Agent.isStopped = false;
                guard.Agent.SetDestination(guard.PlayerTransform.position);
            }
            else
            {
                // Close enough — plant and shoot instead of walking into the player.
                guard.Agent.isStopped = true;
            }

            guard.UpdateAnimationParameters(false, false, true);

            guard.SetGunActive(true);

            guard.detection = 100f;

            guard.UpdateSliderUI();

            guard.SetSliderColor(Color.red);

            if (fireTimer > guard.fireRate)
            {
                fireTimer = 1f / shotsPerSecond;
                guard.PlayShootAnimation();

                ShootAtPlayer();

                guard.GunshotAudio();
            }
        }
        else
        {
            losePlayerTimer += Time.deltaTime;

            if (losePlayerTimer > waitbeforeSearchTime)
            {
                AlertState alert = new AlertState();

                UnityEngine.AI.NavMeshHit hit;
                if (UnityEngine.AI.NavMesh.SamplePosition(guard.LastKnownPlayerPosition, out hit, 2.0f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    alert.lastKnownPosition = hit.position;
                }
                else
                {
                    alert.lastKnownPosition = guard.transform.position;
                }

                stateMachine.ChangeState(alert);

                guard.UpdateAnimationParameters(false, true, false);

                guard.SetGunActive(false);
                Debug.Log("[ATTACK] Lost player, changing to ALERT State");
            }
        }
    }

    private void LookAtPlayer()
    {
        Vector3 direction = guard.PlayerTransform.position - guard.Agent.transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        guard.Agent.transform.rotation = Quaternion.Slerp(
            guard.Agent.transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    public void ShootAtPlayer()
    {
        Transform gunBarrel = guard.gunBarrel;

        CharacterController playerController = guard.PlayerController;
        Vector3 baseTargetPoint = playerController != null ? playerController.bounds.center : guard.player.transform.position;
        Vector3 shootDirection = (baseTargetPoint - gunBarrel.position).normalized;

        float shotRange = 0;

        Vector3 finalDirection = Quaternion.AngleAxis(Random.Range(-shotRange, shotRange), Vector3.up) * shootDirection;

        if (bulletPrefab == null)
        {
            bulletPrefab = Resources.Load<GameObject>("Prefabs/Bullet");
        }

        GameObject bullet = GameObject.Instantiate(
            bulletPrefab,
            gunBarrel.position,
            Quaternion.LookRotation(finalDirection)
        );

        // One GetComponent<Rigidbody>() call instead of two GetComponent calls
        // (Bullet + Rigidbody) that each walked the component list.
        Rigidbody bulletBody = bullet.GetComponent<Rigidbody>();
        bulletBody.linearVelocity = finalDirection * guard.bulletSpeed;
        bullet.GetComponent<Bullet>().SetShooter(guard.gameObject);

        fireTimer = 0f;
    }
}