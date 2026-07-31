using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Raises a UI element slightly while the pointer hovers over it,
/// then eases it back down on exit. Requires an EventSystem in the scene
/// and a raycastable Graphic on this object.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class HoverRise : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("How far (in UI units) the element rises while hovered.")]
    public float riseAmount = 4f;

    [Tooltip("Easing speed toward the target position. Higher = snappier.")]
    public float speed = 10f;

    RectTransform rect;
    Vector2 basePosition;
    bool hovered;

    void Awake()
    {
        rect = (RectTransform)transform;
        basePosition = rect.anchoredPosition;
    }

    public void OnPointerEnter(PointerEventData eventData) => hovered = true;
    public void OnPointerExit(PointerEventData eventData) => hovered = false;

    void Update()
    {
        Vector2 target = hovered ? basePosition + Vector2.up * riseAmount : basePosition;
        float t = 1f - Mathf.Exp(-speed * Time.deltaTime);
        rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, target, t);
    }
}
