using UnityEngine;

public class AbilityUIController : MonoBehaviour
{
    public SkillSlot[] slots;

    void Awake()
    {
        if (slots == null || slots.Length == 0)
            slots = FindObjectsByType<SkillSlot>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    }

    public void LockAllAbilitiesExcept(string allowed)
    {
        foreach (var slot in slots)
        {
            if (slot.assignedAbility == null) continue;

            slot.isTemporarilyDisabled =
                slot.assignedAbility.abilityName != allowed;
        }
    }

    public void UnlockAllAbilities()
    {
        foreach (var slot in slots)
            slot.isTemporarilyDisabled = false;
    }
}
