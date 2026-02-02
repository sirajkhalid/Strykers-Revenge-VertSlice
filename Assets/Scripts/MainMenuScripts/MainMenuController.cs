using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("Buttons")]
    public Button newGameButton;
    public Button creditsButton;
    public Button quitButton;

    [Header("Scene Names")]
    public string CastleScene = "Castle1";
    public string creditsScene = "Credits";

    [Header("Fade")]
    public Image fadeOverlay;    
    public float fadeTime = 0.75f;  // How long the fade lasts

    void Start()
    {
        // Wire up buttons
        newGameButton.onClick.AddListener(OnNewGame);
        creditsButton.onClick.AddListener(OnCredits);
        quitButton.onClick.AddListener(OnQuit);

        // Start with a fade-in
        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);
            fadeOverlay.color = new Color(0, 0, 0, 1);
            StartCoroutine(Fade(1f, 0f));
        }
    }

    void OnNewGame()
    {
        StartCoroutine(FadeAndLoad(CastleScene));
    }

    void OnCredits()
    {
        StartCoroutine(FadeAndLoad(creditsScene));
    }

    void OnQuit()
    {
        Debug.Log("QUIT GAME");
        Application.Quit();
    }

    // ---------------------------
    // FADING
    // ---------------------------

    IEnumerator Fade(float from, float to)
    {
        float t = 0f;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(from, to, t / fadeTime);
            fadeOverlay.color = new Color(0, 0, 0, a);
            yield return null;
        }

        fadeOverlay.color = new Color(0, 0, 0, to);
    }

    IEnumerator FadeAndLoad(string scene)
    {
        // fade to black
        yield return Fade(0f, 1f);

        // load new scene
        SceneManager.LoadScene("Castle1");
    }
}
