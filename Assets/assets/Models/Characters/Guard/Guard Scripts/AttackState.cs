using UnityEngine;

namespace Assets.Scripts.Guardscripts
{
    public class AttackState : BaseState
    {
        private float losePlayerTimer;
        public float waitbeforeSearchTime = 5f;
        public float rotationSpeed = 10f;

        [Header("Fire Rate")]
        public float shotsPerSecond = 1.5f;
        private float fireTimer;

        public override void Enter()
        {
            guard.agent.isStopped = true;
            losePlayerTimer = 0f;
            fireTimer = 0f;

            guard.SetGunActive(true);
        }

        public override void Exit()
        {
            guard.agent.isStopped = false;
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

                guard.UpdateAnimationParameters(false, false, true);

                guard.SetGunActive(true);

                guard.detection = 100f;

                guard.UpdateSliderUI();

                guard.SetSliderColor(Color.red);

                if (fireTimer > guard.fireRate)
                {
                    fireTimer = 1f / shotsPerSecond;
                    guard.PlayShootAnimation();

                // TODO: actual damage/VFX logic goes here
                ShootAtPlayer();

                if (PlayerPrefs.GetInt("LevelIndex", 0) == 0)
                {
                    TutorialManager tutorial = Object.FindObjectOfType<TutorialManager>();

                    if (tutorial != null)
                    {
                        tutorial.playerSpottedByGuard();
                    }
                }
            }
        }
        else
        {
            //guard.UpdateAnimationParameters(false, true, false);

            //guard.SetGunActive(false);

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
            Vector3 direction = guard.PlayerTransform.position - guard.agent.transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.001f) return;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            guard.agent.transform.rotation = Quaternion.Slerp(
                guard.agent.transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        public void ShootAtPlayer()
        {
            Transform gunBarrel = guard.gunBarrel;

            Vector3 baseTargetPoint = guard.player.transform.position + (Vector3.up * 7f);
            Vector3 shootDirection = (baseTargetPoint - gunBarrel.position).normalized;

            Vector3 finalDirection = Quaternion.AngleAxis(Random.Range(-5f, 5f), Vector3.up) * shootDirection;

            GameObject bullet = GameObject.Instantiate(
                Resources.Load("Prefabs/Bullet") as GameObject,
                gunBarrel.position,
                Quaternion.LookRotation(finalDirection)
            );

            bullet.GetComponent<Rigidbody>().linearVelocity = finalDirection * guard.bulletSpeed;

            Debug.Log("Shoot");

        fireTimer = 0f;
    }
    }
}