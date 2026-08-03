using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Smoothly scales a UI element up while the pointer hovers over it,
/// then eases it back to its original size on exit. Scales about the
/// element's pivot, so keep the pivot centered for symmetric growth.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class HoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("Scale multiplier while hovered (1.15 = 15% larger).")]
    public float hoverScale = 1.15f;

    [Tooltip("Easing speed toward the target scale. Higher = snappier.")]
    public float speed = 12f;

    RectTransform rect;
    Vector3 baseScale;
    bool hovered;

    void Awake()
    {
        rect = (RectTransform)transform;
        baseScale = rect.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData) => hovered = true;
    public void OnPointerExit(PointerEventData eventData) => hovered = false;

    void Update()
    {
        Vector3 target = hovered ? baseScale * hoverScale : baseScale;
        float t = 1f - Mathf.Exp(-speed * Time.deltaTime);
        rect.localScale = Vector3.Lerp(rect.localScale, target, t);
    }
}
