using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// The map/travel button. While quests remain it fades to the map scene; once
/// every quest marker is complete it fades to the final boss scene instead.
/// Requires an EventSystem + GraphicRaycaster and a raycastable graphic.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class MapTravelButton : MonoBehaviour, IPointerClickHandler
{
    [Tooltip("Scene to load while quests are still unfinished.")]
    public string mapScene = "Map";

    [Tooltip("Scene to load once all quests are complete.")]
    public string bossScene = "FinalBossInitial";

    [Tooltip("Seconds for the white fade.")]
    public float fadeDuration = 1f;

    bool started;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (started) return;
        started = true;
        string target = QuestManager.AllComplete() ? bossScene : mapScene;
        ScreenFader.FadeToScene(target, fadeDuration);
    }
}
