using UnityEngine;

/// <summary>
/// Fades into another scene when a target (the player) comes within a set radius
/// of this object. Attach to (or place at) the center object; assign the player to
/// Target, or leave it empty to track the main camera. A cyan wire sphere shows the
/// radius in the Scene view when selected.
/// </summary>
public class ProximitySceneTrigger : MonoBehaviour
{
    [Tooltip("What must come close (usually the player). Empty = use the main camera.")]
    public Transform target;

    [Tooltip("Trigger distance from this object, in world units.")]
    public float radius = 15f;

    [Tooltip("Scene to fade into when the target is within range (must be in Build Settings).")]
    public string sceneName = "FragmentPurification";

    [Tooltip("Seconds for the white fade.")]
    public float fadeDuration = 1f;

    [Tooltip("Measure distance on the XZ plane only (ignore height).")]
    public bool ignoreVertical = true;

    bool triggered;

    void Update()
    {
        if (triggered) return;

        Transform t = target != null ? target : (Camera.main != null ? Camera.main.transform : null);
        if (t == null) return;

        Vector3 a = transform.position, b = t.position;
        if (ignoreVertical) { a.y = 0f; b.y = 0f; }

        if ((a - b).sqrMagnitude <= radius * radius)
        {
            triggered = true;
            ScreenFader.FadeToScene(sceneName, fadeDuration);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
