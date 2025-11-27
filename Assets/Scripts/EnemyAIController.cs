using System.Collections;
using UnityEditor.Playables;
using UnityEngine;

public class EnemyAIController : MonoBehaviour
{
    private EnemyStats stats;
    private EnemyAbilityLoadout loadout;
    private TurnManager turnManager;
    private Transform targetPlayer;

    public AIBehaviorProfile behavior;

    private const float ENEMY_DAMAGE_MULTIPLIER = 0.4f;

    void Start()
    {
        stats = GetComponent<EnemyStats>();
        loadout = GetComponent<EnemyAbilityLoadout>();
        turnManager = FindFirstObjectByType<TurnManager>();

        // Always target current active player
        var party = FindFirstObjectByType<PlayerPartyController>();
        if (party != null && party.activeMember != null)
            targetPlayer = party.activeMember.transform;
    }

    public IEnumerator TakeTurn()
    {
        if (stats.currentHealth <= 0)
            yield break;

        stats.currentMovement = stats.maxMovement;

        // Always retarget before acting
        PlayerPartyController party = FindFirstObjectByType<PlayerPartyController>();
        if (party == null || party.activeMember == null)
        {
            
            yield break;
        }

        targetPlayer = party.activeMember.transform;
        FaceTarget(targetPlayer);

        float dist = Vector3.Distance(transform.position, targetPlayer.position);
        Ability chosen = ChooseBestAbility(dist);

        if (chosen != null)
        {
            yield return MoveTowardTargetIfNeeded(dist, chosen);

            // Retarget again after moving 
            party = FindFirstObjectByType<PlayerPartyController>();
            if (party == null || party.activeMember == null)
            {
                
                yield break;
            }

            targetPlayer = party.activeMember.transform;
            FaceTarget(targetPlayer);

            dist = Vector3.Distance(transform.position, targetPlayer.position);

            if (dist <= chosen.range)
                yield return UseChosenAbility(chosen);
        }

    }


    private Ability ChooseBestAbility(float distance)
    {
        Ability fallback = null;

        foreach (var ability in loadout.abilities)
        {
            if (!ability.isUnlocked) continue;

            // Spell slot check
            if (ability.usesSpellSlot &&
                !stats.HasSpellSlots(ability.spellLevel, ability.slotCost))
                continue;

            // Utility logic
            if (ability.category == Ability.AbilityCategory.Utility)
            {
                if (behavior.prefersUtilityWhenLowHP &&
                    stats.currentHealth <= stats.maxHealth * behavior.utilityHPThreshold)
                {
                    return ability;
                }
            }

            // Melee preference
            if (ability.deliveryType == Ability.DeliveryType.Melee &&
                distance <= ability.range)
            {
                return ability;
            }

            // Ranged / Magic
            if (ability.category == Ability.AbilityCategory.Magic)
                fallback = ability;
        }

        return fallback != null ? fallback : (loadout.abilities.Count > 0 ? loadout.abilities[0] : null);
    }

    private IEnumerator MoveTowardTargetIfNeeded(float dist, Ability ability)
    {
        // Already in range → no movement
        if (dist <= ability.range)
            yield break;

        float moveDist = Mathf.Min(stats.currentMovement, dist - ability.range);
        if (moveDist <= 0f)
            yield break;

        Vector3 direction = (targetPlayer.position - transform.position).normalized;
        Vector3 destination = transform.position + direction * moveDist;

        // Smooth but simple movement
        float speed = 3.0f; // tweak for faster/slower enemies

        while (Vector3.Distance(transform.position, destination) > 0.02f)
        {
            // Move toward destination at constant speed
            transform.position = Vector3.MoveTowards(
                transform.position,
                destination,
                speed * Time.deltaTime
            );

            // Make sure the sprite doesn't rotate weirdly
            transform.rotation = Quaternion.identity;

            yield return null;
        }

        // Snap exactly on the final tile
        transform.position = destination;
        stats.currentMovement = 0;
    }



