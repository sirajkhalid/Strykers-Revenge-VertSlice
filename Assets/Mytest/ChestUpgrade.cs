using PixelCrushers.DialogueSystem;
using System.Buffers.Text;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.UIElements;

public class ChestUpgrade : MonoBehaviour
{

    public CharacterStats characterStats;

    
    


    public void Start()
    {
      
        
        characterStats.CalculateDerivedStats();


       

    }


    public void gainHP(double amount)
    {
        // Increase bonus HP:
        characterStats.bonusHP += (int)amount;

        // Recompute maxHealth using the increased bonus HP value:
        characterStats.CalculateDerivedStats();


        FindFirstObjectByType<PlayerHUDManager>().UpdateHealthBar();

        GameObject Chest = GameObject.Find("Chest");
        Chest.gameObject.SetActive(false);

    }
    public void gainMovement(double amount)
    {
        characterStats.bonusHP += (int)amount;

        // Recompute maxHealth using the increased bonus HP value:
        characterStats.CalculateDerivedStats();

       
    }
    public void gainDamage(double amount)
    {
        characterStats.bonusHP += (int)amount;

        // Recompute maxHealth using the increased bonus HP value:
        characterStats.CalculateDerivedStats();
    }
    public void gainArmor(double amount)
    {
        characterStats.bonusArmor += (int)amount;

        // Recompute maxHealth using the increased bonus HP value:
        characterStats.CalculateDerivedStats();


    }

        void OnEnable()
        {
            // Make the functions available to Lua: (Replace these lines with your own.)

            Lua.RegisterFunction(nameof(gainHP), this, SymbolExtensions.GetMethodInfo(() => gainHP((double)0)));

            Lua.RegisterFunction(nameof(gainArmor), this, SymbolExtensions.GetMethodInfo(() => gainArmor((double)0)));
        }

       
    

}