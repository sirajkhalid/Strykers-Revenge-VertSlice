using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class BattleUIManager : MonoBehaviour
{
    public TextMeshProUGUI turnBannerText;
    public Button endTurnButton;

    private Coroutine bannerRoutine;
    private bool firstBannerShown = false;
    private TurnManager turnManager;

    void Start()
    {
        turnManager = FindFirstObjectByType<TurnManager>();
        if (endTurnButton != null)
            endTurnButton.onClick.AddListener(OnEndTurnPressed);
    }

    public void ShowTurnBanner(string characterName)
    {
        if (turnBannerText == null) return;

        if (bannerRoutine != null)
            StopCoroutine(bannerRoutine);

        bannerRoutine = StartCoroutine(ShowBannerCoroutine(characterName));
    }

    private IEnumerator ShowBannerCoroutine(string name)
    {
        if (!firstBannerShown)
        {
            yield return new WaitForSeconds(3f);
            firstBannerShown = true;
        }

        turnBannerText.text = $"{name}'s Turn";
        turnBannerText.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.5f);
        turnBannerText.gameObject.SetActive(false);
        bannerRoutine = null;
    }

    private void OnEndTurnPressed()
    {
        if (turnManager != null)
            turnManager.EndTurn();
    }

    public void ResetBannerDelay()
    {
        firstBannerShown = false;
    }
}
