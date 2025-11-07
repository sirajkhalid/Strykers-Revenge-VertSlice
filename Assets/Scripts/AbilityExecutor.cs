using UnityEngine;
using System.Collections;

public class AbilityExecutor : MonoBehaviour
{
    public CharacterStats playerStats;
    public Animator playerAnimator;
    public Camera mainCamera;

    public void ExecuteAbility(Ability ability, Transform target = null)
    {
        if (ability == null) return;

        switch (ability.targetType)
        {
            case Ability.TargetType.Self:
                StartCoroutine(ExecuteSelfAbility(ability));
                break;

            case Ability.TargetType.Enemy:
                // For now, fire toward mouse position instead of enemy
                StartCoroutine(FireTowardEnemyOnly(ability));
                break;
        }
    }

    IEnumerator ExecuteSelfAbility(Ability ability)
    {
        if (ability == null || playerStats == null)
            yield break;

        // spawn vfx
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

        // D20 roll
        int d20Roll = Random.Range(1, 21); // 1–20 inclusive
        bool isCrit = d20Roll == 20;
        bool isFail = d20Roll == 1;

        // Determine which stat to use for scaling
        float scalingValue = 0f;
        string attr = ability.scalingAttribute.ToLower();
        if (attr == "strength") scalingValue = playerStats.strength;
        else if (attr == "dexterity") scalingValue = playerStats.dexterity;
        else if (attr == "constitution") scalingValue = playerStats.constitution;
        else if (attr == "intelligence") scalingValue = playerStats.intelligence;
        else if (attr == "wisdom") scalingValue = playerStats.wisdom;
        else if (attr == "charisma") scalingValue = playerStats.charisma;

        // Calculate heal amount
        float healRaw = Mathf.Abs(ability.baseDamage) + (d20Roll + scalingValue) * ability.damageScaling;
        if (isCrit) healRaw *= 2f; // double on crit
        if (isFail) healRaw *= 0.5f; // halve on natural 1

        int healAmount = Mathf.RoundToInt(healRaw);
        playerStats.currentHealth = Mathf.Min(playerStats.currentHealth + healAmount, playerStats.maxHealth);

        
        if (playerStats.floatingDamagePrefab != null)
        {
            Color color = isCrit ? Color.yellow : Color.green;
            string text = isFail ? $"Missed Heal" : $"+{healAmount}";
            playerStats.ShowFloatingText(text, color);
        }

        yield return null;
    }

    IEnumerator FireTowardEnemyOnly(Ability ability)
    {
        if (ability.visualEffectPrefab == null || mainCamera == null)
            yield break;
        
        // Check if the player is hovering over an enemy
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;

        RaycastHit2D hit = Physics2D.Raycast(mouseWorld, Vector2.zero);
        if (hit.collider == null) yield break;

        EnemyStats enemyStats = hit.collider.GetComponent<EnemyStats>();
        if (enemyStats == null) yield break;

        Transform enemy = hit.collider.transform;

        // Spawn projectile at player position
        Vector3 spawnPos = playerStats.transform.position;
        Vector3 targetPos = enemy.position;
        GameObject projectile = Instantiate(ability.visualEffectPrefab, spawnPos, Quaternion.identity);

        projectile.transform.localScale = Vector3.one * 3f;

        // Set proper render layer
        SpriteRenderer sr = projectile.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = "VFX";
            sr.sortingOrder = 5;
        }

        // Move projectile until it hits enemy
        float speed = 10f;
        Vector3 dir = (targetPos - spawnPos).normalized;

        while (projectile && Vector3.Distance(projectile.transform.position, targetPos) > 0.1f)
        {
            projectile.transform.position += dir * speed * Time.deltaTime;

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            projectile.transform.rotation = Quaternion.Euler(0, 0, angle);

            yield return null;
        }

        // When it reaches the enemy, apply damage & destroy projectile
        if (enemyStats != null)
        {
            int damage = Mathf.RoundToInt(ability.baseDamage + playerStats.intelligence * ability.damageScaling);
            enemyStats.TakeDamage(damage);
        }

        if (projectile != null)
            Destroy(projectile);
    }

    private int GetModifierForScaling(CharacterStats stats, string scaling)
    {
        switch (scaling.ToLower())
        {
            case "strength": return stats.strength;
            case "dexterity": return stats.dexterity;
            case "intelligence": return stats.intelligence;
            case "wisdom": return stats.wisdom;
            case "charisma": return stats.charisma;
            case "constitution": return stats.constitution;
            default: return 0;
        }
    }
}
