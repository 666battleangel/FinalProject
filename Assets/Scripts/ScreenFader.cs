using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Spawns a full-screen white overlay, fades it in, loads the requested scene,
/// then fades it back out. Survives the scene load via DontDestroyOnLoad, so the
/// transition stays smooth. Created at runtime — nothing to place in the scene.
/// </summary>
public class ScreenFader : MonoBehaviour
{
    public static void FadeToScene(string sceneName, float duration)
    {
        var go = new GameObject("ScreenFader",
            typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster), typeof(Image));
        DontDestroyOnLoad(go);
        go.AddComponent<ScreenFader>().Begin(sceneName, duration);
    }

    void Begin(string sceneName, float duration) => StartCoroutine(Run(sceneName, duration));

    IEnumerator Run(string sceneName, float duration)
    {
        var canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32767; // above everything

        var img = GetComponent<Image>();
        img.raycastTarget = true;    // block input during the transition
        img.color = new Color(1f, 1f, 1f, 0f);

        yield return Fade(img, 0f, 1f, duration);            // fade to white

        // Placeholder-safe: only transition if the scene exists and is in Build Settings.
        // Until then the click just flashes to white and back. Add the scene later and it
        // starts transitioning automatically -- no code changes needed.
        if (!string.IsNullOrEmpty(sceneName) && Application.CanStreamedLevelBeLoaded(sceneName))
        {
            yield return SceneManager.LoadSceneAsync(sceneName); // swap scenes under the white
            yield return null;                                   // let the new scene settle a frame
        }

        yield return Fade(img, 1f, 0f, duration);            // fade back in
        Destroy(gameObject);
    }

    static IEnumerator Fade(Image img, float from, float to, float dur)
    {
        for (float t = 0f; t < dur; t += Time.deltaTime)
        {
            float a = dur <= 0f ? to : Mathf.Lerp(from, to, t / dur);
            img.color = new Color(1f, 1f, 1f, a);
            yield return null;
        }
        img.color = new Color(1f, 1f, 1f, to);
    }
}
