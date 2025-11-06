using UnityEngine;

public class BattleStateManager : MonoBehaviour
{
    [Header("Battle UI References")]
    public GameObject battleUI;
    public bool isBattleActive = false;

    void Start()
    {
        if (battleUI != null)
            battleUI.SetActive(false);
    }

    // Toggle manually from other scripts when battle begins
    public void ToggleBattleState()
    {
        isBattleActive = !isBattleActive;

        if (battleUI != null)
            battleUI.SetActive(isBattleActive);
    }

    // start battle
    public void StartBattle()
    {
        isBattleActive = true;
        if (battleUI != null)
            battleUI.SetActive(true);
    }

    // end battle
    public void EndBattle()
    {
        isBattleActive = false;
        if (battleUI != null)
            battleUI.SetActive(false);
    }
}
