using UnityEngine;

public class PartyInputHandler : MonoBehaviour
{
    PlayerPartyController party;

    void Start()
    {
        party = FindFirstObjectByType<PlayerPartyController>();
    }

    void Update()
    {
        if (party == null) return;

        
        if (Input.GetKeyDown(KeyCode.Z)) party.SwitchTo(0);
        if (Input.GetKeyDown(KeyCode.X)) party.SwitchTo(1);
        if (Input.GetKeyDown(KeyCode.C)) party.SwitchTo(2);
        if (Input.GetKeyDown(KeyCode.V)) party.SwitchTo(3);
    }
}
