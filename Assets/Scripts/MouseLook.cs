using UnityEngine;

/// <summary>
/// First-person mouse look for a camera. Move the mouse to turn (yaw) and
/// look up/down (pitch, clamped). Cursor locks on play; press Esc to toggle
/// the lock so you can click away. Uses the legacy Input Manager axes.
/// </summary>
public class MouseLook : MonoBehaviour
{
    [Tooltip("Degrees of rotation per unit of mouse movement.")]
    public float sensitivity = 2f;

    [Tooltip("Maximum look up/down angle, in degrees.")]
    public float pitchClamp = 80f;

    [Tooltip("Lock the cursor to the screen center while looking.")]
    public bool lockCursor = true;

    float yaw;
    float pitch;

    void Start()
    {
        Vector3 e = transform.localEulerAngles;
        yaw = e.y;
        pitch = e.x > 180f ? e.x - 360f : e.x;
        if (lockCursor) SetLocked(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            SetLocked(Cursor.lockState != CursorLockMode.Locked);

        // While unlocked, don't steal the mouse.
        if (lockCursor && Cursor.lockState != CursorLockMode.Locked) return;

        yaw += Input.GetAxis("Mouse X") * sensitivity;
        pitch -= Input.GetAxis("Mouse Y") * sensitivity;
        pitch = Mathf.Clamp(pitch, -pitchClamp, pitchClamp);
        transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void SetLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
