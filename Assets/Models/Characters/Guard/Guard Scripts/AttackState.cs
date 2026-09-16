using UnityEngine;

public class AttackState : BaseState
{
    private float losePlayerTimer;
    public float waitbeforeSearchTime = 5f;
    public float rotationSpeed = 10f;

    [Header("Chase")]
    [Tooltip("Guard closes distance while chasing at this speed.")]
    public float chaseSpeed = 2.7f;
    [Tooltip("Guard stops advancing once within this distance and just shoots.")]
    public float attackRange = 4f;

    [Header("Fire Rate")]
    public float shotsPerSecond = 1.5f;
    private float fireTimer;

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

            float distanceToPlayer = Vector3.Distance(guard.transform.position, guard.PlayerTransform.position);

            if (distanceToPlayer > attackRange)
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

        Vector3 baseTargetPoint = guard.player.transform.position + (Vector3.up * guard.eyeHeight);
        Vector3 shootDirection = (baseTargetPoint - gunBarrel.position).normalized;

        float shotRange = 0;

        //if (PlayerPrefs.GetInt("currentLevelPrefKey", 0) == 0)
        //{
        //    shotRange = 0;
        //}
        //else
        //{
        //    shotRange = 2f;
        //}

        Vector3 finalDirection = Quaternion.AngleAxis(Random.Range(-shotRange, shotRange), Vector3.up) * shootDirection;

        GameObject bullet = GameObject.Instantiate(
            Resources.Load("Prefabs/Bullet") as GameObject,
            gunBarrel.position,
            Quaternion.LookRotation(finalDirection)
        );

        bullet.GetComponent<Bullet>().SetShooter(guard.gameObject);

        bullet.GetComponent<Rigidbody>().linearVelocity = finalDirection * guard.bulletSpeed;

        Debug.Log("Shoot");

        fireTimer = 0f;
    }
}