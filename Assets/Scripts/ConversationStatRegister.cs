using JetBrains.Annotations;
using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.UnityGUI;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class ConversationStatRegister : MonoBehaviour
{
    //Referencing GameObjects
    private CharacterStats characterStats;

    GameObject parentTrigger;
    GameObject athleticsTrigger;
    GameObject sleightOfHandTrigger;
    GameObject arcanaTrigger;
    GameObject historyTrigger;
    GameObject investigationTrigger;
    GameObject religionTrigger;
    GameObject insightTrigger;
    GameObject deceptionTrigger;
    GameObject persuasionTrigger;
    GameObject performanceTrigger;
    GameObject intimidationTrigger;

    //int used for the dice roll
    int diceRollNumber;

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
        //Grabbing the Character Stats script in order to reference it
        characterStats = Object.FindFirstObjectByType<CharacterStats>();
    }

    
    void Update()
    {
        //Updates the players stats within this script ever 10 second via coroutine
        StartCoroutine(UpdatePlayerStats());
        StartCoroutine(UpdateSceneTriggers());
    }

    IEnumerator UpdateSceneTriggers()
    {
        yield return new WaitForSeconds(1);

        parentTrigger = GameObject.Find("Dice Roll Triggers");

        if (parentTrigger != null )
        {
            athleticsTrigger = parentTrigger.transform.GetChild(0).gameObject;
            sleightOfHandTrigger = parentTrigger.transform.GetChild(1).gameObject;
            arcanaTrigger = parentTrigger.transform.GetChild(2).gameObject;
            historyTrigger = parentTrigger.transform.GetChild(3).gameObject;
            investigationTrigger = parentTrigger.transform.GetChild(4).gameObject;
            religionTrigger = parentTrigger.transform.GetChild(5).gameObject;
            insightTrigger = parentTrigger.transform.GetChild(6).gameObject;
            deceptionTrigger = parentTrigger.transform.GetChild(7).gameObject;
            persuasionTrigger = parentTrigger.transform.GetChild(8).gameObject;
            performanceTrigger = parentTrigger.transform.GetChild(9).gameObject;
            intimidationTrigger = parentTrigger.transform.GetChild(10).gameObject;
        }
    }

    IEnumerator UpdatePlayerStats()
    {
        yield return new WaitForSeconds(10);

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

    IEnumerator DisableTriggers()
    {
        yield return new WaitForSeconds(5);

        athleticsTrigger.SetActive(false);
        sleightOfHandTrigger.SetActive(false);
        arcanaTrigger.SetActive(false);
        historyTrigger.SetActive(false);
        investigationTrigger.SetActive(false);
        religionTrigger.SetActive(false);
        insightTrigger.SetActive(false);
        deceptionTrigger.SetActive(false);
        persuasionTrigger.SetActive(false);
        performanceTrigger.SetActive(false);
        intimidationTrigger.SetActive(false);
    }

    //Function for generating dice roll
    private void GenerateDiceRoll()
    {
        diceRollNumber = Random.Range(1, 20);
    }

    bool HasAthletics(string requiredAthletics)
    {
        if (athleticsTrigger.activeSelf)
        {
            GenerateDiceRoll();
            if (diceRollNumber <= characterStats.athletics)
            {
                requiredAthletics = "1";
                StartCoroutine(DisableTriggers());
                return true;
            }
            else
            {
                requiredAthletics = "0";
                StartCoroutine(DisableTriggers());
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    bool HasSOH(string requiredSOH)
    {
        if (sleightOfHandTrigger.activeSelf)
        {
            GenerateDiceRoll();
            if (diceRollNumber <= characterStats.sleightOfHand)
            {
                requiredSOH = "1";
                StartCoroutine(DisableTriggers());
                return true;
            }
            else
            {
                requiredSOH = "0";
                StartCoroutine(DisableTriggers());
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    bool HasArcana(string requiredArcana)
    {
        if (arcanaTrigger.activeSelf)
        {
            GenerateDiceRoll();
            if (diceRollNumber <= characterStats.arcana)
            {
                requiredArcana = "1";
                StartCoroutine(DisableTriggers());
                return true;
            }
            else
            {
                requiredArcana = "0";
                StartCoroutine(DisableTriggers());
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    bool HasHistory(string requiredHistory)
    {
        if (historyTrigger.activeSelf)
        {
            GenerateDiceRoll();
            if (diceRollNumber <= characterStats.history)
            {
                requiredHistory = "1";
                StartCoroutine(DisableTriggers());
                return true;
            }
            else
            {
                requiredHistory = "0";
                StartCoroutine(DisableTriggers());
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    bool HasInvestigation(string requiredInvestigation)
    {
        if (investigationTrigger.activeSelf)
        {
            GenerateDiceRoll();
            if (diceRollNumber <= characterStats.investigation)
            {
                requiredInvestigation = "1";
                StartCoroutine(DisableTriggers());
                return true;
            }
            else
            {
                requiredInvestigation = "0";
                StartCoroutine(DisableTriggers());
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    bool HasReligion(string requiredReligion)
    {
        if (religionTrigger.activeSelf)
        {
            GenerateDiceRoll();
            if (diceRollNumber <= characterStats.religion)
            {
                requiredReligion = "1";
                StartCoroutine(DisableTriggers());
                return true;
            }
            else
            {
                requiredReligion = "0";
                StartCoroutine(DisableTriggers());
                return false;
            }
        }
        else
        {
            return false;
        }
    }
    
    bool HasInsight(string requiredInsight)
    {
        if (insightTrigger.activeSelf)
        {
            GenerateDiceRoll();
            if (diceRollNumber <= characterStats.insight)
            {
                requiredInsight = "1";
                StartCoroutine(DisableTriggers());
                return true;
            }
            else
            {
                requiredInsight = "0";
                StartCoroutine(DisableTriggers());
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    bool HasDeception(string requiredDeception)
    {
        if (deceptionTrigger.activeSelf)
        {
            GenerateDiceRoll();
            if (diceRollNumber <= characterStats.deception)
            {
                requiredDeception = "1";
                StartCoroutine(DisableTriggers());
                return true;
            }
            else
            {
                requiredDeception = "0";
                StartCoroutine(DisableTriggers());
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    bool HasPersuasion(string requiredPersuasion)
    {
        if (persuasionTrigger.activeSelf)
        {
            GenerateDiceRoll();
            if (diceRollNumber <= characterStats.persuasion)
            {
                requiredPersuasion = "1";
                StartCoroutine(DisableTriggers());
                return true;
            }
            else
            {
                requiredPersuasion = "0";
                StartCoroutine(DisableTriggers());
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    bool HasPerformance(string requiredPerformance)
    {
        if (performanceTrigger.activeSelf)
        {
            GenerateDiceRoll();
            if (diceRollNumber <= characterStats.performance)
            {
                requiredPerformance = "1";
                StartCoroutine(DisableTriggers());
                return true;
            }
            else
            {
                requiredPerformance = "0";
                StartCoroutine(DisableTriggers());
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    bool HasIntimidation(string requiredIntimidation)
    {
        if (intimidationTrigger.activeSelf)
        {
            GenerateDiceRoll();
            if (diceRollNumber <= characterStats.intimidation)
            {
                requiredIntimidation = "1";
                StartCoroutine(DisableTriggers());
                return true;
            }
            else
            {
                requiredIntimidation = "0";
                StartCoroutine(DisableTriggers());
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    #region Register with Lua
    void OnEnable()
    {
        //Registering the bool functions with the Lua code so they can be used by the dialogue system
        //New entries can use the following template below

        //Lua.RegisterFunction("EnterFunctionNameHere", this, SymbolExtensions.GetMethodInfo(() => EnterFunctionNameHere(string.Empty)));

        Lua.RegisterFunction("HasAthletics", this, SymbolExtensions.GetMethodInfo(() => HasAthletics(string.Empty)));
        Lua.RegisterFunction("HasSOH", this, SymbolExtensions.GetMethodInfo(() => HasSOH(string.Empty)));
        Lua.RegisterFunction("HasArcana", this, SymbolExtensions.GetMethodInfo(() => HasArcana(string.Empty)));
        Lua.RegisterFunction("HasHistory", this, SymbolExtensions.GetMethodInfo(() => HasHistory(string.Empty)));
        Lua.RegisterFunction("HasInvestigation", this, SymbolExtensions.GetMethodInfo(() => HasInvestigation(string.Empty)));
        Lua.RegisterFunction("HasReligion", this, SymbolExtensions.GetMethodInfo(() => HasReligion(string.Empty)));
        Lua.RegisterFunction("HasInsight", this, SymbolExtensions.GetMethodInfo(() => HasInsight(string.Empty)));
        Lua.RegisterFunction("HasDeception", this, SymbolExtensions.GetMethodInfo(() => HasDeception(string.Empty)));
        Lua.RegisterFunction("HasPersuasion", this, SymbolExtensions.GetMethodInfo(() => HasPersuasion(string.Empty)));
        Lua.RegisterFunction("HasPerformance", this, SymbolExtensions.GetMethodInfo(() => HasPerformance(string.Empty)));
        Lua.RegisterFunction("HasIntimidation", this, SymbolExtensions.GetMethodInfo(() => HasIntimidation(string.Empty)));
    }

    void OnDisable()
    {
        //Disables the bool functions in Lua since they are not on a persistent object
        //New entries can use the following template below

        //Lua.UnregisterFunction("EnterFunctionNameHere");

        Lua.UnregisterFunction("HasAthletics");
        Lua.UnregisterFunction("HasSOH");
        Lua.UnregisterFunction("HasArcana");
        Lua.UnregisterFunction("HasHistory");
        Lua.UnregisterFunction("HasInvestigation");
        Lua.UnregisterFunction("HasReligion");
        Lua.UnregisterFunction("HasInsight");
        Lua.UnregisterFunction("HasDeception");
        Lua.UnregisterFunction("HasPersuasion");
        Lua.UnregisterFunction("HasPerformance");
        Lua.UnregisterFunction("HasIntimidation");
    }
    #endregion
}
