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

    private TurnOrderUI turnOrderUI;

    void Start()
    {
        battleUIManager = FindAnyObjectByType<BattleUIManager>(FindObjectsInactive.Include);
        battleStateManager = FindAnyObjectByType<BattleStateManager>(FindObjectsInactive.Include);
        turnOrderUI = FindAnyObjectByType<TurnOrderUI>(FindObjectsInactive.Include);
    }

    // --------------------------------------------------------------------
    // INITIALIZE TURN ORDER
    // --------------------------------------------------------------------
    public void InitializeTurnOrder(List<GameObject> players, List<GameObject> enemies)
    {
        combatants.Clear();
        combatants.AddRange(players);
        combatants.AddRange(enemies);

        if (combatants.Count == 0)
            return;

        // Roll initiative
        foreach (var c in combatants)
        {
            if (c == null) continue;

            if (c.TryGetComponent<CharacterStats>(out var cs))
            {
                int roll = Random.Range(1, 21);
                cs.rolledInitiative = cs.initiative + roll;
            }
            else if (c.TryGetComponent<EnemyStats>(out var es))
            {
                int roll = Random.Range(1, 21);
                es.rolledInitiative = es.initiative + roll;
            }
        }

        // Sort DESC
        combatants = combatants
            .OrderByDescending(obj =>
            {
                if (obj.TryGetComponent<CharacterStats>(out var pc))
                    return pc.rolledInitiative;
                if (obj.TryGetComponent<EnemyStats>(out var ec))
                    return ec.rolledInitiative;
                return 0;
            })
            .ToList();

        // Build turn order UI
        turnOrderUI?.BuildTurnOrder(combatants);

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

    // --------------------------------------------------------------------
    // START TURN
    // --------------------------------------------------------------------
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
        
        var effects = currentTurnObject.GetComponent<StatusEffectManager>();
        effects?.TickStatusEffects();
        
        // Update UI highlight
        turnOrderUI?.UpdateTurnHighlight(currentTurnObject);

        // Skip dead
        if (currentTurnObject == null || !IsAlive(currentTurnObject))
        {
            EndTurn();
            return;
        }

        // Turn Banner
        if (battleUIManager != null)
        {
            string name =
                currentTurnObject.TryGetComponent<CharacterStats>(out var pc) ? pc.characterName :
                currentTurnObject.TryGetComponent<EnemyStats>(out var ec) ? ec.enemyName :
                "Unknown";

            battleUIManager.ShowTurnBanner(name);
        }

        // Player turn
        if (currentTurnObject.GetComponent<CharacterStats>())
        {
            isPlayerTurn = true;
            EnableEndTurnButton(true);

            var cs = currentTurnObject.GetComponent<CharacterStats>();
            if (cs != null)
            {
                cs.ResetTurnActions();
                cs.hasSwitchedThisRound = false;
                cs.midSwapEnteredTurn = false;

                var hud = FindFirstObjectByType<PlayerHUDManager>(FindObjectsInactive.Include);
                hud?.UpdateActionUI();
            }
        }
        else
        {
            // enemy turn
            isPlayerTurn = false;
            EnableEndTurnButton(false);
            StartCoroutine(EnemyTurn(currentTurnObject));
        }
    }

    // --------------------------------------------------------------------
    // ENEMY TURN
    // --------------------------------------------------------------------
    private IEnumerator EnemyTurn(GameObject enemy)
    {
        isProcessingTurn = true;

        EnemyAIController ai = enemy.GetComponent<EnemyAIController>();

        if (ai != null && IsAlive(enemy))
        {
            yield return ai.TakeTurn();
        }
        else
        {
            Debug.LogWarning($"{enemy.name} has no AIController!");
            yield return new WaitForSeconds(0.8f);
        }

        yield return new WaitForSeconds(0.6f);

        isProcessingTurn = false;

        EndTurn();
    }

    // --------------------------------------------------------------------
    // END TURN
    // --------------------------------------------------------------------
    public void EndTurn()
    {
        EnableEndTurnButton(false);
        isPlayerTurn = false;

        turnOrderUI?.AdvanceTurn(currentTurnObject);

        combatants = combatants.Where(c => c != null).ToList();
        if (combatants.Count == 0)
            return;

        if (AllEnemiesDefeated())
        {
            EndBattle();
            return;
        }

        // Advance the index AFTER the portrait is animated
        currentTurnIndex++;
        if (currentTurnIndex >= combatants.Count)
            currentTurnIndex = 0;

        if (currentTurnIndex == 0)
        {
            combatants = combatants
                .Where(c => c != null)
                .OrderByDescending(GetInitiative)
                .ToList();
        }

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

    public void ReplaceCombatant(GameObject oldObj, GameObject newObj)
    {
        int index = combatants.IndexOf(oldObj);
        if (index != -1)
            combatants[index] = newObj;

        if (currentTurnObject == oldObj)
            currentTurnObject = newObj;

        // Update TURN ORDER UI
        var stats = newObj.GetComponent<CharacterStats>();
        if (stats != null && stats.characterSquarePortrait != null)
        {
            turnOrderUI?.ReplacePortraitKey(oldObj, newObj, stats.characterSquarePortrait);
        }
    }


    // REMOVE COMBATANT (death)
    public void RemoveCombatant(GameObject obj)
    {
        if (obj == null) return;

        bool wasCurrent = (currentTurnObject == obj);

        combatants.Remove(obj);


        turnOrderUI?.RemovePortrait(obj);


        if (obj.GetComponent<EnemyStats>())
            battleStateManager?.MarkEnemyKilled(obj);

        if (combatants.Count == 0)
            return;

        if (currentTurnIndex >= combatants.Count)
            currentTurnIndex = 0;

        if (wasCurrent)
            StartTurn();
    }



}
