using PixelCrushers.DialogueSystem;
using System.Buffers.Text;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.UIElements;

public class ChestUpgrade : MonoBehaviour
{

    public CharacterStats characterStats;
    public PlayerHUDManager playerHUDManager;
    


    public void Start()
    {
      
        
        characterStats.CalculateDerivedStats();


        playerHUDManager.UpdateHealthBar();

    }


    public void gainHP(double amount)
    {
        // Increase bonus HP:
        characterStats.bonusHP += (int)amount;

        // Recompute maxHealth using the increased bonus HP value:
        characterStats.CalculateDerivedStats();

        playerHUDManager.UpdateHealthBar();
    }
    public void gainMovement(double amount)
    {

    }
    public void gainDamage(double amount)
    {

    }
    public void gainArmor()
    {

    }

        void OnEnable()
        {
            // Make the functions available to Lua: (Replace these lines with your own.)

            Lua.RegisterFunction(nameof(gainHP), this, SymbolExtensions.GetMethodInfo(() => gainHP((double)0)));
        }

       
    

}