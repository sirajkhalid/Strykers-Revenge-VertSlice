using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class PauseMenu : MonoBehaviour
{
    [Header("References")]
    public CanvasGroup canvasGroup;
    public RectTransform window;

    [Header("Sub-Menus")]
    public GameObject optionsPanel;
    public GameObject helpPanel;

    [Header("Help Panels")]
    public GameObject helpPanel1;
    public GameObject helpPanel2;
    public GameObject helpPanel3;


    [Header("Animation")]
    public float fadeTime = 0.25f;
    public float popupScale = 1.05f;
    public float popupTime = 0.25f;

    public static bool IsOpen { get; private set; }

    void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (window == null)
            window = GetComponentInChildren<RectTransform>();

        
        gameObject.SetActive(true);
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // Sub-menus start disabled
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (helpPanel != null) helpPanel.SetActive(false);

        IsOpen = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) ||
            Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (IsOpen) Close();
        else Open();
    }

    public void Open()
    {
        IsOpen = true;
        Time.timeScale = 0f;

        canvasGroup.DOKill();
        window.DOKill();

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        // Reset subpanels when opening
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (helpPanel != null) helpPanel.SetActive(false);

        // pop animation
        window.localScale = Vector3.one * 0.8f;
        window.DOScale(popupScale, popupTime * 0.7f)
              .SetEase(Ease.OutBack)
              .SetUpdate(true)
              .OnComplete(() =>
              {
                  window.DOScale(1f, popupTime * 0.3f)
                        .SetEase(Ease.OutSine)
                        .SetUpdate(true);
              });
    }

    public void Close()
    {
        IsOpen = false;
        Time.timeScale = 1f;

        canvasGroup.DOKill();
        window.DOKill();

        canvasGroup
            .DOFade(0f, fadeTime)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;

                // Reset sub-panels
                if (optionsPanel != null) optionsPanel.SetActive(false);
                if (helpPanel != null) helpPanel.SetActive(false);

                // Ensure main window is active when closed via ESC/P
                if (window != null)
                    window.gameObject.SetActive(true);
            });
    }



    public void ResumeGame()
    {
        Close();
    }

    public void OpenOptions()
    {
        if (optionsPanel == null) return;

        optionsPanel.SetActive(true);
        if (helpPanel != null) helpPanel.SetActive(false);
    }

    public void OpenHelp()
    {
        if (helpPanel == null) return;

        helpPanel.SetActive(true);
        if (optionsPanel != null) optionsPanel.SetActive(false);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void HideMainWindow()
    {
        if (window == null) return;

        // Fade out + disable interaction
        window.DOKill();
        window.gameObject.SetActive(false);
    }
    public void ShowMainWindow()
    {
        if (window == null) return;

        window.gameObject.SetActive(true);

        
        window.localScale = Vector3.one * 0.8f;
        window.DOScale(1f, 0.25f)
              .SetEase(Ease.OutBack)
              .SetUpdate(true);
    }

    
    public void CloseOptionsPanel()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        ShowMainWindow();   // Restore main pause window UI
    }


    
    public void CloseHelpPanel()
    {
        if (helpPanel != null)
            helpPanel.SetActive(false);

        ShowMainWindow();   // Restore main pause window UI
    }

    public void OpenHelpPanel1()
    {
        // Hide pause menu window
        window.gameObject.SetActive(false);

        helpPanel1.SetActive(true);
        helpPanel2.SetActive(false);
        helpPanel3.SetActive(false);
    }

    public void OpenHelpPanel2()
    {
        helpPanel1.SetActive(false);
        helpPanel2.SetActive(true);
        helpPanel3.SetActive(false);
    }

    public void OpenHelpPanel3()
    {
        helpPanel1.SetActive(false);
        helpPanel2.SetActive(false);
        helpPanel3.SetActive(true);
    }

    public void BackToHelpPanel1()
    {
        helpPanel1.SetActive(true);
        helpPanel2.SetActive(false);
        helpPanel3.SetActive(false);
    }
    public void BackToHelpPanel2()
    {
        helpPanel1.SetActive(false);
        helpPanel2.SetActive(true);
        helpPanel3.SetActive(false);
    }
    public void CloseHelpPanels()
    {
        helpPanel1.SetActive(false);
        helpPanel2.SetActive(false);
        helpPanel3.SetActive(false);

        // Re-show pause window
        window.gameObject.SetActive(true);
    }


}
