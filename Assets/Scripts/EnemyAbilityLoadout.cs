using System.Collections.Generic;
using UnityEngine;

public class EnemyAbilityLoadout : MonoBehaviour
{
    [Header("Enemy Abilities")]
    public List<Ability> abilities = new List<Ability>();

    // Optional: choose if the AI picks randomly or uses logic later
    public bool chooseRandomAbility = true;

    public Ability GetRandomAbility()
    {
        if (abilities.Count == 0) return null;
        int index = Random.Range(0, abilities.Count);
        return abilities[index];
    }

    public Ability GetAbility(int index)
    {
        if (index < 0 || index >= abilities.Count) return null;
        return abilities[index];
    }

    public Ability GetUsableAbility(EnemyStats stats)
    {
        List<Ability> usable = new List<Ability>();

        foreach (var ability in abilities)
        {
            if (!ability.usesSpellSlot)
            {
                usable.Add(ability);
            }
            else if (stats.HasSpellSlots(ability.spellLevel, ability.slotCost))
            {
                usable.Add(ability);
            }
        }

        if (usable.Count == 0) return null;

        return usable[Random.Range(0, usable.Count)];
    }

}
