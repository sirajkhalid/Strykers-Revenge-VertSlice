using DG.Tweening;
using System.Collections;
using UnityEngine;

public class AbilityExecutor : MonoBehaviour
{
    public CharacterStats playerStats;
    public Animator playerAnimator;
    public Camera mainCamera;

    private TurnManager turnManager;
    private PlayerHUDManager hud;
    private PlayerPartyController party;

    void Start()
    {
        turnManager = FindFirstObjectByType<TurnManager>();
        hud = FindFirstObjectByType<PlayerHUDManager>(FindObjectsInactive.Include);
        party = FindFirstObjectByType<PlayerPartyController>();

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    // Always sync active member
    private void RefreshActiveCharacter()
    {
        if (party == null)
            party = FindFirstObjectByType<PlayerPartyController>();

        if (party != null)
        {
            playerStats = party.GetActiveStats();
            if (party.activeMember != null)
                playerAnimator = party.activeMember.GetComponent<Animator>();
        }
    }

    public void ExecuteAbility(Ability ability, Transform target = null)
    {
        StartCoroutine(ExecuteAbilityRoutine(ability, target));
    }

    private IEnumerator ExecuteAbilityRoutine(Ability ability, Transform target)
    {
        if (ability == null) yield break;

        RefreshActiveCharacter();
        if (playerStats == null) yield break;

        // Must be player's turn
        if (turnManager != null && !turnManager.isPlayerTurn) yield break;

        // Must have action/bonus action available
        if (!playerStats.CanUseActionType(ability.actionType)) yield break;

        // --- STEP 1: CHECK RANGE BEFORE ANY COST IS CONSUMED ---
        if (ability.targetType == Ability.TargetType.Enemy)
        {
            Transform locked = FindFirstObjectByType<TargetSelector>()?.GetCurrentTarget();

            if (locked == null)
            {
                ShowOutOfRangeMessage();
                yield break;
            }

            float dist = Vector2.Distance(playerStats.transform.position, locked.position);

            if (dist > ability.range)
            {
                ShowOutOfRangeMessage();
                yield break;
            }
        }

        // --- STEP 2: CHECK SPELL SLOTS (before consuming) ---
        if (ability.usesSpellSlot &&
            !playerStats.HasSpellSlots(ability.spellLevel, ability.slotCost))
        {
            // Optional: show "No Slots" floating text here
            yield break;
        }

        // --- STEP 3: SUCCESS → NOW CONSUME COSTS ---
        playerStats.ConsumeActionType(ability.actionType);

        if (ability.usesSpellSlot)
            playerStats.SpendSpellSlots(ability.spellLevel, ability.slotCost);

        hud?.UpdateActionUI();
        hud?.UpdateSpellSlotUI();

        // Animation
        if (playerAnimator != null &&
            !string.IsNullOrEmpty(playerStats.castAnimationTrigger))
        {
            playerAnimator.ResetTrigger(playerStats.castAnimationTrigger);
            playerAnimator.SetTrigger(playerStats.castAnimationTrigger);
            yield return new WaitForSeconds(0.15f);
        }

        // --- STEP 4: ABILITY EXECUTION ---
        switch (ability.targetType)
        {
            case Ability.TargetType.Self:
                StartCoroutine(ExecuteSelfAbility(ability));
                break;

            case Ability.TargetType.Enemy:
                switch (ability.deliveryType)
                {
                    case Ability.DeliveryType.Melee:
                        StartCoroutine(ExecuteMeleeAbility(ability));
                        break;

                    case Ability.DeliveryType.Projectile:
                        StartCoroutine(FireTowardEnemyOnly(ability));
                        break;

                    case Ability.DeliveryType.Instant:
                        StartCoroutine(ExecuteInstantMagic(ability));
                        break;
                }
                break;

            case Ability.TargetType.Ally:
                StartCoroutine(ExecuteAllyAbility(ability));
                break;

            case Ability.TargetType.Area:
                StartCoroutine(ExecuteAreaAbility(ability));
                break;
        }
    }


    // SELF ABILITY (HEAL, ETC)
    IEnumerator ExecuteSelfAbility(Ability ability)
    {
        if (ability == null || playerStats == null) yield break;

        if (ability.visualEffectPrefab)
        {
            GameObject vfx = Instantiate(ability.visualEffectPrefab, playerStats.transform.position, Quaternion.identity);
            vfx.transform.localScale = Vector3.one * 3f;
            Destroy(vfx, 1.5f);
        }

        int d20Roll = Random.Range(1, 21);
        bool isCrit = d20Roll == 20;
        bool isFail = d20Roll == 1;

        int scaling = GetModifierForScaling(playerStats, ability.scalingAttribute);
        float healRaw = Mathf.Abs(ability.baseDamage) + (d20Roll + scaling) * ability.damageScaling;

        if (isCrit) healRaw *= 2f;
        if (isFail) healRaw *= 0.5f;

        int healAmount = Mathf.RoundToInt(healRaw);

        playerStats.SetCurrentHealth(playerStats.currentHealth + healAmount);

        if (playerStats.floatingDamagePrefab != null)
        {
            Color color = isCrit ? Color.yellow : Color.green;
            string text = isFail ? "MISS HEAL" : $"+{healAmount}";
            playerStats.ShowFloatingText(text, color);
        }
    }

    // PROJECTILE ABILITY
    IEnumerator FireTowardEnemyOnly(Ability ability)
    {
        if (ability.visualEffectPrefab == null || mainCamera == null || playerStats == null)
            yield break;

        TargetSelector selector = FindFirstObjectByType<TargetSelector>();
        Transform lockedTarget = selector?.GetCurrentTarget();

        if (lockedTarget == null) yield break;
        EnemyStats enemyStats = lockedTarget.GetComponent<EnemyStats>();
        if (enemyStats == null) yield break;

        // RANGE CHECK before spawning projectile
        float dist = Vector2.Distance(playerStats.transform.position, lockedTarget.position);
        if (dist > ability.range)
        {
            if (playerStats.outOfRangePrefab)
            {
                GameObject msg = Instantiate(
                    playerStats.outOfRangePrefab,
                    playerStats.transform.position + Vector3.up * 2f,
                    Quaternion.identity
                );
                Destroy(msg, 1f);
            }
            yield break;
        }

        Vector3 spawnPos = playerStats.transform.position;
        Vector3 targetPos = lockedTarget.position;

        GameObject projectile = Instantiate(ability.visualEffectPrefab, spawnPos, Quaternion.identity);
        projectile.transform.localScale = Vector3.one * 3f;

        //kill projectile 
        Destroy(projectile, 2.0f);

        SpriteRenderer sr = projectile.GetComponent<SpriteRenderer>();
        if (sr)
        {
            sr.sortingLayerName = "VFX";
            sr.sortingOrder = 5;
        }

        float speed = 10f;
        Vector3 dir = (targetPos - spawnPos).normalized;

        while (projectile && Vector3.Distance(projectile.transform.position, targetPos) > 0.1f)
        {
            projectile.transform.position += dir * speed * Time.deltaTime;
            projectile.transform.rotation = Quaternion.Euler(
                0,
                0,
                Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg
            );
            yield return null;
        }

        if (projectile != null)
            Destroy(projectile);

        // D20 attack 
        bool hit = ResolveAttack(
            ability.baseDamage,
            ability.numberOfDice,
            ability.diceSides,
            ability.scalingAttribute,
            playerStats,
            enemyStats.armorClass,
            out int finalDamage,
            out bool isCrit,
            out bool isMiss
        );

        enemyStats.TakeDamage(finalDamage, isCrit, isMiss);

        // floating text
        if (enemyStats.floatingDamagePrefab != null)
        {
            string text = isMiss ? "MISS" : $"-{finalDamage}";
            Color color = isCrit ? Color.yellow : Color.red;
            //enemyStats.ShowFloatingText(text, color);
        }
    }


    IEnumerator ExecuteMeleeAbility(Ability ability)
    {
        if (ability == null || playerStats == null) yield break;

        // Play animation
        if (playerAnimator && ability.abilityAnimation)
            playerAnimator.Play(ability.abilityAnimation.name);

        yield return new WaitForSeconds(0.25f);

        // Find Target
        TargetSelector selector = FindFirstObjectByType<TargetSelector>();
        Transform lockedTarget = selector?.GetCurrentTarget();

        EnemyStats closestEnemy = null;

        // If locked target exists and is in range → use it
        if (lockedTarget != null)
        {
            float distToLocked = Vector2.Distance(playerStats.transform.position, lockedTarget.position);
            if (distToLocked <= ability.range)
                closestEnemy = lockedTarget.GetComponent<EnemyStats>();
        }

        // If no locked target OR locked target out of range → find closest enemy within circle
        if (closestEnemy == null)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(playerStats.transform.position, ability.range);
            float closest = Mathf.Infinity;

            foreach (var collider in hits)
            {
                EnemyStats e = collider.GetComponent<EnemyStats>();
                if (e != null)
                {
                    float dist = Vector2.Distance(playerStats.transform.position, e.transform.position);
                    if (dist < closest)
                    {
                        closest = dist;
                        closestEnemy = e;
                    }
                }
            }
        }

        // if out of range
        if (!closestEnemy)
        {
            ShowOutOfRangeMessage();
            yield break;
        }

        // Safety Check
        float finalDist = Vector2.Distance(
            playerStats.transform.position,
            closestEnemy.transform.position
        );

        if (finalDist > ability.range)
        {
            ShowOutOfRangeMessage();
            yield break;
        }

        // Spawn VFX
        if (ability.visualEffectPrefab)
        {
            GameObject vfx = Instantiate(
                ability.visualEffectPrefab,
                closestEnemy.transform.position,
                Quaternion.identity
            );
            Destroy(vfx, 1f);
        }

        // Damage Calculation
        bool hit = AbilityExecutor.ResolveAttack(
            ability.baseDamage,
            ability.numberOfDice,
            ability.diceSides,
            ability.scalingAttribute,
            playerStats,
            closestEnemy.armorClass,
            out int finalDamage,
            out bool isCrit,
            out bool isMiss
        );

        // Utility abilities that deal no damage
        if (ability.baseDamage == 0 && ability.numberOfDice == 0)
        {
            finalDamage = 0;
            isCrit = false;
            isMiss = false;
        }

        closestEnemy.TakeDamage(finalDamage, isCrit, isMiss);

        if (!isMiss && ability.abilityName == "Push")
        {
            Vector3 dir = (closestEnemy.transform.position - playerStats.transform.position).normalized;
            float pushDistance = 5.0f;

            Vector3 startPos = closestEnemy.transform.position;
            Vector3 targetPos = startPos + dir * pushDistance;

            // DOTween smooth movement
            closestEnemy.transform
                .DOMove(targetPos, 0.35f)
                .SetEase(Ease.OutQuad);
        }
    }

