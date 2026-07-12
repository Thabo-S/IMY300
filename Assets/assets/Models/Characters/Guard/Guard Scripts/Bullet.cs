using UnityEngine;

public class Bullet : MonoBehaviour
{
    private void Start()
    {
        Destroy(gameObject, 5f);
    }
    public int bulletDamage = 10;

    private void OnCollisionEnter(Collision collision)
    {
        Transform hitObject = collision.transform;

        if (hitObject.CompareTag("Player"))
        {
            hitObject.GetComponent<Player>().TakeDamage(bulletDamage);
        }

        Destroy(gameObject);
    }
}
