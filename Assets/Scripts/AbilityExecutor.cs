using UnityEngine;
using System.Collections;

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

    // CHANGE: wrapper to allow cast animation delay
    public void ExecuteAbility(Ability ability, Transform target = null)
    {
        StartCoroutine(ExecuteAbilityRoutine(ability, target));
    }

    private IEnumerator ExecuteAbilityRoutine(Ability ability, Transform target)
    {
        if (ability == null) yield break;

        RefreshActiveCharacter();
        if (playerStats == null) yield break;

        if (turnManager != null && !turnManager.isPlayerTurn) yield break;
        if (!playerStats.CanUseActionType(ability.actionType)) yield break;

        int slotCost = ability.usesSpellSlot ? Mathf.Max(ability.slotCost, 0) : 0;
        int level = ability.spellLevel;
        if (!playerStats.HasSpellSlots(level, slotCost)) yield break;

        // Consume resources
        playerStats.ConsumeActionType(ability.actionType);
        playerStats.SpendSpellSlots(level, slotCost);

        hud?.UpdateActionUI();
        hud?.UpdateSpellSlotUI();

        // UNIVERSAL CAST ANIMATION ⭐⭐
        if (playerAnimator && playerStats.castAnimation != null)
        {
            playerAnimator.Play(playerStats.castAnimation.name);

            float animDelay = Mathf.Min(0.25f, playerStats.castAnimation.length * 0.3f);
            yield return new WaitForSeconds(animDelay);
        }

        // Execute actual ability
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
                if (ability.deliveryType == Ability.DeliveryType.Area)
                    StartCoroutine(ExecuteAreaAbility(ability));
                else
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

        Vector3 spawnPos = playerStats.transform.position;
        Vector3 targetPos = lockedTarget.position;

        GameObject projectile = Instantiate(ability.visualEffectPrefab, spawnPos, Quaternion.identity);
        projectile.transform.localScale = Vector3.one * 3f;

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
            projectile.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
            yield return null;
        }

        int damage = Mathf.RoundToInt(ability.baseDamage + playerStats.intelligence * ability.damageScaling);
        enemyStats.TakeDamage(damage);

        if (projectile != null) Destroy(projectile);
    }

    // MELEE ABILITY
    IEnumerator ExecuteMeleeAbility(Ability ability)
    {
        if (ability == null || playerStats == null) yield break;

        if (playerAnimator && ability.abilityAnimation)
            playerAnimator.Play(ability.abilityAnimation.name);

        yield return new WaitForSeconds(0.25f);

        TargetSelector selector = FindFirstObjectByType<TargetSelector>();
        Transform lockedTarget = selector?.GetCurrentTarget();

        EnemyStats closestEnemy = null;

        // priority: locked target
        if (lockedTarget)
            closestEnemy = lockedTarget.GetComponent<EnemyStats>();
        else
        {
            // fallback: find closest in range
            Collider2D[] hits = Physics2D.OverlapCircleAll(playerStats.transform.position, ability.range);
            float closest = Mathf.Infinity;

            foreach (var hit in hits)
            {
                EnemyStats e = hit.GetComponent<EnemyStats>();
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

        if (!closestEnemy)
        {
            if (playerStats.outOfRangePrefab)
            {
                GameObject msg = Instantiate(playerStats.outOfRangePrefab, playerStats.transform.position + Vector3.up * 2f, Quaternion.identity);
                Destroy(msg, 1f);
            }
            yield break;
        }

        // VFX on enemy
        if (ability.visualEffectPrefab)
        {
            GameObject vfx = Instantiate(ability.visualEffectPrefab, closestEnemy.transform.position, Quaternion.identity);
            Destroy(vfx, 1f);
        }

        int d20Roll = Random.Range(1, 21);
        bool isCrit = d20Roll == 20;
        bool isMiss = d20Roll == 1;

        int damage = Mathf.RoundToInt(ability.baseDamage + playerStats.strength * ability.damageScaling);
        if (isCrit) damage = Mathf.RoundToInt(damage * 1.5f);
        if (isMiss) damage = 0;

        closestEnemy.TakeDamage(damage);

        // Correct floating text placement (ENEMY only)
        if (closestEnemy.floatingDamagePrefab != null)
        {
            string text = isMiss ? "MISS" : $"-{damage}";
            Color color = isCrit ? Color.yellow : Color.red;
            closestEnemy.ShowFloatingText(text, color);
        }
    }

    // INSTANT MAGIC (Spawn on enemy)
    IEnumerator ExecuteInstantMagic(Ability ability)
    {
        if (ability == null || playerStats == null) yield break;

        TargetSelector selector = FindFirstObjectByType<TargetSelector>();
        Transform lockedTarget = selector?.GetCurrentTarget();
        if (lockedTarget == null) yield break;

        EnemyStats enemyStats = lockedTarget.GetComponent<EnemyStats>();
        if (enemyStats == null) yield break;

        if (ability.visualEffectPrefab)
        {
            GameObject vfx = Instantiate(ability.visualEffectPrefab, lockedTarget.position, Quaternion.identity);
            vfx.transform.localScale = Vector3.one * 3f;
            Destroy(vfx, 1.5f);
        }

        int dmg = Mathf.RoundToInt(ability.baseDamage + playerStats.intelligence * ability.damageScaling);
        enemyStats.TakeDamage(dmg);

        if (enemyStats.floatingDamagePrefab != null)
            enemyStats.ShowFloatingText($"-{dmg}", Color.red);
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

        var allies = FindObjectsByType<CharacterStats>(FindObjectsSortMode.None);
        foreach (var ally in allies)
        {
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
}
