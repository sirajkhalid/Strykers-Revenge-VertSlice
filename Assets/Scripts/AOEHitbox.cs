using UnityEngine;

public class AOEHitbox : MonoBehaviour
{
    public Ability ability;

    private CharacterStats caster;

    private void Awake()
    {
        // Auto-detect active party member as the caster
        PlayerPartyController party = FindFirstObjectByType<PlayerPartyController>();
        if (party != null)
            caster = party.GetActiveStats();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (caster == null || ability == null)
            return;

        EnemyStats enemy = other.GetComponent<EnemyStats>();
        if (enemy == null)
            return;

        // Damage calculation
        AbilityExecutor.ResolveAttack(
            ability.baseDamage,
            ability.numberOfDice,
            ability.diceSides,
            ability.scalingAttribute,
            caster,
            enemy.armorClass,
            out int finalDamage,
            out bool isCrit,
            out bool isMiss
        );

        enemy.TakeDamage(finalDamage, isCrit, isMiss);
    }
}
