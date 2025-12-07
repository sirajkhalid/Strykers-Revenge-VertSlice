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
        if (ability == null)
            return;

        StartCoroutine(ExecuteAbilityRoutine(ability, target));
    }

    private IEnumerator ExecuteAbilityRoutine(Ability ability, Transform target)
    {
        if (ability == null) yield break;

        RefreshActiveCharacter();
        if (playerStats == null) yield break;

        // Must be player's turn
        if (turnManager != null && !turnManager.isPlayerTurn) yield break;

        // --- SNEAK RESTRICTIONS ---
        if (playerStats.isSneaking)
        {
            // Only Sneak Attack allowed
            if (ability.abilityName != "Sneak Attack")
            {
                ShowCustomMessage("You cannot act while Sneaking!");
                yield break;
            }
        }

        // Must have action/bonus action available
        if (!playerStats.CanUseActionType(ability.actionType))
        {
            ShowNoActionMessage();
            yield break;
        }

        // --- PER-BATTLE USAGE LIMIT CHECK ---
        if (ability.maxUsesPerBattle > 0 &&
            ability.usesThisBattle >= ability.maxUsesPerBattle)
        {
            ShowNoUsesLeftMessage();
            yield break;
        }

        // --- STEP 1: CHECK RANGE / TARGET BEFORE COSTS ---
        if (ability.targetType == Ability.TargetType.Enemy)
        {
            Transform locked = FindFirstObjectByType<TargetSelector>()?.GetCurrentTarget();

            if (locked == null)
            {
                ShowSelectTargetMessage();
                yield break;
            }

            float dist = Vector2.Distance(playerStats.transform.position, locked.position);

            if (dist > ability.range)
            {
                ShowOutOfRangeMessage();
                yield break;
            }
        }

        // --- STEP 2: CHECK SPELL SLOTS ---
        if (ability.usesSpellSlot &&
            !playerStats.HasSpellSlots(ability.spellLevel, ability.slotCost))
        {
            ShowNoSpellSlotsMessage();
            yield break;
        }

        // --- STEP 3: CONSUME COSTS ---
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

                // Custom self instant abilities
                if (ability.deliveryType == Ability.DeliveryType.Instant)
                {
                    if (ability.abilityName == "Sneak")
                    {
                        StartCoroutine(ExecuteSneak(ability));
                        break;
                    }
                    if (ability.abilityName == "Fancy Footwork")
                    {
                        StartCoroutine(ExecuteFancyFootwork(ability));
                        break;
                    }
                    if (ability.abilityName == "Calm Mind" || ability.abilityName == "Spellbook Restore")
                    {
                        StartCoroutine(ExecuteCalmMind(ability));
                        break;
                    }
                }

                StartCoroutine(ExecuteSelfAbility(ability));
                break;

            case Ability.TargetType.Enemy:

                if (ability.deliveryType == Ability.DeliveryType.Area)
                {
                    StartCoroutine(ExecuteAreaAbility(ability));
                    break;
                }

                switch (ability.deliveryType)
                {
                    case Ability.DeliveryType.Melee:
                        StartCoroutine(ExecuteMeleeAbility(ability));
                        break;

                    case Ability.DeliveryType.Projectile:
                        StartCoroutine(FireTowardEnemyOnly(ability));
                        break;

                    case Ability.DeliveryType.Instant:
                        if (ability.abilityName == "Sneak Attack")
                        {
                            if (!playerStats.isSneaking)
                            {
                                ShowCustomMessage("You must be Sneaking!");
                                yield break;
                            }
                            StartCoroutine(ExecuteSneakAttack(ability));
                            yield break;
                        }
                        if (ability.abilityName == "Dash Attack")
                        {
                            StartCoroutine(ExecuteDashAttack(ability));
                            break;
                        }
                        StartCoroutine(ExecuteInstantMagic(ability));
                        break;

                    case Ability.DeliveryType.Ray:
                        StartCoroutine(ExecuteRayAbility(ability));
                        break;
                }
                break;

            case Ability.TargetType.Area:
                StartCoroutine(ExecuteAreaAbility(ability));
                break;

            case Ability.TargetType.Ally:
                StartCoroutine(ExecuteAllyAbility(ability));
                break;
        }

        ability.usesThisBattle++;

    }

    // ============================================================
    // SELF ABILITY (HEAL, ETC)
    // ============================================================
    IEnumerator ExecuteSelfAbility(Ability ability)
    {
        if (ability == null || playerStats == null)
            yield break;

        // SPECIAL CASE: BARRIER
        if (ability.abilityName == "Barrier")
        {
            var effects = playerStats.GetComponent<StatusEffectManager>();

            if (effects != null)
            {
                effects.ApplyBarrier(
                    rounds: 3,
                    damageReduction: 0.5f,
                    vfxPrefab: ability.visualEffectPrefab
                );
            }

            yield break;
        }

        // NORMAL SELF ABILITIES
        if (ability.visualEffectPrefab)
        {
            GameObject vfx = Instantiate(
                ability.visualEffectPrefab,
                playerStats.transform.position,
                Quaternion.identity
            );

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
            string text = isFail ? "MISS HEAL" : "+" + healAmount;
            playerStats.ShowFloatingText(text, color);
        }
    }

    // ============================================================
    // PROJECTILE ABILITY
    // ============================================================
    IEnumerator FireTowardEnemyOnly(Ability ability)
    {
        if (ability.visualEffectPrefab == null || mainCamera == null || playerStats == null)
            yield break;

        TargetSelector selector = FindFirstObjectByType<TargetSelector>();
        Transform lockedTarget = selector?.GetCurrentTarget();

        if (lockedTarget == null)
        {
            ShowSelectTargetMessage();
            yield break;
        }

        EnemyStats enemyStats = lockedTarget.GetComponent<EnemyStats>();
        if (enemyStats == null) yield break;

        // RANGE CHECK
        float dist = Vector2.Distance(playerStats.transform.position, lockedTarget.position);
        if (dist > ability.range)
        {
            ShowOutOfRangeMessage();
            yield break;
        }

        Vector3 spawnPos = playerStats.transform.position;
        Vector3 targetPos = lockedTarget.position;

        GameObject projectile = Instantiate(ability.visualEffectPrefab, spawnPos, Quaternion.identity);
        projectile.transform.localScale = Vector3.one * 3f;

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
    }

    // ============================================================
    // MELEE ABILITY (incl. Wide Slash + Push)
    // ============================================================
    IEnumerator ExecuteMeleeAbility(Ability ability)
    {
        if (ability == null || playerStats == null)
            yield break;

        // USAGE LIMIT CHECK
        if (ability.maxUsesPerBattle > 0 &&
            ability.usesThisBattle >= ability.maxUsesPerBattle)
        {
            ShowNoUsesLeftMessage();
            yield break;
        }

        // SPECIAL CASE: WIDE SLASH
        if (ability.abilityName == "Wide Slash")
        {
            StartCoroutine(ExecuteWideSlash(ability));
            yield break;
        }

        // PLAY ANIMATION
        if (playerAnimator && ability.abilityAnimation)
            playerAnimator.Play(ability.abilityAnimation.name);

        yield return new WaitForSeconds(0.25f);

        // FIND TARGET (locked → closest)
        TargetSelector selector = FindFirstObjectByType<TargetSelector>();
        Transform lockedTarget = selector?.GetCurrentTarget();

        EnemyStats closestEnemy = null;

        if (lockedTarget != null)
        {
            if (Vector2.Distance(playerStats.transform.position, lockedTarget.position) <= ability.range)
                closestEnemy = lockedTarget.GetComponent<EnemyStats>();
        }

        if (closestEnemy == null)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(playerStats.transform.position, ability.range);
            float closest = Mathf.Infinity;

            foreach (Collider2D c in hits)
            {
                EnemyStats e = c.GetComponent<EnemyStats>();
                if (e == null) continue;

                float d = Vector2.Distance(playerStats.transform.position, e.transform.position);
                if (d < closest)
                {
                    closest = d;
                    closestEnemy = e;
                }
            }
        }

        if (closestEnemy == null)
        {
            ShowOutOfRangeMessage();
            yield break;
        }

        float finalDist = Vector2.Distance(playerStats.transform.position, closestEnemy.transform.position);
        if (finalDist > ability.range)
        {
            ShowOutOfRangeMessage();
            yield break;
        }

        // NORMAL MELEE VFX
        if (ability.visualEffectPrefab)
        {
            GameObject vfx = Instantiate(
                ability.visualEffectPrefab,
                closestEnemy.transform.position,
                Quaternion.identity
            );
            Destroy(vfx, 1f);
        }

        // TRIPLE-HIT OR SINGLE
        int hitCount = ability.abilityName.Contains("Triple") ? 3 : 1;

        for (int i = 0; i < hitCount; i++)
        {
            bool hit = ResolveAttack(
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

            closestEnemy.TakeDamage(finalDamage, isCrit, isMiss);

            if (hitCount > 1)
                yield return new WaitForSeconds(0.1f);
        }

        ability.usesThisBattle++;

        // OPTIONAL KNOCKBACK
        if (ability.specialEffect == Ability.SpecialEffect.Knockback)
        {
            Rigidbody2D enemyRb = closestEnemy.GetComponent<Rigidbody2D>();

            if (enemyRb != null)
            {
                Vector2 pushDir =
                    (enemyRb.position - (Vector2)playerStats.transform.position).normalized;

                float pushDistance = 3.5f;
                float pushDuration = 0.15f;

                Vector2 safePos = GetSafePushPosition(enemyRb, pushDir, pushDistance);

                StartCoroutine(SmoothKnockback(enemyRb, enemyRb.position, safePos, pushDuration));
            }
        }
    }

    // ============================================================
    // RAY ABILITY (e.g. Dragon Breath)
    // ============================================================
    private IEnumerator ExecuteRayAbility(Ability ability)
    {
        TargetSelector selector = FindFirstObjectByType<TargetSelector>();
        Transform lockedTarget = selector?.GetCurrentTarget();
        if (lockedTarget == null || playerStats == null)
        {
            ShowSelectTargetMessage();
            yield break;
        }

        Vector2 playerPos = playerStats.transform.position;
        Vector2 targetPos = lockedTarget.position;

        Vector2 dir = (targetPos - playerPos).normalized;
        float dist = Vector2.Distance(playerPos, targetPos);

        // RANGE CHECK
        if (dist > ability.range)
        {
            ShowOutOfRangeMessage();
            yield break;
        }

        float beamLength = Mathf.Min(dist, ability.range);

        // SPAWN BEAM VFX
        GameObject beam = null;

        if (ability.visualEffectPrefab)
        {
            beam = Instantiate(ability.visualEffectPrefab, playerPos, Quaternion.identity);

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            beam.transform.rotation = Quaternion.Euler(0, 0, angle);

            SpriteRenderer sr = beam.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                float spriteWidth = sr.bounds.size.x;
                float scaleX = beamLength / spriteWidth;
                beam.transform.localScale = new Vector3(scaleX, beam.transform.localScale.y, 1f);
            }
            else
            {
                beam.transform.localScale = new Vector3(beamLength, 1f, 1f);
            }
        }

        yield return new WaitForSeconds(0.1f);

        // HIT ALL ENEMIES IN BEAM
        int mask = ~0;
        RaycastHit2D[] hits = Physics2D.RaycastAll(playerPos, dir, ability.range, mask);

        foreach (var hit in hits)
        {
            EnemyStats es = hit.collider?.GetComponent<EnemyStats>();
            if (es == null)
                continue;

            ResolveAttack(
                ability.baseDamage,
                ability.numberOfDice,
                ability.diceSides,
                ability.scalingAttribute,
                playerStats,
                es.armorClass,
                out int finalDamage,
                out bool isCrit,
                out bool isMiss
            );

            es.TakeDamage(finalDamage, isCrit, isMiss);
        }

        if (beam != null)
            Destroy(beam, 1.5f);
    }

    // ============================================================
    // INSTANT MAGIC (spawn at enemy)
    // ============================================================
    IEnumerator ExecuteInstantMagic(Ability ability)
    {
        if (ability == null || playerStats == null) yield break;

        TargetSelector selector = FindFirstObjectByType<TargetSelector>();
        Transform lockedTarget = selector?.GetCurrentTarget();
        if (lockedTarget == null)
        {
            ShowSelectTargetMessage();
            yield break;
        }

        EnemyStats enemyStats = lockedTarget.GetComponent<EnemyStats>();
        if (enemyStats == null) yield break;

        float dist = Vector2.Distance(playerStats.transform.position, lockedTarget.position);
        if (dist > ability.range)
        {
            ShowOutOfRangeMessage();
            yield break;
        }

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
    }

    private IEnumerator ExecuteAreaAbility(Ability ability)
    {
        if (ability == null || ability.visualEffectPrefab == null)
            yield break;

        RefreshActiveCharacter();
        if (playerStats == null)
            yield break;

        if (mainCamera == null)
            mainCamera = Camera.main;

        // 1. Determine where to cast
        Vector3 castPos;

        // If an enemy is selected → cast on that enemy
        TargetSelector selector = FindFirstObjectByType<TargetSelector>();
        Transform lockedTarget = selector?.GetCurrentTarget();

        if (lockedTarget != null)
        {
            castPos = lockedTarget.position;
        }
        else
        {
            // Otherwise cast where the mouse clicks
            Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;
            castPos = mouseWorld;
        }

        // 2. Spawn the VFX prefab
        GameObject aoe = Instantiate(
            ability.visualEffectPrefab,
            castPos,
            Quaternion.identity
        );

        // Let animator/collider initialize
        yield return null;

        Collider2D[] results = new Collider2D[20];
        ContactFilter2D filter = ContactFilter2D.noFilter;

        int count = aoe.GetComponent<Collider2D>().Overlap(filter, results);

        // 3. Damage all enemies touched by the hitbox
        for (int i = 0; i < count; i++)
        {
            Collider2D c = results[i];
            if (c == null) continue;

            EnemyStats enemy = c.GetComponent<EnemyStats>();
            if (enemy == null) continue;

            ResolveAttack(
                ability.baseDamage,
                ability.numberOfDice,
                ability.diceSides,
                ability.scalingAttribute,
                playerStats,
                enemy.armorClass,
                out int finalDamage,
                out bool isCrit,
                out bool isMiss
            );

            enemy.TakeDamage(finalDamage, isCrit, isMiss);
        }

        Destroy(aoe, 1.5f);
    }


    // ============================================================
    // Ally single-target (placeholder)
    // ============================================================
    IEnumerator ExecuteAllyAbility(Ability ability)
    {
        yield break;
    }

    // ============================================================
    // HELPERS
    // ============================================================
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

        int roll = D20System.RollD20();

        bool nat20 = roll == 20;
        bool nat1 = roll == 1;

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

        if (nat1)
        {
            isMiss = true;
            finalDamage = 0;
            return false;
        }

        if (nat20)
        {
            isCrit = true;

            if (baseDamage == 0 && diceCount == 0)
            {
                finalDamage = 0;
                return true;
            }

            int diceDamage = diceCount > 0
                ? D20System.RollDice(diceCount * 2, diceSides)
                : 0;

            finalDamage = Mathf.Max(
                0,
                baseDamage + diceDamage + attackMod
            );

            return true;
        }

        int totalToHit = roll + attackMod;
        bool hit = totalToHit >= targetAC;

        if (!hit)
        {
            isMiss = true;
            finalDamage = 0;
            return false;
        }

        int normalDice = diceCount > 0 ? D20System.RollDice(diceCount, diceSides) : 0;

        if (baseDamage == 0 && diceCount == 0)
        {
            finalDamage = 0;
            return true;
        }

        finalDamage = Mathf.Max(
            0,
            baseDamage + normalDice + attackMod
        );

        return true;
    }

    // ----------------- POPUP HELPERS -----------------
    private void ShowCustomMessage(string text)
    {
        if (playerStats == null || playerStats.outOfRangePrefab == null)
            return;

        GameObject msg = Instantiate(
            playerStats.outOfRangePrefab,
            playerStats.transform.position + Vector3.up * 2f,
            Quaternion.identity
        );

        var tmp = msg.GetComponentInChildren<TMPro.TMP_Text>();
        if (tmp != null)
            tmp.text = text;

        Destroy(msg, 1f);
    }

    private void ShowOutOfRangeMessage()
    {
        ShowCustomMessage("Out of range!");
    }

    private void ShowSelectTargetMessage()
    {
        ShowCustomMessage("Select a target!");
    }

    private void ShowSelectAreaMessage()
    {
        ShowCustomMessage("Select an area!");
    }

    private void ShowNoActionMessage()
    {
        ShowCustomMessage("No actions available!");
    }

    public void ShowNoUsesLeftMessage()
    {
        ShowCustomMessage("No Uses Left!");
    }

    public void ShowNoSpellSlotsMessage()
    {
        ShowCustomMessage("No Spell Slots!");
    }

    // ----------------- KNOCKBACK HELPERS -----------------
    private Vector2 GetSafePushPosition(Rigidbody2D enemyRb, Vector2 direction, float distance)
    {
        Vector2 start = enemyRb.position;
        Vector2 end = start + direction * distance;

        RaycastHit2D hit = Physics2D.Raycast(start, direction, distance, LayerMask.GetMask("Player", "Walls", "Enemies"));

        if (hit.collider != null)
        {
            return hit.point - direction * 0.1f;
        }

        return end;
    }

    private IEnumerator SmoothKnockback(Rigidbody2D rb, Vector2 start, Vector2 target, float duration)
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;

            Vector2 next = Vector2.Lerp(start, target, t);
            rb.MovePosition(next);

            yield return null;
        }
    }

    // ============================================================
    // WIDE SLASH
    // ============================================================
    private IEnumerator ExecuteWideSlash(Ability ability)
    {
        Vector3 playerPos = playerStats.transform.position;

        TargetSelector selector = FindFirstObjectByType<TargetSelector>();
        Transform lockedTarget = selector?.GetCurrentTarget();

        Vector3 dir = lockedTarget != null ?
            (lockedTarget.position - playerPos).normalized :
            Vector3.right;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        GameObject slash = null;

        if (ability.visualEffectPrefab)
        {
            slash = Instantiate(ability.visualEffectPrefab, playerPos, Quaternion.identity);
            slash.transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // Allow collider to initialize
        yield return null;

        Collider2D[] results = new Collider2D[20];
        ContactFilter2D filter = ContactFilter2D.noFilter;

        int count = slash.GetComponent<Collider2D>().Overlap(filter, results);

        for (int i = 0; i < count; i++)
        {
            Collider2D hit = results[i];
            if (hit == null) continue;

            EnemyStats enemy = hit.GetComponent<EnemyStats>();
            if (enemy == null) continue;

            ResolveAttack(
                ability.baseDamage,
                ability.numberOfDice,
                ability.diceSides,
                ability.scalingAttribute,
                playerStats,
                enemy.armorClass,
                out int finalDamage,
                out bool isCrit,
                out bool isMiss
            );

            Time.timeScale = 0.35f;
            yield return new WaitForSecondsRealtime(0.08f);
            Time.timeScale = 1f;

            enemy.TakeDamage(finalDamage, isCrit, isMiss);
        }

        if (slash != null)
            Destroy(slash, 0.35f);
    }

    // ===============================================
    // DASH ATTACK (Teleport behind enemy and strike)
    // ===============================================
    private IEnumerator ExecuteDashAttack(Ability ability)
    {
        RefreshActiveCharacter();
        if (playerStats == null) yield break;

        // Must have a target
        TargetSelector selector = FindFirstObjectByType<TargetSelector>();
        Transform lockedTarget = selector?.GetCurrentTarget();

        if (lockedTarget == null)
        {
            ShowSelectTargetMessage();
            yield break;
        }

        EnemyStats enemy = lockedTarget.GetComponent<EnemyStats>();
        if (enemy == null)
        {
            ShowSelectTargetMessage();
            yield break;
        }

        // RANGE CHECK
        float dist = Vector2.Distance(playerStats.transform.position, lockedTarget.position);
        if (dist > ability.range)
        {
            ShowOutOfRangeMessage();
            yield break;
        }

        Vector3 startPos = playerStats.transform.position;
        Vector3 enemyPos = lockedTarget.position;

        // Direction from enemy → player
        Vector3 dir = (playerStats.transform.position - enemyPos).normalized;

        // How far behind the enemy the player should appear
        float dashDistance = 10f;   
                                     

        Vector3 endPos = enemyPos + (-dir * dashDistance);
        endPos.z = 0f;   // ensure stays in 2D plane

        // Play VFX at start
        if (ability.visualEffectPrefab != null)
        {
            GameObject startFX = Instantiate(ability.visualEffectPrefab, startPos, Quaternion.identity);
            Destroy(startFX, 1f);
        }

        // Instantly teleport player
        playerStats.transform.position = endPos;

        // Play VFX at destination
        if (ability.visualEffectPrefab != null)
        {
            GameObject endFX = Instantiate(ability.visualEffectPrefab, endPos, Quaternion.identity);
            Destroy(endFX, 1f);
        }

        // ------------------------------------------
        // DAMAGE CALCULATION
        // ------------------------------------------
        ResolveAttack(
            ability.baseDamage,
            ability.numberOfDice,
            ability.diceSides,
            ability.scalingAttribute,
            playerStats,
            enemy.armorClass,
            out int finalDamage,
            out bool isCrit,
            out bool isMiss
        );

        enemy.TakeDamage(finalDamage, isCrit, isMiss);

        yield return null;
    }

    // =====================================================================
    // FANCY FOOTWORK (Refill movement, play VFX above player, no damage)
    // =====================================================================
    private IEnumerator ExecuteFancyFootwork(Ability ability)
    {
        RefreshActiveCharacter();
        if (playerStats == null) yield break;

        // Refill movement
        playerStats.currentMovement = playerStats.maxMovement;

        // Play the VFX above player's head
        if (ability.visualEffectPrefab != null)
        {
            Vector3 spawnPos = playerStats.transform.position + Vector3.up * 3.0f;
            // ^ Adjust 1.0f if needed, but your pivot should already align this perfectly

            GameObject fx = Instantiate(
                ability.visualEffectPrefab,
                spawnPos,
                Quaternion.identity
            );

            // Do NOT scale it — use prefab scale
            Destroy(fx, 1.5f);
        }

        // Optional: Show message
        //playerStats.ShowFloatingText("Movement Restored", Color.lightGoldenRodYellow);

        yield return null;
    }

    private IEnumerator ExecuteSneak(Ability ability)
    {
        RefreshActiveCharacter();
        if (playerStats == null) yield break;

        // Play VFX at player (keep prefab scale)
        if (ability.visualEffectPrefab != null)
        {
            GameObject fx = Instantiate(
                ability.visualEffectPrefab,
                playerStats.transform.position,
                Quaternion.identity
            );

            fx.transform.localScale = ability.visualEffectPrefab.transform.localScale;

            Destroy(fx, 1.5f);
        }

        // Activate Sneak state
        playerStats.isSneaking = true;
        playerStats.isImmune = true;

        // Fade sprite renderer
        SpriteRenderer sr = playerStats.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.35f);

        // Disable all abilities except Sneak Attack
        AbilityUIController abilityUI = FindFirstObjectByType<AbilityUIController>();
        if (abilityUI != null)
            abilityUI.LockAllAbilitiesExcept("Sneak Attack");

        // Sneak lasts TWO FULL ROUNDS
        int turnsToWait = 2;

        while (turnsToWait > 0)
        {
            // Wait until player turn ends
            while (turnManager.isPlayerTurn)
                yield return null;

            // Wait until player turn begins again
            while (!turnManager.isPlayerTurn)
                yield return null;

            turnsToWait--;
        }

        // END SNEAK
        playerStats.isSneaking = false;
        playerStats.isImmune = false;

        if (sr != null)
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);

        if (abilityUI != null)
            abilityUI.UnlockAllAbilities();
    }


    private IEnumerator ExecuteSneakAttack(Ability ability)
    {
        RefreshActiveCharacter();
        if (playerStats == null) yield break;

        TargetSelector selector = FindFirstObjectByType<TargetSelector>();
        Transform target = selector?.GetCurrentTarget();
        if (target == null)
        {
            ShowSelectTargetMessage();
            yield break;
        }

        EnemyStats enemy = target.GetComponent<EnemyStats>();
        if (enemy == null) yield break;

        float dist = Vector2.Distance(playerStats.transform.position, target.position);
        if (dist > ability.range)
        {
            ShowOutOfRangeMessage();
            yield break;
        }

        // Normal slash VFX
        if (ability.visualEffectPrefab)
        {
            GameObject fx = Instantiate(
                ability.visualEffectPrefab,
                enemy.transform.position,
                Quaternion.identity
            );
            Destroy(fx, 1f);
        }

        // Two damage rolls
        for (int i = 0; i < 2; i++)
        {
            ResolveAttack(
                ability.baseDamage,
                ability.numberOfDice,
                ability.diceSides,
                ability.scalingAttribute,
                playerStats,
                enemy.armorClass,
                out int finalDamage,
                out bool isCrit,
                out bool isMiss
            );

            enemy.TakeDamage(finalDamage, isCrit, isMiss);
            yield return new WaitForSeconds(0.15f);
        }

        // End Sneak mode after Sneak Attack
        playerStats.isSneaking = false;
        playerStats.isImmune = false;

        SpriteRenderer sr = playerStats.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);

        AbilityUIController abilityUI = FindFirstObjectByType<AbilityUIController>();
        if (abilityUI != null)
            abilityUI.UnlockAllAbilities();
    }

    private IEnumerator ExecuteCalmMind(Ability ability)
    {
        RefreshActiveCharacter();
        if (playerStats == null) yield break;

        // --- PLAY VFX IN FRONT OF PLAYER ---
        if (ability.visualEffectPrefab != null)
        {
            Vector3 spawnPos = playerStats.transform.position;

            // Push slightly in front of character (toward camera)
            spawnPos.z -= 0.1f;

            GameObject fx = Instantiate(
                ability.visualEffectPrefab,
                spawnPos,
                Quaternion.identity
            );

            // Use prefab's original scale
            fx.transform.localScale = ability.visualEffectPrefab.transform.localScale;

            Destroy(fx, 1.5f);
        }

        // --- RESTORE SPELL SLOTS ---
        playerStats.RestoreAllSpellSlots();

        // Update HUD
        var hud = FindFirstObjectByType<PlayerHUDManager>(FindObjectsInactive.Include);
        if (hud != null)
            hud.UpdateSpellSlotUI();

        // --- HANDLE LIFETIME USES ---
        if (ability.MaxLifetimeUses > 0)
        {
            ability.LifetimeUses++;

            if (ability.LifetimeUses >= ability.MaxLifetimeUses)
            {
                 hud = FindFirstObjectByType<PlayerHUDManager>(FindObjectsInactive.Include);
                if (hud && playerStats != null)
                    hud.RefreshSkillBar(playerStats);
            }
        }

        yield return null;
    }




}
