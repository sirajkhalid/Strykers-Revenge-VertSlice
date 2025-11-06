using UnityEngine;
using System.Collections;

public class AbilityExecutor : MonoBehaviour
{
    public CharacterStats playerStats;
    public Animator playerAnimator;

    public void ExecuteAbility(Ability ability, Transform target = null)
    {
        if (ability == null)
        {
            Debug.LogWarning("Tried to execute a null ability!");
            return;
        }

        Debug.Log($"Executing {ability.abilityName}");

        // Spawn the ability visual prefab if it exists
        if (ability.visualEffectPrefab != null)
            StartCoroutine(PlayAbilityVFX(ability, target));

        // Handle by type
        switch (ability.targetType)
        {
            case Ability.TargetType.Self:
                ApplySelfAbility(ability);
                break;
            case Ability.TargetType.Enemy:
                Debug.Log("TODO: Implement targeting enemy later");
                break;
            default:
                Debug.Log($"Unhandled ability target type: {ability.targetType}");
                break;
        }
    }

    private IEnumerator PlayAbilityVFX(Ability ability, Transform target)
    {
        // Use the player's world position instead of this object's
        Transform playerTransform = playerStats?.transform ?? transform;
        Vector3 spawnPos = playerTransform.position;
        spawnPos.z = -0.5f; // keep it visible in front of the player

        GameObject vfx = Instantiate(ability.visualEffectPrefab, spawnPos, Quaternion.identity);
        Debug.Log($"[VFX] Spawned {vfx.name} at {spawnPos}");

        // (same renderer and animator setup as before)
        SpriteRenderer sr = vfx.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = "VFX";
            sr.sortingOrder = 10;
            sr.enabled = true;
        }

        vfx.transform.localScale = Vector3.one;
        vfx.SetActive(true);

        Animator fxAnimator = vfx.GetComponent<Animator>();
        if (fxAnimator && fxAnimator.runtimeAnimatorController)
        {
            fxAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            AnimationClip[] clips = fxAnimator.runtimeAnimatorController.animationClips;
            float clipLength = clips.Length > 0 ? clips[0].length : 1f;
            yield return new WaitForSeconds(clipLength);
        }

        Destroy(vfx);
    }


    private void ApplySelfAbility(Ability ability)
    {
        float amount = 0f;

        if (ability.damageType == Ability.DamageType.Holy)
        {
            amount = Mathf.Abs(ability.baseDamage) + playerStats.intelligence * ability.damageScaling;
            int healAmount = Mathf.RoundToInt(amount);
            playerStats.currentHealth = Mathf.Min(playerStats.currentHealth + healAmount, playerStats.maxHealth);
            Debug.Log($"Healed self for {healAmount} HP!");
        }
        else
        {
            Debug.Log($"{ability.abilityName} used on self (non-heal).");
        }
    }
}