    // INSTANT MAGIC (Spawn on enemy)
    IEnumerator ExecuteInstantMagic(Ability ability)
    {
        if (ability == null || playerStats == null) yield break;

        // Get locked target
        TargetSelector selector = FindFirstObjectByType<TargetSelector>();
        Transform lockedTarget = selector?.GetCurrentTarget();
        if (lockedTarget == null) yield break;

        EnemyStats enemyStats = lockedTarget.GetComponent<EnemyStats>();
        if (enemyStats == null) yield break;

        // RANGE CHECK
        float dist = Vector2.Distance(playerStats.transform.position, lockedTarget.position);
        if (dist > ability.range)
        {
            if (playerStats.outOfRangePrefab)
            {
                GameObject msg = Instantiate(
                    playerStats.outOfRangePrefab,
                    playerStats.transform.position + Vector3.up * 2f,
                    Quaternion.identity
                );
                Destroy(msg, 1f);
            }
            yield break;
        }

        // Spawn VFX at enemy
        if (ability.visualEffectPrefab)
        {
            GameObject vfx = Instantiate(
                ability.visualEffectPrefab,
                lockedTarget.position,
                Quaternion.identity
            );
            vfx.transform.localScale = Vector3.one * 3f;
            Destroy(vfx, 1.5f);
        }

        // D20 attack
        bool hit = ResolveAttack(
            ability.baseDamage,
            ability.numberOfDice,
            ability.diceSides,
            ability.scalingAttribute,
            playerStats,           // attacker
            enemyStats.armorClass, // enemy AC
            out int finalDamage,
            out bool isCrit,
            out bool isMiss
        );

        // Apply damage
        enemyStats.TakeDamage(finalDamage, isCrit, isMiss);

        // Floating damage text
        if (enemyStats.floatingDamagePrefab != null)
        {
            string text = isMiss ? "MISS" : $"-{finalDamage}";
            Color color = isCrit ? Color.yellow : Color.red;
            //enemyStats.ShowFloatingText(text, color);
        }
    }



