using UnityEngine;

public class Player : MonoBehaviour
{
    public float PlayerHealth = 100;

    public void TakeDamage(int damage)
    {
        PlayerHealth -= damage;

        Debug.Log("Player took damage: " + PlayerHealth);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
