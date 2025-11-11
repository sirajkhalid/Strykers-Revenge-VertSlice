using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [Header("Turn Order")]
    public List<GameObject> combatants = new List<GameObject>();
    public int currentTurnIndex = 0;

    private BattleUIManager battleUIManager;
    private bool isProcessingTurn = false;

    void Start()
    {
        battleUIManager = FindAnyObjectByType<BattleUIManager>(FindObjectsInactive.Include);
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

        // Remove null or destroyed entries
        combatants = combatants.Where(c => c != null).ToList();

        // If list is empty after cleanup, battle is over
        if (combatants.Count == 0)
            return;

        // Clamp current index
        currentTurnIndex = Mathf.Clamp(currentTurnIndex, 0, combatants.Count - 1);

        var current = combatants[currentTurnIndex];

        // Skip null or dead units
        if (current == null || !IsAlive(current))
        {
            EndTurn();
            return;
        }

        string name = current.GetComponent<CharacterStats>() ? current.GetComponent<CharacterStats>().characterName :
                       current.GetComponent<EnemyStats>() ? current.GetComponent<EnemyStats>().enemyName : "Unknown";

        battleUIManager?.ShowTurnBanner(name);

        if (current.GetComponent<CharacterStats>())
        {
            EnableEndTurnButton(true);
        }
        else if (current.GetComponent<EnemyStats>())
        {
            EnableEndTurnButton(false);
            StartCoroutine(EnemyTurn(current));
        }
    }

    private IEnumerator EnemyTurn(GameObject enemy)
    {
        isProcessingTurn = true;
        yield return new WaitForSeconds(1f);

        if (enemy != null && IsAlive(enemy))
        {
            // Placeholder for AI
            Debug.Log($"{enemy.name} attacks!");
        }

        yield return new WaitForSeconds(1f);
        isProcessingTurn = false;
        EndTurn();
    }

    public void EndTurn()
    {
        EnableEndTurnButton(false);

        // Clean list again to remove destroyed objects
        combatants = combatants.Where(c => c != null).ToList();

        // End battle if all combatants are gone
        if (combatants.Count == 0)
            return;

        currentTurnIndex++;
        if (currentTurnIndex >= combatants.Count)
            currentTurnIndex = 0;

        // Skip null or dead
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

    private void EnableEndTurnButton(bool value)
    {
        if (battleUIManager != null && battleUIManager.endTurnButton != null)
            battleUIManager.endTurnButton.interactable = value;
    }
}