    // AREA (AOE)
    IEnumerator ExecuteAreaAbility(Ability ability)
    {
        if (mainCamera == null) yield break;

        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        if (ability.visualEffectPrefab)
        {
            GameObject vfx = Instantiate(ability.visualEffectPrefab, mouseWorld, Quaternion.identity);
            Destroy(vfx, 1.5f);
        }

        //  Only apply Bless to player party members 
        var party = FindFirstObjectByType<PlayerPartyController>();
        if (party == null) yield break;

        foreach (var member in party.partyMembers)
        {
            CharacterStats ally = member.GetComponent<CharacterStats>();
            if (ally == null) continue;

            if (Vector3.Distance(mouseWorld, ally.transform.position) <= ability.areaRadius)
            {
                var effect = ally.GetComponent<StatusEffectManager>();
                if (effect != null && ability.statusEffectName == "Blessed")
                    effect.ApplyBless(ability.statusDuration);
            }
        }
    }


    // Ally single-target
    IEnumerator ExecuteAllyAbility(Ability ability)
    {
        yield break; // Not implemented yet but prevents errors
    }

    private int GetModifierForScaling(CharacterStats stats, Ability.ScalingAttribute scaling)
    {
        switch (scaling)
        {
            case Ability.ScalingAttribute.Strength: return stats.strength;
            case Ability.ScalingAttribute.Dexterity: return stats.dexterity;
            case Ability.ScalingAttribute.Intelligence: return stats.intelligence;
            case Ability.ScalingAttribute.Wisdom: return stats.wisdom;
            case Ability.ScalingAttribute.Charisma: return stats.charisma;
            case Ability.ScalingAttribute.Constitution: return stats.constitution;
            default: return 0;
        }
    }

