using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.ProBuilder.AutoUnwrapSettings;

public class Player : MonoBehaviour
{
    [Header("Health Variables")]
    public Slider HealthBarSlider;
    public float MaxHealth = 100;
    public float PlayerHealth;
        public Gradient gradient;
    public Image fill;

    [Header("Footstep Audio")]
    public AudioSource walkingFootsteps;
    public AudioSource runningFootsteps;

    void Start()
    {
        PlayerHealth = MaxHealth;

        HealthBarSlider.maxValue = MaxHealth;

        HealthBarSlider.value = MaxHealth;

        fill.color = gradient.Evaluate(1f);
    }

    public void TakeDamage(int damage)
    {
        PlayerHealth -= damage;

        HealthBarSlider.value = PlayerHealth;

        fill.color = gradient.Evaluate(HealthBarSlider.normalizedValue);

        Debug.Log("Player took damage: " + PlayerHealth);
    }
    public void PlayFootsteps(bool isSprinting)
    {
        if (isSprinting)
        {
            if (!runningFootsteps.isPlaying)
            {
                walkingFootsteps.Stop();
                runningFootsteps.Play();
            }
        }
        else
        {
            if (!walkingFootsteps.isPlaying)
            {
                runningFootsteps.Stop();
                walkingFootsteps.Play();
            }
        }
    }

    public void StopFootsteps()
    {
        walkingFootsteps.Stop();
        runningFootsteps.Stop();
    }
}
