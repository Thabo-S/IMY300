using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int bulletDamage = 10;
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

        // Ignore whoever fired this bullet - it can spawn overlapping the
        // guard's own collider for the first physics step, which was
        // destroying the bullet instantly instead of letting it travel.
        if (shooter != null &&
            (hitObject.gameObject == shooter || hitObject.root.gameObject == shooter))
        {
            return;
        }

        if (hitObject.CompareTag("Player") && PlayerPrefs.GetInt("LevelIndex", 0) == 0)
        {
            hitObject.GetComponent<Player>().TakeDamage(bulletDamageTutorial);

        }
        else
        {
            hitObject.GetComponent<Player>().TakeDamage(bulletDamage);
        }

        Destroy(gameObject);
    }
}