    public static bool ResolveAttack(
       int baseDamage,
       int diceCount,
       int diceSides,
       Ability.ScalingAttribute scaling,
       object attackerStats,
       int targetAC,
       out int finalDamage,
       out bool isCrit,
       out bool isMiss)
    {
        finalDamage = 0;
        isCrit = false;
        isMiss = false;

        // roll d20
        int roll = D20System.RollD20();

        bool nat20 = roll == 20;
        bool nat1 = roll == 1;

        // determine scaling modifier
        int GetStatFromSource(Ability.ScalingAttribute attr, object src)
        {
            if (src is CharacterStats cs)
            {
                return attr switch
                {
                    Ability.ScalingAttribute.Strength => cs.strength,
                    Ability.ScalingAttribute.Dexterity => cs.dexterity,
                    Ability.ScalingAttribute.Constitution => cs.constitution,
                    Ability.ScalingAttribute.Intelligence => cs.intelligence,
                    Ability.ScalingAttribute.Wisdom => cs.wisdom,
                    Ability.ScalingAttribute.Charisma => cs.charisma,
                    _ => 0
                };
            }

            if (src is EnemyStats es)
            {
                return attr switch
                {
                    Ability.ScalingAttribute.Strength => es.strength,
                    Ability.ScalingAttribute.Dexterity => es.dexterity,
                    Ability.ScalingAttribute.Constitution => es.constitution,
                    Ability.ScalingAttribute.Intelligence => es.intelligence,
                    Ability.ScalingAttribute.Wisdom => es.wisdom,
                    Ability.ScalingAttribute.Charisma => es.charisma,
                    _ => 0
                };
            }

            return 0;
        }

        int attackMod = GetStatFromSource(scaling, attackerStats);

        // nat 1 = miss
        if (nat1)
        {
            isMiss = true;
            finalDamage = 0;
            return false;
        }

        // critical hits
        if (nat20)
        {
            isCrit = true;

            // If this is a utility / non-damage ability → still 0 dmg
            if (baseDamage == 0 && diceCount == 0)
            {
                finalDamage = 0;
                return true;
            }

            int diceDamage = diceCount > 0
                ? D20System.RollDice(diceCount * 2, diceSides)   // double dice
                : 0;

            finalDamage = Mathf.Max(
                0,             // allow 0-damage abilities
                baseDamage + diceDamage + attackMod
            );

            return true;
        }

        // Normal Hit check
        int totalToHit = roll + attackMod;
        bool hit = totalToHit >= targetAC;

        if (!hit)
        {
            isMiss = true;
            finalDamage = 0;
            return false;
        }

        // Normal Damage
        int normalDice = diceCount > 0 ? D20System.RollDice(diceCount, diceSides) : 0;

        // Special case: 0-damage utility abilities
        if (baseDamage == 0 && diceCount == 0)
        {
            finalDamage = 0;
            return true;
        }

        finalDamage = Mathf.Max(
            0,      // allow 0 damage
            baseDamage + normalDice + attackMod
        );

        return true;
    }
    private void ShowOutOfRangeMessage()
    {
        if (playerStats != null && playerStats.outOfRangePrefab != null)
        {
            GameObject msg = Instantiate(
                playerStats.outOfRangePrefab,
                playerStats.transform.position + Vector3.up * 2f,
                Quaternion.identity
            );
            Destroy(msg, 1f);
        }
    }
    private void AbilityExecutor_ShowOutOfRangePopup()
    {
        if (playerStats == null || playerStats.outOfRangePrefab == null)
            return;

        GameObject msg = Instantiate(
            playerStats.outOfRangePrefab,
            playerStats.transform.position + Vector3.up * 2f,
            Quaternion.identity
        );

    }


}
