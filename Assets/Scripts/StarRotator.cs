using UnityEngine;

/// <summary>
/// Slowly spins the object around its Z axis. Works for UI elements
/// (RectTransform) and regular transforms alike.
/// </summary>
public class StarRotator : MonoBehaviour
{
    [Tooltip("Rotation speed in degrees per second. Negative spins the other way.")]
    public float degreesPerSecond = 15f;

    void Update()
    {
        transform.Rotate(0f, 0f, degreesPerSecond * Time.deltaTime);
    }
}
