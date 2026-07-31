using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// On click, fades the screen to white and loads the target scene.
/// Attach to a raycastable UI element (needs an EventSystem in the scene).
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class FadeToScene : MonoBehaviour, IPointerClickHandler
{
    [Tooltip("Exact name of the scene to load. It MUST be added to Build Settings.")]
    public string sceneName = "EntranceScene";

    [Tooltip("Seconds for the fade-to-white (and the fade back in on the new scene).")]
    public float fadeDuration = 1f;

    bool started;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (started) return;
        started = true;
        ScreenFader.FadeToScene(sceneName, fadeDuration);
    }
}
