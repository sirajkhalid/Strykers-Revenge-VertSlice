using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class BaseEnemyAbilities
{
    [System.Serializable]
    public class EnemyAbility
    {
        public string abilityName;
        public string description;
        public int damage;
        public Action<GameObject> onUse; // what happens when used
    }

    public List<EnemyAbility> abilities = new List<EnemyAbility>();

    public abstract void InitializeAbilities(GameObject user);

    public void UseAbility(int index, GameObject user)
    {
        if (index < 0 || index >= abilities.Count)
        {
            Debug.LogWarning($"{user.name} tried to use invalid ability index {index}");
            return;
        }
        
        Debug.Log($"{user.name} uses {abilities[index].abilityName}!");
        abilities[index].onUse?.Invoke(user);
    }
}
