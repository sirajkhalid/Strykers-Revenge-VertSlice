using UnityEngine;
using System.Collections;

public class AbilityExecutor : MonoBehaviour
{
    public CharacterStats playerStats;
    public Animator playerAnimator;
    public Camera mainCamera;

    private TurnManager turnManager;
    private PlayerHUDManager hud;

    void Start()
    {
        turnManager = FindFirstObjectByType<TurnManager>();
        hud = FindFirstObjectByType<PlayerHUDManager>(FindObjectsInactive.Include);
        if (playerStats == null) playerStats = FindFirstObjectByType<CharacterStats>();
    }

    public void ExecuteAbility(Ability ability, Transform target = null)
    {
        if (ability == null) return;
        if (turnManager != null && !turnManager.isPlayerTurn) return;
        if (!playerStats.CanUseActionType(ability.actionType)) return;

        int slotCost = ability.usesSpellSlot ? Mathf.Max(ability.slotCost, 0) : 0;
        int level = ability.spellLevel; // 0 = cantrip
        if (!playerStats.HasSpellSlots(level, slotCost)) return;

        playerStats.ConsumeActionType(ability.actionType);
        playerStats.SpendSpellSlots(level, slotCost);
        if (hud != null)
        {
            hud.UpdateActionUI();
            hud.UpdateSpellSlotUI();
        }

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
                    default:
                        StartCoroutine(FireTowardEnemyOnly(ability));
                        break;
                }
                break;
            case Ability.TargetType.Ally:
                if (ability.deliveryType == Ability.DeliveryType.Area)
                    StartCoroutine(ExecuteAreaAbility(ability));
                else
                {
                    var allies = FindObjectsByType<CharacterStats>(FindObjectsSortMode.None);
                    foreach (var ally in allies)
                    {
                        if (Vector3.Distance(playerStats.transform.position, ally.transform.position) <= ability.range)
                        {
                            var effect = ally.GetComponent<StatusEffectManager>();
                            if (effect != null && ability.statusEffectName == "Blessed")
                                effect.ApplyBless(ability.statusDuration);
                        }
                    }
                }
                break;
            case Ability.TargetType.Area:
                StartCoroutine(ExecuteAreaAbility(ability));
                break;
        }
    }

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
        playerStats.currentHealth = Mathf.Min(playerStats.currentHealth + healAmount, playerStats.maxHealth);

        if (playerStats.floatingDamagePrefab != null)
        {
            Color color = isCrit ? Color.yellow : Color.green;
            string text = isFail ? "Missed Heal" : $"+{healAmount}";
            playerStats.ShowFloatingText(text, color);
        }
    }

    IEnumerator FireTowardEnemyOnly(Ability ability)
    {
        if (ability.visualEffectPrefab == null || mainCamera == null) yield break;

        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;

        RaycastHit2D hit = Physics2D.Raycast(mouseWorld, Vector2.zero);
        if (hit.collider == null) yield break;

        EnemyStats enemyStats = hit.collider.GetComponent<EnemyStats>();
        if (enemyStats == null) yield break;

        Transform enemy = hit.collider.transform;
        Vector3 spawnPos = playerStats.transform.position;
        Vector3 targetPos = enemy.position;

        GameObject projectile = Instantiate(ability.visualEffectPrefab, spawnPos, Quaternion.identity);
        projectile.transform.localScale = Vector3.one * 3f;

        SpriteRenderer sr = projectile.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = "VFX";
            sr.sortingOrder = 5;
        }

        float speed = 10f;
        Vector3 dir = (targetPos - spawnPos).normalized;

        while (projectile && Vector3.Distance(projectile.transform.position, targetPos) > 0.1f)
        {
            projectile.transform.position += dir * speed * Time.deltaTime;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            projectile.transform.rotation = Quaternion.Euler(0, 0, angle);
            yield return null;
        }

        if (enemyStats != null)
        {
            int damage = Mathf.RoundToInt(ability.baseDamage + playerStats.intelligence * ability.damageScaling);
            enemyStats.TakeDamage(damage);
        }

        if (projectile != null) Destroy(projectile);
    }

    IEnumerator ExecuteMeleeAbility(Ability ability)
    {
        if (ability == null || playerStats == null) yield break;

        if (playerAnimator && ability.abilityAnimation)
            playerAnimator.Play(ability.abilityAnimation.name);

        yield return new WaitForSeconds(0.25f);

        Collider2D[] hits = Physics2D.OverlapCircleAll(playerStats.transform.position, ability.range);
        EnemyStats closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            EnemyStats enemy = hit.GetComponent<EnemyStats>();
            if (enemy != null)
            {
                float dist = Vector2.Distance(playerStats.transform.position, enemy.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestEnemy = enemy;
                }
            }
        }

        if (closestEnemy == null)
        {
            if (playerStats.outOfRangePrefab != null)
            {
                GameObject msg = Instantiate(playerStats.outOfRangePrefab, playerStats.transform.position + Vector3.up * 2f, Quaternion.identity);
                Destroy(msg, 1f);
            }
            yield break;
        }

        if (ability.visualEffectPrefab)
        {
            GameObject vfx = Instantiate(ability.visualEffectPrefab, closestEnemy.transform.position, Quaternion.identity);
            Destroy(vfx, 1.0f);
        }

        int d20Roll = Random.Range(1, 21);
        bool isCrit = d20Roll == 20;
        bool isMiss = d20Roll == 1;

        int damage = Mathf.RoundToInt(ability.baseDamage + playerStats.strength * ability.damageScaling);
        if (isCrit) damage = Mathf.RoundToInt(damage * 1.5f);
        if (isMiss) damage = 0;

        closestEnemy.TakeDamage(damage);

        // Handle knockback via special effect
        if (ability.specialEffect == Ability.SpecialEffect.Knockback && closestEnemy != null)
        {
            Vector3 pushDir = (closestEnemy.transform.position - playerStats.transform.position).normalized;
            float pushDistance = 3f;
            Vector3 targetPos = closestEnemy.transform.position + pushDir * pushDistance;
            StartCoroutine(SmoothKnockback(closestEnemy.transform, targetPos, 0.25f));
        }

        if (playerStats.floatingDamagePrefab != null)
        {
            Color color = isCrit ? Color.yellow : Color.red;
            string text = isMiss ? "MISS" : $"-{damage}";
            playerStats.ShowFloatingText(text, color);
        }
    }

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

    private IEnumerator SmoothKnockback(Transform target, Vector3 destination, float duration)
    {
        float elapsed = 0f;
        Vector3 start = target.position;

        while (elapsed < duration)
        {
            if (target == null) yield break;
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            target.position = Vector3.Lerp(start, destination, t);
            yield return null;
        }

        if (target != null)
            target.position = destination;
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
