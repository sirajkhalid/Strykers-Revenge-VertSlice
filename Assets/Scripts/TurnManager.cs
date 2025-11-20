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


    // INITIALIZE TURN ORDER
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
        if (!obj) return 0;
        if (obj.TryGetComponent<CharacterStats>(out var p)) return p.initiative;
        if (obj.TryGetComponent<EnemyStats>(out var e)) return e.initiative;
        return 0;
    }

    // START TURN
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

        // UI Banner
        string name =
            currentTurnObject.TryGetComponent<CharacterStats>(out var pc) ? pc.characterName :
            currentTurnObject.TryGetComponent<EnemyStats>(out var ec) ? ec.enemyName :
            "Unknown";

        battleUIManager?.ShowTurnBanner(name);

        // If PLAYER turn
        if (currentTurnObject.GetComponent<CharacterStats>())
        {
            isPlayerTurn = true;
            EnableEndTurnButton(true);

            var cs = currentTurnObject.GetComponent<CharacterStats>();
            if (cs != null)
            {
                // Always reset actions at the start of ANY new turn
                cs.ResetTurnActions();
                cs.hasSwitchedThisRound = false;
                cs.midSwapEnteredTurn = false; // clear any leftover flag

                var hud = FindFirstObjectByType<PlayerHUDManager>(FindObjectsInactive.Include);
                if (hud != null)
                    hud.UpdateActionUI();
            }
        }
        else
        {
            // ENEMY TURN
            isPlayerTurn = false;
            EnableEndTurnButton(false);
            StartCoroutine(EnemyTurn(currentTurnObject));
        }
    }

    // ENEMY TURN
    private IEnumerator EnemyTurn(GameObject enemy)
    {
        isProcessingTurn = true;
        yield return new WaitForSeconds(1f);

        if (enemy && IsAlive(enemy))
        {
            Debug.Log($"{enemy.name} attacks!");
        }

        yield return new WaitForSeconds(1f);
        isProcessingTurn = false;
        EndTurn();
    }

    // END TURN
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

        // Increment turn index
        currentTurnIndex++;
        if (currentTurnIndex >= combatants.Count)
            currentTurnIndex = 0;

        // NEW ROUND → RESORT INITIATIVE
        if (currentTurnIndex == 0)
        {
            combatants = combatants
                .Where(c => c != null)
                .OrderByDescending(GetInitiative)
                .ToList();
        }

        // Skip dead/null
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

    // HELPER FUNCTIONS
    private bool IsAlive(GameObject obj)
    {
        if (!obj) return false;
        if (obj.TryGetComponent<CharacterStats>(out var p)) return p.currentHealth > 0;
        if (obj.TryGetComponent<EnemyStats>(out var e)) return e.currentHealth > 0;
        return false;
    }

    private bool AllEnemiesDefeated()
    {
        return !combatants.Any(c =>
            c != null &&
            c.GetComponent<EnemyStats>() &&
            IsAlive(c)
        );
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

    // REPLACE COMBATANT (USED FOR SWITCHING)
    public void ReplaceCombatant(GameObject oldObj, GameObject newObj)
    {
        int index = combatants.IndexOf(oldObj);
        if (index != -1)
            combatants[index] = newObj;

        if (currentTurnObject == oldObj)
            currentTurnObject = newObj;
    }
}
