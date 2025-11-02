using UnityEngine;

[System.Serializable]
public class UndeadAbilities : BaseEnemyAbilities
{
    public override void InitializeAbilities(GameObject user)
    {
        abilities.Clear();

        // Life Drain
        abilities.Add(new EnemyAbility
        {
            abilityName = "Life Drain",
            description = "Drains vitality from the target.",
            damage = 8,
            onUse = (caster) => Debug.Log($"{caster.name} uses Life Drain!")
        });

        // Fear Aura
        abilities.Add(new EnemyAbility
        {
            abilityName = "Fear Aura",
            description = "Instills fear in nearby enemies.",
            damage = 0,
            onUse = (caster) => Debug.Log($"{caster.name} emits a fear-inducing aura!")
        });

        // Shadow Step
        abilities.Add(new EnemyAbility
        {
            abilityName = "Shadow Step",
            description = "Teleports to a nearby position.",
            damage = 0,
            onUse = (caster) => Debug.Log($"{caster.name} vanishes and reappears in the shadows!")
        });

        // Grave Touch
        abilities.Add(new EnemyAbility
        {
            abilityName = "Grave Touch",
            description = "A chilling attack that weakens the living.",
            damage = 6,
            onUse = (caster) => Debug.Log($"{caster.name} delivers a Grave Touch!")
        });
    }
}