    private IEnumerator UseChosenAbility(Ability ability)
    {
        Debug.Log($"{stats.enemyName} uses {ability.abilityName}!");

        // Animation
        if (stats.enemyAnimator && !string.IsNullOrEmpty(stats.castAnimationTrigger))
        {
            stats.enemyAnimator.SetTrigger(stats.castAnimationTrigger);
            yield return new WaitForSeconds(0.25f);
        }

        // Spell slot cost
        if (ability.usesSpellSlot)
            stats.SpendSpellSlots(ability.spellLevel, ability.slotCost);

        yield return ExecuteEnemyAbilityVisuals(ability, targetPlayer);
        yield return new WaitForSeconds(0.2f);
    }

    private IEnumerator ExecuteEnemyAbilityVisuals(Ability ability, Transform target)
    {
        CharacterStats targetStats = target.GetComponent<CharacterStats>();
        Vector3 casterPos = transform.position;
        Vector3 targetPos = target.position;

        // -------------------------------------------------------
        // TELEPORT (Shadow Step)
        // -------------------------------------------------------
        if (ability.abilityName == "Shadow Step")
        {
            if (ability.visualEffectPrefab)
                Instantiate(ability.visualEffectPrefab, casterPos, Quaternion.identity);

            yield return new WaitForSeconds(0.25f);

            // behind the player
            Vector3 dir = (targetPos - casterPos).normalized;
            Vector3 newPos = targetPos - dir * 1.5f;
            transform.position = newPos;

            if (ability.visualEffectPrefab)
                Instantiate(ability.visualEffectPrefab, newPos, Quaternion.identity);

            yield break;
        }

        // -------------------------------------------------------
        // RAY (Life Drain, etc.)
        // -------------------------------------------------------
        if (ability.deliveryType == Ability.DeliveryType.Ray)
        {
            // VFX: stretch a sprite between caster and target
            if (ability.visualEffectPrefab)
            {
                float dist = Vector3.Distance(casterPos, targetPos);
                Vector3 midPoint = (casterPos + targetPos) * 0.5f;

                GameObject vfx = Instantiate(ability.visualEffectPrefab, midPoint, Quaternion.identity);

                // point along the line caster and target
                Vector3 dir = (targetPos - casterPos).normalized;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                vfx.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

                // stretch along X so it visually reaches the target
                float baseLength = 3f; // tweak if sprite’s default length is different
                float stretch = dist / baseLength;
                Vector3 scale = vfx.transform.localScale;
                scale.x *= stretch;
                vfx.transform.localScale = scale;

                // optional: slow the animation a bit so it "lingers"
                Animator anim = vfx.GetComponent<Animator>();
                if (anim != null)
                    anim.speed = 0.8f;   // tweak for more/less linger

                // let the animation play
                yield return new WaitForSeconds(0.6f);
                Destroy(vfx);
            }

            // target might have died / despawned during the VFX
            if (targetStats == null ||
                !targetStats.gameObject.activeInHierarchy ||
                targetStats.currentHealth <= 0)
                yield break;

            // D20 resolve
            bool hit = AbilityExecutor.ResolveAttack(
                ability.baseDamage,
                ability.numberOfDice,
                ability.diceSides,
                ability.scalingAttribute,
                stats,                 // EnemyStats attacker
                targetStats.armorClass,
                out int finalDamage,
                out bool isCrit,
                out bool isMiss
            );

            if (targetStats == null || !targetStats.gameObject.activeInHierarchy)
                yield break;

            int tunedDamage = Mathf.FloorToInt(finalDamage * ENEMY_DAMAGE_MULTIPLIER);

            targetStats.TakeDamage(tunedDamage, isCrit, isMiss);

            // Life drain heal
            if (ability.abilityName == "Life Drain" && !isMiss && tunedDamage > 0)
            {
                int healAmount = Mathf.FloorToInt(tunedDamage * 0.4f); // 40% lifesteal

                // Heal this enemy
                stats.Heal(healAmount);

                // Floating heal text on the enemy
                if (stats.floatingDamagePrefab != null)
                {
                    stats.ShowFloatingText($"+{healAmount}", Color.green);
                }
            }


            // Floating text
            if (targetStats.floatingDamagePrefab != null)
            {
                string text = isMiss ? "MISS" : $"-{tunedDamage}";
                Color color = isCrit ? Color.yellow : Color.red;
                //targetStats.ShowFloatingText(text, color);
            }

            yield break;
        }

        // -------------------------------------------------------
        // PROJECTILE
        // -------------------------------------------------------
        if (ability.deliveryType == Ability.DeliveryType.Projectile)
        {
            GameObject proj = Instantiate(ability.visualEffectPrefab, casterPos, Quaternion.identity);
            float speed = 10f;

            while (proj && Vector3.Distance(proj.transform.position, targetPos) > 0.1f)
            {
                proj.transform.position =
                    Vector3.MoveTowards(proj.transform.position, targetPos, speed * Time.deltaTime);
                yield return null;
            }

            Destroy(proj);

            ResolveDamage(ability, targetStats);
            yield break;
        }

        // -------------------------------------------------------
        // INSTANT MAGIC
        // -------------------------------------------------------
        if (ability.deliveryType == Ability.DeliveryType.Instant)
        {
            if (ability.visualEffectPrefab)
            {
                GameObject vfx = Instantiate(ability.visualEffectPrefab, targetPos, Quaternion.identity);
                Destroy(vfx, 1.0f);
            }

            ResolveDamage(ability, targetStats);
            yield break;
        }



        // -------------------------------------------------------
        // MELEE (BITE, etc.)
        // -------------------------------------------------------
        if (ability.deliveryType == Ability.DeliveryType.Melee)
        {
            if (ability.visualEffectPrefab)
                Instantiate(ability.visualEffectPrefab, targetPos, Quaternion.identity);

            ResolveDamage(ability, targetStats);
            yield break;
        }
    }
    private void ResolveDamage(Ability ability, CharacterStats targetStats)
    {
        // Target vanished, died, or is inactive → stop
        if (targetStats == null ||
            targetStats.currentHealth <= 0 ||
            !targetStats.gameObject.activeInHierarchy)
            return;

        bool hit = AbilityExecutor.ResolveAttack(
            ability.baseDamage,
            ability.numberOfDice,
            ability.diceSides,
            ability.scalingAttribute,
            stats,                    // EnemyStats (attacker)
            targetStats.armorClass,   // Player AC
            out int finalDamage,
            out bool isCrit,
            out bool isMiss
        );

        // If target died or deactivated mid-calc, bail
        if (targetStats == null || !targetStats.gameObject.activeInHierarchy)
            return;

        // Apply enemy tuning multiplier
        int tunedDamage = Mathf.FloorToInt(finalDamage * ENEMY_DAMAGE_MULTIPLIER);

        // Apply damage using tuned value
        targetStats.TakeDamage(tunedDamage, isCrit, isMiss);

        // Floating damage feedback
        if (targetStats.floatingDamagePrefab != null)
        {
            string text = isMiss ? "MISS" : $"-{tunedDamage}";
            Color color = isCrit ? Color.yellow : Color.red;

            //targetStats.ShowFloatingText(text, color);
        }
    }


    private void FaceTarget(Transform target)
    {
        if (target == null) return;

        Vector3 dir = target.position - transform.position;


        transform.rotation = Quaternion.identity;


        if (Mathf.Abs(dir.x) > 0.01f)
        {
            Vector3 scale = transform.localScale;
            float baseX = Mathf.Abs(scale.x);


            if (dir.x >= 0f)
                scale.x = -baseX;
            else
                scale.x = baseX;

            transform.localScale = scale;
        }
    }




    public void ForceRetarget(Transform newTarget)
    {
        targetPlayer = newTarget;
    }

}
