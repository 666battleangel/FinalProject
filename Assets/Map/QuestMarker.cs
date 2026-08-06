using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// An "unfinished quest" marker on the map.
/// - Grows slightly while hovered.
/// - On click: fades the screen to white and teleports the player to a location.
/// - Once the quest at that location is complete, call <see cref="CompleteQuest"/>:
///   the marker hides itself and can no longer be clicked/teleported to.
///
/// Registers itself with <see cref="QuestManager"/> so other scenes (e.g. the map
/// button) can tell when every quest is done. Needs an EventSystem + GraphicRaycaster
/// in the scene and a raycastable Image on this object (RaycastTarget on).
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class QuestMarker : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Hover")]
    [Tooltip("Scale multiplier while hovered (1.15 = 15% larger).")]
    public float hoverScale = 1.15f;
    [Tooltip("Easing speed toward the hover scale.")]
    public float hoverSpeed = 12f;

    [Header("Teleport (leave empty for now)")]
    [Tooltip("The player object to move on click. Leave empty until the player exists.")]
    public Transform player;
    [Tooltip("Where the player teleports to. Leave empty for now.")]
    public Transform teleportTarget;
    [Tooltip("Seconds for the white fade.")]
    public float fadeDuration = 1f;

    [Header("Quest state")]
    [Tooltip("Unique id for this quest. Leave empty to use the GameObject's name.")]
    public string questId;
    [Tooltip("False while the quest is unfinished. Becomes true (and the marker hides) once CompleteQuest() is called.")]
    public bool questComplete = false;

    string Id => string.IsNullOrEmpty(questId) ? gameObject.name : questId;

    RectTransform rect;
    Vector3 baseScale;
    bool hovered;

    void Awake()
    {
        rect = (RectTransform)transform;
        baseScale = rect.localScale;
        QuestManager.Register(Id);            // count this quest toward the total
        if (questComplete) QuestManager.Complete(Id);
    }

    void OnEnable()
    {
        // If this quest is already complete (this session), don't show the marker.
        if (questComplete || QuestManager.IsComplete(Id))
        {
            questComplete = true;
            QuestManager.Complete(Id);
            gameObject.SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData e) => hovered = true;
    public void OnPointerExit(PointerEventData e) => hovered = false;

    void Update()
    {
        Vector3 target = hovered ? baseScale * hoverScale : baseScale;
        float t = 1f - Mathf.Exp(-hoverSpeed * Time.deltaTime);
        rect.localScale = Vector3.Lerp(rect.localScale, target, t);
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (questComplete) return;                 // completed -> can't teleport back here
        ScreenFader.FadeAndRun(fadeDuration, Teleport);
    }

    void Teleport()
    {
        // Left intentionally guarded: does nothing until a player + target are assigned.
        if (player != null && teleportTarget != null)
        {
            player.position = teleportTarget.position;
            player.rotation = teleportTarget.rotation;
        }
    }

    /// <summary>
    /// Call this when the quest at this location is finished. Marks it complete,
    /// hides the unfinished-quest image, records it with the QuestManager, and
    /// prevents any further teleporting here.
    /// </summary>
    public void CompleteQuest()
    {
        questComplete = true;
        QuestManager.Complete(Id);
        gameObject.SetActive(false); // hide the unfinished-quest image
    }
}
