using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerPartyController : MonoBehaviour
{
    [Header("Party Setup")]
    public List<GameObject> partyPrefabs = new List<GameObject>();

    [Header("Runtime Party Objects")]
    public List<GameObject> partyMembers = new List<GameObject>();

    public int activeIndex = 0;
    public GameObject activeMember;

    [Header("References")]
    public Camera mainCamera;

    [Header("Settings")]
    public float spawnOffsetY = 0.0f;

    public CinemachineImpulseSource impulseSource;

    void Start()
    {
        if (!mainCamera)
            mainCamera = Camera.main;

        SpawnInitialParty();

        var ui = FindFirstObjectByType<TeamSwitchUI>();
        if (ui != null)
            ui.RefreshDisplay();

        Invoke(nameof(ForceHUDInit), 0.1f);
    }

    void SpawnInitialParty()
    {
        if (partyPrefabs.Count == 0)
        {
            Debug.LogError("No party prefabs assigned!");
            return;
        }

        Vector3 startPos = transform.position;

        // First member = active
        GameObject first = Instantiate(partyPrefabs[0], startPos, Quaternion.identity);
        activeMember = first;
        partyMembers.Add(first);

        // Others inactive
        for (int i = 1; i < partyPrefabs.Count; i++)
        {
            GameObject p = Instantiate(partyPrefabs[i], startPos, Quaternion.identity);
            p.SetActive(false);
            partyMembers.Add(p);
        }

        // Init stats
        foreach (var m in partyMembers)
        {
            var stats = m.GetComponent<CharacterStats>();
            if (stats != null)
            {
                stats.CalculateAllStats();
                stats.currentHealth = stats.maxHealth;
            }
        }

        HookCamera();
    }

    void HookCamera()
    {
        var cine = FindFirstObjectByType<CinemachineCamera>();
        if (cine != null && activeMember)
            cine.Follow = activeMember.transform;
    }

    // SWITCH FUNCTION
    public void SwitchTo(int newIndex)
    {
        if (newIndex < 0 || newIndex >= partyMembers.Count)
            return;

        if (newIndex == activeIndex)
            return;

        GameObject oldChar = activeMember;
        if (!oldChar) return;

        CharacterStats oldStats = oldChar.GetComponent<CharacterStats>();
        if (!oldStats) return;

        if (oldStats.currentHealth <= 0)
            return;

        GameObject newChar = partyMembers[newIndex];
        if (!newChar) return;

        CharacterStats newStats = newChar.GetComponent<CharacterStats>();
        if (!newStats) return;

        if (newStats.currentHealth <= 0)
            return;

        var battle = FindFirstObjectByType<BattleStateManager>();
        var turnManager = FindFirstObjectByType<TurnManager>();
        bool inBattle = battle && battle.isBattleActive;

        // BATTLE SWITCH CHECKS
        if (inBattle)
        {
            if (!turnManager) return;

            if (!turnManager.isPlayerTurn)
                return;

            if (turnManager.currentTurnObject != oldChar)
                return;

            if (oldStats.hasSwitchedThisRound)
                return;

            if (!oldStats.hasBonusAction)
                return;
        }

        // POSITION + VFX
        Transform anchor = oldChar.transform.childCount > 0
            ? oldChar.transform.GetChild(0)
            : oldChar.transform;

        Vector3 pos = anchor.position;
        pos.y += spawnOffsetY;

        if (oldStats.swapOutVFX)
        {
            GameObject vfx = GameObject.Instantiate(oldStats.swapOutVFX, anchor.position, Quaternion.identity);
            Destroy(vfx, oldStats.vfxLifetime);
        }

        oldChar.SetActive(false);

        newChar.transform.position = pos;
        newChar.SetActive(true);

        if (inBattle)
            newStats.midSwapEnteredTurn = true;

        if (inBattle)
            turnManager.ReplaceCombatant(oldChar, newChar);

        if (impulseSource)
            impulseSource.GenerateImpulse();

        if (newStats.swapInVFX)
        {
            GameObject vfx = GameObject.Instantiate(newStats.swapInVFX, pos, Quaternion.identity);
            Destroy(vfx, newStats.vfxLifetime);
        }

        // SET NEW ACTIVE MEMBER
        activeMember = newChar;
        activeIndex = newIndex;
        HookCamera();

        var hud = FindFirstObjectByType<PlayerHUDManager>(FindObjectsInactive.Include);
        if (hud)
        {
            hud.SetTarget(newStats);
            hud.RefreshSkillBar(newStats);
        }

        var teamUI = FindFirstObjectByType<TeamSwitchUI>();
        if (teamUI)
            teamUI.RefreshDisplay();

        // MID-BATTLE SWITCH LOGIC
        if (inBattle)
        {
            oldStats.hasBonusAction = false;
            oldStats.hasAction = false;
            oldStats.hasSwitchedThisRound = true;

            newStats.hasAction = false;
            newStats.hasBonusAction = true;
            newStats.currentMovement = newStats.maxMovement;

            turnManager.currentTurnObject = newChar;
            turnManager.isPlayerTurn = true;

            if (hud)
                hud.UpdateActionUI();

            var battleUI = FindFirstObjectByType<BattleUIManager>(FindObjectsInactive.Include);
            if (battleUI)
                battleUI.ShowTurnBanner(newStats.characterName);
        }
    }

    // NEXT ALIVE (DEATH SWITCHING)
    public bool HasAliveBackup()
    {
        foreach (var member in partyMembers)
        {
            if (member != activeMember &&
                member.GetComponent<CharacterStats>().currentHealth > 0)
                return true;
        }
        return false;
    }

    public CharacterStats GetActiveStats()
    {
        if (activeMember == null) return null;
        return activeMember.GetComponent<CharacterStats>();
    }
    public void SwitchToNextAlive()
    {
        for (int i = 0; i < partyMembers.Count; i++)
        {
            if (i != activeIndex)
            {
                var stats = partyMembers[i].GetComponent<CharacterStats>();
                if (stats.currentHealth > 0)
                {
                    
                    SwitchTo(i);
                    return;
                }
            }
        }
    }

    void ForceHUDInit()
    {
        var hud = FindFirstObjectByType<PlayerHUDManager>(FindObjectsInactive.Include);
        if (hud)
        {
            hud.SetTarget(activeMember.GetComponent<CharacterStats>());
            hud.RefreshSkillBar(activeMember.GetComponent<CharacterStats>());
        }
    }

}
