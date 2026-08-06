using UnityEngine;

/// <summary>
/// The Empathy boss demon's stats. Unlike the Strength and Intelligence demons,
/// the Empathy demon has no attack at all -- it cannot hit. It only walks and can
/// take damage. Attach to the Empathy demon GameObject.
/// </summary>
public class EmpathyDemon : MonoBehaviour
{
    [Header("Stats")]
    [Tooltip("Current health. The demon dies at 0.")]
    public float health = 10f;

    [Tooltip("Movement speed, in units per second.")]
    public float walkingSpeed = 2f;

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
