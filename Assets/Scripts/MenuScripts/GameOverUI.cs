using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;

public class GameOverUI : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI gameOverText;
    public Button mainMenuButton;
    public Button quitButton;

    void Start()
    {
        // Fade in the text
        gameOverText.alpha = 0;
        gameOverText.DOFade(1f, 1.5f).SetEase(Ease.OutQuad);

        // Button listeners
        mainMenuButton.onClick.AddListener(GoToMainMenu);
        quitButton.onClick.AddListener(QuitGame);
    }

    private void GoToMainMenu()
    {
        SceneManager.LoadScene("mainmenu");
    }

    private void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
