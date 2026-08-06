using UnityEngine;

/// <summary>
/// The Intelligence boss demon's combat stats. Identical to the other boss demons for
/// now -- kept as separate scripts so each demon can be tuned or given unique
/// behaviour later. Attach to the Intelligence demon GameObject.
/// </summary>
public class IntelligenceDemon : MonoBehaviour
{
    [Header("Stats")]
    [Tooltip("Current health. The demon dies at 0.")]
    public float health = 10f;

    [Tooltip("Movement speed, in units per second.")]
    public float walkingSpeed = 2f;

    [Tooltip("Attacks per second (0.5 = one hit every 2 seconds).")]
    public float hitFrequency = 0.5f;

    [Tooltip("Damage dealt per hit, in health.")]
    public float hitStrength = 1f;

    /// <summary>Seconds between attacks, derived from the hit frequency.</summary>
    public float SecondsBetweenHits => hitFrequency > 0f ? 1f / hitFrequency : Mathf.Infinity;

    /// <summary>True while the demon still has health.</summary>
    public bool IsAlive => health > 0f;

    /// <summary>Apply damage; the demon dies when health reaches 0.</summary>
    public void TakeDamage(float amount)
    {
        health = Mathf.Max(0f, health - amount);
        if (health <= 0f) Die();
    }

    void Die()
    {
        // Placeholder -- swap in death VFX/animation later.
        Destroy(gameObject);
    }
}
