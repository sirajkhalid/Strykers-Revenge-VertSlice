using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [Header("Turn Order")]
    public List<GameObject> combatants = new List<GameObject>();
    public int currentTurnIndex = 0;

    [HideInInspector] public GameObject currentTurnObject;
    [HideInInspector] public bool isPlayerTurn = false;

    private BattleUIManager battleUIManager;
    private BattleStateManager battleStateManager;
    private bool isProcessingTurn = false;

    void Start()
    {
        battleUIManager = FindAnyObjectByType<BattleUIManager>(FindObjectsInactive.Include);
        battleStateManager = FindAnyObjectByType<BattleStateManager>(FindObjectsInactive.Include);
    }

    public void InitializeTurnOrder(List<GameObject> players, List<GameObject> enemies)
    {
        combatants.Clear();
        combatants.AddRange(players);
        combatants.AddRange(enemies);

        if (combatants.Count == 0)
            return;

        combatants = combatants.OrderByDescending(GetInitiative).ToList();

        currentTurnIndex = 0;
        StartTurn();
    }

    private int GetInitiative(GameObject obj)
    {
        if (obj == null) return 0;
        if (obj.TryGetComponent<CharacterStats>(out var p)) return p.initiative;
        if (obj.TryGetComponent<EnemyStats>(out var e)) return e.initiative;
        return 0;
    }

    private void StartTurn()
    {
        if (combatants.Count == 0) return;
        if (isProcessingTurn) return;

        combatants = combatants.Where(c => c != null).ToList();
        if (combatants.Count == 0) return;

        currentTurnIndex = Mathf.Clamp(currentTurnIndex, 0, combatants.Count - 1);

        if (AllEnemiesDefeated())
        {
            EndBattle();
            return;
        }

        currentTurnObject = combatants[currentTurnIndex];
        if (currentTurnObject == null || !IsAlive(currentTurnObject))
        {
            EndTurn();
            return;
        }

        string name = currentTurnObject.GetComponent<CharacterStats>() ? currentTurnObject.GetComponent<CharacterStats>().characterName :
                       currentTurnObject.GetComponent<EnemyStats>() ? currentTurnObject.GetComponent<EnemyStats>().enemyName : "Unknown";

        battleUIManager?.ShowTurnBanner(name);

        // Determine whose turn it is
        if (currentTurnObject.GetComponent<CharacterStats>())
        {
            isPlayerTurn = true;
            EnableEndTurnButton(true);

            // Reset player actions & refresh HUD
            var cs = currentTurnObject.GetComponent<CharacterStats>();
            if (cs != null)
            {
                cs.ResetTurnActions();

                var hud = FindFirstObjectByType<PlayerHUDManager>(FindObjectsInactive.Include);
                if (hud != null)
                    hud.UpdateActionUI();
            }
        }
        else
        {
            isPlayerTurn = false;
            EnableEndTurnButton(false);
            StartCoroutine(EnemyTurn(currentTurnObject));
        }
    }

    private IEnumerator EnemyTurn(GameObject enemy)
    {
        isProcessingTurn = true;
        yield return new WaitForSeconds(1f);

        if (enemy != null && IsAlive(enemy))
        {
            Debug.Log($"{enemy.name} attacks!");
        }

        yield return new WaitForSeconds(1f);
        isProcessingTurn = false;
        EndTurn();
    }

    public void EndTurn()
    {
        EnableEndTurnButton(false);
        isPlayerTurn = false;

        combatants = combatants.Where(c => c != null).ToList();
        if (combatants.Count == 0)
            return;

        if (AllEnemiesDefeated())
        {
            EndBattle();
            return;
        }

        currentTurnIndex++;
        if (currentTurnIndex >= combatants.Count)
            currentTurnIndex = 0;

        int safety = 0;
        while ((combatants[currentTurnIndex] == null || !IsAlive(combatants[currentTurnIndex])) && safety < 50)
        {
            currentTurnIndex++;
            if (currentTurnIndex >= combatants.Count)
                currentTurnIndex = 0;
            safety++;
        }

        StartTurn();
    }

    private bool IsAlive(GameObject obj)
    {
        if (obj == null) return false;
        if (obj.TryGetComponent<CharacterStats>(out var p))
            return p.currentHealth > 0;
        if (obj.TryGetComponent<EnemyStats>(out var e))
            return e.currentHealth > 0;
        return false;
    }

    private bool AllEnemiesDefeated()
    {
        return !combatants.Any(c => c != null && c.GetComponent<EnemyStats>() && IsAlive(c));
    }

    private void EndBattle()
    {
        EnableEndTurnButton(false);
        battleStateManager?.EndBattle();
        isPlayerTurn = false;
        currentTurnObject = null;
    }

    private void EnableEndTurnButton(bool value)
    {
        if (battleUIManager != null && battleUIManager.endTurnButton != null)
            battleUIManager.endTurnButton.interactable = value;
    }
}
