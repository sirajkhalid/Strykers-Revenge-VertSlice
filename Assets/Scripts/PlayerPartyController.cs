using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;


public class PlayerPartyController : MonoBehaviour
{
    [Header("Party Setup (Party members go here)")]
    public List<GameObject> partyPrefabs = new List<GameObject>();

    [Header("Runtime Party Objects")]
    public List<GameObject> partyMembers = new List<GameObject>();

    public int activeIndex = 0;
    public GameObject activeMember;

    [Header("References")]
    public Camera mainCamera;
    public GameObject switchVFX_In;
    public GameObject switchVFX_Out;

    [Header("Settings")]
    public float spawnOffsetY = 0.0f;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        SpawnInitialParty();
        var ui = FindFirstObjectByType<TeamSwitchUI>();
        if (ui != null)
            ui.RefreshDisplay();
    }

    void Update()
    {
        var battle = FindFirstObjectByType<BattleStateManager>();
        if (battle != null && battle.isBattleActive)
            return;

        if (Input.GetKeyDown(KeyCode.Z)) SwitchTo(0);
        if (Input.GetKeyDown(KeyCode.X)) SwitchTo(1);
        if (Input.GetKeyDown(KeyCode.C)) SwitchTo(2);
        if (Input.GetKeyDown(KeyCode.V)) SwitchTo(3);
    }



    void SpawnInitialParty()
    {
        if (partyPrefabs == null || partyPrefabs.Count == 0)
        {
            Debug.LogError("No party prefabs assigned!");
            return;
        }

        Vector3 startPos = transform.position;

        // Spawn first member
        GameObject first = Instantiate(partyPrefabs[0], startPos, Quaternion.identity);
        activeMember = first;
        partyMembers.Add(first);

        // Preload the others (inactive)
        for (int i = 1; i < partyPrefabs.Count; i++)
        {
            GameObject p = Instantiate(partyPrefabs[i], startPos, Quaternion.identity);
            p.SetActive(false);
            partyMembers.Add(p);
        }


           foreach (var member in partyMembers)
               {
            var stats = member.GetComponent<CharacterStats>();
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

        if (cine != null && activeMember != null)
        {
            cine.Follow = activeMember.transform;
        }
    }

    public void SwitchTo(int newIndex)
    {
        if (newIndex < 0 || newIndex >= partyMembers.Count)
            return;

        if (newIndex == activeIndex)
            return;

        GameObject oldChar = activeMember;
        GameObject newChar = partyMembers[newIndex];

        Vector3 swapPos = oldChar.transform.position;
        swapPos.y += spawnOffsetY;

        // VFX out
        if (switchVFX_Out && oldChar != null)
            Instantiate(switchVFX_Out, oldChar.transform.position, Quaternion.identity);

        // Disable old
        oldChar.SetActive(false);

        // Enable new
        newChar.transform.position = swapPos;
        newChar.SetActive(true);

        // VFX in
        if (switchVFX_In && newChar != null)
            Instantiate(switchVFX_In, newChar.transform.position, Quaternion.identity);

        activeMember = newChar;
        activeIndex = newIndex;

        HookCamera();

        // Update UI
        var ui = FindFirstObjectByType<TeamSwitchUI>();
        if (ui != null)
            ui.RefreshDisplay();

    }

    public CharacterStats GetActiveStats()
    {
        return activeMember.GetComponent<CharacterStats>();
    }
}
