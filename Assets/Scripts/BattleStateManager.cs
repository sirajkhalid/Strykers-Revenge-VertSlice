using UnityEngine;

public class BattleStateManager : MonoBehaviour
{
    public GameObject battleUI; 
    public bool isBattleActive = false; // Tracks whether the battle state is active
    void Start()
    {
        // Ensure the BattleUI starts off
        battleUI.SetActive(false);
    }
    void Update()
    {
        // Check for battle state activation (for testing purposes, press 'B' to toggle)
        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleBattleState();
        }
    }
    public void ToggleBattleState()
    {
        isBattleActive = !isBattleActive; // Toggle the battle state
        battleUI.SetActive(isBattleActive); // Show or hide the BattleUI based on the state
    }
}