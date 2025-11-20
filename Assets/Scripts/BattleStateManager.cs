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

        // Only the active party member gets included in the turn manager
        var partyController = FindFirstObjectByType<PlayerPartyController>();
        GameObject activePlayer = partyController != null ? partyController.activeMember : null;

        List<GameObject> players = new List<GameObject>();
        if (activePlayer != null)
            players.Add(activePlayer);

        // Collect only active enemies
        List<GameObject> enemies = Resources.FindObjectsOfTypeAll<GameObject>()
            .Where(obj => obj.CompareTag("Enemy") && obj.scene.IsValid() && obj.activeInHierarchy)
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
