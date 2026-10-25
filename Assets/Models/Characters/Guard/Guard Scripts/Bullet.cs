using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int bulletDamage = 15;
    public int bulletDamageTutorial = 25;

    private GameObject shooter; // the guard that fired this bullet

    private void Start()
    {
        Destroy(gameObject, 3f);
    }

    public void SetShooter(GameObject shooterObj)
    {
        shooter = shooterObj;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Transform hitObject = collision.transform;

        // Ignore the shooter and its child colliders so the bullet can leave the gun.
        if (shooter != null &&
            (hitObject.gameObject == shooter || hitObject.root.gameObject == shooter))
        {
            return;
        }

        // Player colliders may be on child objects, so search up the hierarchy.
        Player player = hitObject.GetComponentInParent<Player>();
        if (player != null)
        {
            int damage = PlayerPrefs.GetInt("currentLevelPrefKey", 0) == 0
                ? bulletDamageTutorial
                : bulletDamage;
            player.TakeDamage(damage);
        }

        // Bullets are consumed by any non-shooter impact, including walls and props.
        Destroy(gameObject);
    }
}