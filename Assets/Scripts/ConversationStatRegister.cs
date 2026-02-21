using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.UnityGUI;
using UnityEngine;

public class ConversationStatRegister : MonoBehaviour
{
    public CharacterStats characterStats;

    string currentAthletics = "0";
    string currentSOH = "0";
    string currentArcana = "0";
    string currentHistory = "0";
    string currentInvestigation = "0";
    string currentReligion = "0";
    string currentInsight = "0";
    string currentDeception = "0";
    string currentPersuasion = "0";
    string currentPerformance = "0";
    string currentIntimidation = "0";

    void Start()
    {
        characterStats = Object.FindFirstObjectByType<CharacterStats>();
        /*
        Debug.Log(characterStats.athletics);
        Debug.Log(characterStats.sleightOfHand);
        Debug.Log(characterStats.arcana);
        Debug.Log(characterStats.history);
        Debug.Log(characterStats.investigation);
        Debug.Log(characterStats.religion);
        Debug.Log(characterStats.insight);
        Debug.Log(characterStats.deception);
        Debug.Log(characterStats.persuasion);
        Debug.Log(characterStats.performance);
        Debug.Log(characterStats.intimidation);
        */
    }

    
    void Update()
    {
        currentAthletics = characterStats.athletics.ToString();
        currentSOH = characterStats.sleightOfHand.ToString();
        currentArcana = characterStats.arcana.ToString();
        currentHistory = characterStats.history.ToString();
        currentInvestigation = characterStats.investigation.ToString();
        currentReligion = characterStats.religion.ToString();
        currentInsight = characterStats.insight.ToString();
        currentDeception = characterStats.deception.ToString();
        currentPersuasion = characterStats.persuasion.ToString();
        currentPerformance = characterStats.performance.ToString();
        currentIntimidation = characterStats.intimidation.ToString();
    }

    public bool HasAthletics(string requiredAthletics)
    {
        return currentAthletics == requiredAthletics;
    }

    #region Register with Lua
    void OnEnable()
    {
        Lua.RegisterFunction("HasAthletics", this, SymbolExtensions.GetMethodInfo(() => HasAthletics(string.Empty)));
    }

    void OnDisable()
    {
        Lua.UnregisterFunction(nameof(HasAthletics));
    }
    #endregion
}
