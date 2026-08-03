using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Spawns a full-screen white overlay, fades it in, performs an action at peak
/// white (load a scene, teleport, anything), then fades back out. Survives scene
/// loads via DontDestroyOnLoad. Created at runtime -- nothing to place in the scene.
/// </summary>
public class ScreenFader : MonoBehaviour
{
    /// <summary>Fade to white, load a scene (only if it exists in Build Settings), fade back in.</summary>
    public static void FadeToScene(string sceneName, float duration)
    {
        Create().Begin(duration, () =>
        {
            // Placeholder-safe: only load if the scene is actually available.
            if (!string.IsNullOrEmpty(sceneName) && Application.CanStreamedLevelBeLoaded(sceneName))
                SceneManager.LoadScene(sceneName);
        });
    }

    /// <summary>Fade to white, run an action (e.g. teleport the player), fade back in.</summary>
    public static void FadeAndRun(float duration, Action atWhite)
    {
        Create().Begin(duration, atWhite);
    }

    static ScreenFader Create()
    {
        var go = new GameObject("ScreenFader",
            typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster), typeof(Image));
        DontDestroyOnLoad(go);
        return go.AddComponent<ScreenFader>();
    }

    void Begin(float duration, Action atWhite) => StartCoroutine(Run(duration, atWhite));

    IEnumerator Run(float duration, Action atWhite)
    {
        var canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32767; // above everything

        var img = GetComponent<Image>();
        img.raycastTarget = true;    // block input during the transition
        img.color = new Color(1f, 1f, 1f, 0f);

        yield return Fade(img, 0f, 1f, duration); // fade to white
        atWhite?.Invoke();                         // load scene / teleport / etc.
        yield return null;                         // settle a frame under the white
        yield return Fade(img, 1f, 0f, duration); // fade back in
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
