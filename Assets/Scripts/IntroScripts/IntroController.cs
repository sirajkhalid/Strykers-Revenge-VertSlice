using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class IntroController : MonoBehaviour
{
    [Header("Next Scene")]
    [SerializeField] string nextSceneName = "CharacterCreation";

    [Header("Timing")]
    [SerializeField] float contentFadeIn = 0.6f;  // fade-in of your intro text/UI
    [SerializeField] float holdTime = 2.0f;  // time to show before skip/autoadvance
    [SerializeField] float blackFadeOut = 0.5f;  // fade-to-black duration

    [Header("Skip")]
    [SerializeField] bool allowSkip = true;
    [SerializeField] float skipDelay = 0.25f;     // slight delay before skip becomes active

    [Header("UI (assign in Inspector)")]
    [SerializeField] CanvasGroup contentGroup;    // CanvasGroup on your intro UI (texts/panel)
    [SerializeField] CanvasGroup blackOverlay;    // CanvasGroup on a full-screen black Image

    void Awake()
    {
        // Fallbacks if not wired (optional)
        if (!contentGroup) contentGroup = GetComponentInChildren<CanvasGroup>(true);
        if (!blackOverlay)
        {
            var go = GameObject.Find("BlackOverlay");
            if (go) blackOverlay = go.GetComponent<CanvasGroup>();
        }

        if (contentGroup) contentGroup.alpha = 0f;
        if (blackOverlay) blackOverlay.alpha = 0f;

        Time.timeScale = 1f; // just in case
    }

    void OnEnable() => StartCoroutine(RunIntro());

    IEnumerator RunIntro()
    {
        // Fade content in
        if (contentGroup && contentFadeIn > 0f)
            yield return Fade(contentGroup, 0f, 1f, contentFadeIn);
        else if (contentGroup) contentGroup.alpha = 1f;

        // Wait (allow skip)
        if (allowSkip && skipDelay > 0f) yield return new WaitForSecondsRealtime(skipDelay);

        float t = 0f;
        while (t < holdTime)
        {
            t += Time.unscaledDeltaTime;
            if (allowSkip && SkipPressedThisFrame()) break;
            yield return null;
        }

        // Fade to BLACK (covers the whole screen so you don't see camera background)
        if (blackOverlay && blackFadeOut > 0f)
            yield return Fade(blackOverlay, 0f, 1f, blackFadeOut);
        else if (contentGroup && blackFadeOut > 0f)
            yield return Fade(contentGroup, 1f, 0f, blackFadeOut); // fallback
        // else instant

        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator Fade(CanvasGroup cg, float from, float to, float dur)
    {
        float t = 0f;
        cg.alpha = from; // write on this frame
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / dur);
            cg.alpha = Mathf.Lerp(from, to, p);
            yield return null;
        }
        cg.alpha = to;
    }

    bool SkipPressedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) return true;
        if (Mouse.current != null &&
            (Mouse.current.leftButton.wasPressedThisFrame ||
             Mouse.current.rightButton.wasPressedThisFrame ||
             Mouse.current.middleButton.wasPressedThisFrame)) return true;
        if (Gamepad.current != null &&
            (Gamepad.current.startButton.wasPressedThisFrame ||
             Gamepad.current.buttonSouth.wasPressedThisFrame ||
             Gamepad.current.buttonEast.wasPressedThisFrame)) return true;
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame) return true;
        return false;
#else
        return Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1);
#endif
    }
}
