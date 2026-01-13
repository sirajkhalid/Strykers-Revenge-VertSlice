using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAIController : MonoBehaviour
{
    private EnemyStats stats;
    private EnemyAbilityLoadout loadout;
    private TurnManager turnManager;
    private Transform targetPlayer;

    public AIBehaviorProfile behavior;
    private EnemyMovementController mover;

    private Ability lastUsedAbility = null;
    private int lastUsedCount = 0;

    public AudioSource abilityAudioSource;
    private static AudioSource globalAbilitySource;


    private const float ENEMY_DAMAGE_MULTIPLIER = 0.5f;

    void Start()
    {
        stats = GetComponent<EnemyStats>();
        loadout = GetComponent<EnemyAbilityLoadout>();
        turnManager = FindFirstObjectByType<TurnManager>();
        mover = GetComponent<EnemyMovementController>();

        // Always target current active player
        var party = FindFirstObjectByType<PlayerPartyController>();
        if (party != null && party.activeMember != null)
            targetPlayer = party.activeMember.transform;

        if (globalAbilitySource == null)
        {
            GameObject obj = GameObject.Find("AbilitiesSource");
            if (obj != null)
                globalAbilitySource = obj.GetComponent<AudioSource>();
        }

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
        List<Ability> usable = new List<Ability>();

        // Filter abilities that are unlocked and have needed spell slots
        foreach (var a in loadout.abilities)
        {
            if (!a.isUnlocked) continue;

            if (a.usesSpellSlot &&
                !stats.HasSpellSlots(a.spellLevel, a.slotCost))
                continue;

            usable.Add(a);
        }

        if (usable.Count == 0)
            return null;

        // Prevent using same ability 3+ times in a row
        if (lastUsedAbility != null && lastUsedCount >= 2)
        {
            // Remove the last-used ability temporarily
            usable.Remove(lastUsedAbility);
        }

        // If only 1 ability remains after filtering, use it
        if (usable.Count == 1)
            return usable[0];


        // Weighted random choice

        // Example weights: Melee = 2, Ranged = 2, Magic = 3, Utility = 1
        int GetWeight(Ability a)
        {
            switch (a.category)
            {
                case Ability.AbilityCategory.Melee: return 2;
                case Ability.AbilityCategory.Ranged: return 2;
                case Ability.AbilityCategory.Magic: return 3;
                case Ability.AbilityCategory.Utility: return 1;
                default: return 1;
            }
        }

        // Build weighted list
        List<Ability> weightedList = new List<Ability>();
        foreach (var a in usable)
        {
            int w = GetWeight(a);
            for (int i = 0; i < w; i++)
                weightedList.Add(a);
        }

        Ability chosen = weightedList[Random.Range(0, weightedList.Count)];
        return chosen;
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


        float speed = 3.0f; // tweak for faster/slower enemies

        while (Vector3.Distance(transform.position, destination) > 0.02f)
        {
            // Move toward destination at constant speed
            mover.MoveTowards(destination, speed);

            transform.rotation = Quaternion.identity;

            yield return null;
        }

        transform.position = destination;
        stats.currentMovement = 0;
    }



    private IEnumerator UseChosenAbility(Ability ability)
    {

        if (lastUsedAbility == ability)
        {
            lastUsedCount++;
        }
        else
        {
            lastUsedAbility = ability;
            lastUsedCount = 1;
        }
        Debug.Log($"{stats.enemyName} uses {ability.abilityName}!");

        PlayAbilitySound(ability);

        // Animation
        if (stats.enemyAnimator)
        {
            // 50/50 which animation to play
            bool useAltCast = Random.value < 0.5f;

            string triggerToUse = useAltCast
                ? stats.castAnimationTrigger2
                : stats.castAnimationTrigger;

            if (!string.IsNullOrEmpty(triggerToUse))
            {
                stats.enemyAnimator.ResetTrigger(stats.castAnimationTrigger);
                stats.enemyAnimator.ResetTrigger(stats.castAnimationTrigger2);

                stats.enemyAnimator.SetTrigger(triggerToUse);

                yield return new WaitForSeconds(1f);
            }
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
        // AREA DAMAGE (Acid Leak, etc.)
        // -------------------------------------------------------
        if (ability.deliveryType == Ability.DeliveryType.Area)
        {
            PlayerPartyController party = FindFirstObjectByType<PlayerPartyController>();
            if (party == null) yield break;

            GameObject active = party.activeMember;

            // Play VFX at the feet of the active player
            if (ability.visualEffectPrefab && active != null)
            {
                Vector3 pos = active.transform.position + new Vector3(0, -0.3f, 0);
                GameObject vfx = Instantiate(ability.visualEffectPrefab, pos, Quaternion.identity);
                Destroy(vfx, 1f);
            }

            // Damage ONLY the active player (behaves like normal single-target ability)
            CharacterStats activeStats = active.GetComponent<CharacterStats>();
            if (activeStats != null)
            {
                ResolveDamage(ability, activeStats);
            }

            yield break;
        }

        // -------------------------------------------------------
        // RAY (Life Drain, etc.)
        // -------------------------------------------------------
        if (ability.deliveryType == Ability.DeliveryType.Ray)
        {
            if (ability.visualEffectPrefab)
            {
                float dist = Vector3.Distance(casterPos, targetPos);
                Vector3 midPoint = (casterPos + targetPos) * 0.5f;

                GameObject vfx = Instantiate(ability.visualEffectPrefab, midPoint, Quaternion.identity);

                // point along the line caster and target
                Vector3 dir = (targetPos - casterPos).normalized;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                vfx.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

                float baseLength = 3f; // tweak if sprite’s default length is different
                float stretch = dist / baseLength;
                Vector3 scale = vfx.transform.localScale;
                scale.x *= stretch;
                vfx.transform.localScale = scale;

                Animator anim = vfx.GetComponent<Animator>();
                if (anim != null)
                    anim.speed = 0.8f;   // tweak for more/less linger

                yield return new WaitForSeconds(0.6f);
                Destroy(vfx);
            }


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
                // Move
                proj.transform.position =
                    Vector3.MoveTowards(proj.transform.position, targetPos, speed * Time.deltaTime);

                // Rotate toward player
                Vector3 direction = (targetPos - proj.transform.position).normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                proj.transform.rotation = Quaternion.Euler(0f, 0f, angle);

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
            if (ability.visualEffectPrefab && targetStats != null)
            {
                Vector3 vfxPos = targetStats.transform.position; // centered perfectly

                GameObject vfx = Instantiate(
                    ability.visualEffectPrefab,
                    vfxPos,
                    Quaternion.identity
                );

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
            {
                GameObject vfx = Instantiate(
                    ability.visualEffectPrefab,
                    targetPos,
                    Quaternion.identity
                );
                Destroy(vfx, 1.0f); // cleanup
            }

            ResolveDamage(ability, targetStats);
            yield break;
        }
    }
    private void ResolveDamage(Ability ability, CharacterStats targetStats)
    {
        if (targetStats == null ||
            targetStats.currentHealth <= 0 ||
            !targetStats.gameObject.activeInHierarchy)
            return;

        bool hit = AbilityExecutor.ResolveAttack(
            ability.baseDamage,
            ability.numberOfDice,
            ability.diceSides,
            ability.scalingAttribute,
            stats,                    
            targetStats.armorClass,   
            out int finalDamage,
            out bool isCrit,
            out bool isMiss
        );

        if (targetStats == null || !targetStats.gameObject.activeInHierarchy)
            return;

        int tunedDamage = Mathf.FloorToInt(finalDamage * ENEMY_DAMAGE_MULTIPLIER);

        targetStats.TakeDamage(tunedDamage, isCrit, isMiss);

        // Floating damage feedback
        if (targetStats.floatingDamagePrefab != null)
        {
            string text = isMiss ? "MISS" : $"-{tunedDamage}";
            Color color = isCrit ? Color.yellow : Color.red;


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
    private void PlayAbilitySound(Ability ability)
    {
        if (ability == null) return;
        if (ability.abilitySound == null) return;

        if (globalAbilitySource == null)
        {
            GameObject obj = GameObject.Find("AbilitiesSource");
            if (obj != null)
                globalAbilitySource = obj.GetComponent<AudioSource>();
        }

        if (globalAbilitySource != null)
            globalAbilitySource.PlayOneShot(ability.abilitySound);
    }



}
