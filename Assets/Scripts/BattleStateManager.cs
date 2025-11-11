using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleStateManager : MonoBehaviour
{
    [Header("Battle UI References")]
    public GameObject battleUI;
    public bool isBattleActive = false;

    private BattleIntroManager introManager;
    private TurnManager turnManager;

    void Start()
    {
        if (battleUI != null)
            battleUI.SetActive(false);

        introManager = FindFirstObjectByType<BattleIntroManager>();
        turnManager = GetComponent<TurnManager>();
    }

    public void ToggleBattleState()
    {
        isBattleActive = !isBattleActive;
        if (battleUI != null)
            battleUI.SetActive(isBattleActive);

        if (isBattleActive)
            TriggerBattleIntro();
    }

    public void StartBattle()
    {
        isBattleActive = true;

        if (battleUI != null)
            battleUI.SetActive(true);

        TriggerBattleIntro();

        List<GameObject> players = Resources.FindObjectsOfTypeAll<GameObject>()
            .Where(obj => obj.CompareTag("Player") && obj.scene.IsValid())
            .ToList();

        List<GameObject> enemies = Resources.FindObjectsOfTypeAll<GameObject>()
            .Where(obj => obj.CompareTag("Enemy") && obj.scene.IsValid())
            .ToList();

        if (turnManager != null)
            turnManager.InitializeTurnOrder(players, enemies);
    }

    private void TriggerBattleIntro()
    {
        if (introManager != null)
            introManager.PlayBattleIntro();
    }

    public void EndBattle()
    {
        isBattleActive = false;
        if (battleUI != null)
            battleUI.SetActive(false);

        var ui = FindFirstObjectByType<BattleUIManager>();
        if (ui != null)
            ui.ResetBannerDelay();
    }

}
