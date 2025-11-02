using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerInfoMenu : MonoBehaviour
{
    [Header("References")]
    public GameObject playerMenuPanel; // PlayerInfoPanel
    public CharacterStats playerStats; // Player GameObject
    public Image portraitImage;

    [Header("Left Side")]
    public TMP_Text nameText;
    public TMP_Text classText;
    public TMP_Text healthText;
    public TMP_Text initiativeText;
    public TMP_Text armorClassText;

    [Header("Right Side - Basic Info")]
    public TMP_Text raceText;
    public TMP_Text backgroundText;
    public TMP_Text alignmentText;
    public TMP_Text expText;

    [Header("Right Side - Attributes")]
    public TMP_Text STRText;
    public TMP_Text DEXText;
    public TMP_Text CONText;
    public TMP_Text INTText;
    public TMP_Text WISText;
    public TMP_Text CHAText;

    [Header("Right Side - Skills")]
    public TMP_Text athleticsText;
    public TMP_Text acrobaticsText;
    public TMP_Text soHText;
    public TMP_Text stealthText;
    public TMP_Text arcanaText;
    public TMP_Text historyText;
    public TMP_Text investigationText;
    public TMP_Text natureText;
    public TMP_Text religionText;
    public TMP_Text animalHandlingText;
    public TMP_Text insightText;
    public TMP_Text medicineText;
    public TMP_Text perceptionText;
    public TMP_Text survivalText;
    public TMP_Text deceptionText;
    public TMP_Text intimidationText;
    public TMP_Text performanceText;
    public TMP_Text persuasionText;

    private bool isMenuOpen = false;

    void Start()
    {
        if (playerMenuPanel != null)
            playerMenuPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (isMenuOpen)
                CloseMenu();
            else
                OpenMenu();
        }
    }
    void Awake()
    {
        if (!Application.isPlaying)
        {
            if (playerMenuPanel != null)
                playerMenuPanel.SetActive(false);
        }
        else
        {
            if (playerMenuPanel != null)
                playerMenuPanel.SetActive(true);
        }
    }

    void OpenMenu()
    {
        if (playerMenuPanel == null || playerStats == null) return;

        playerMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        isMenuOpen = true;

        UpdatePlayerInfo();
    }

    void CloseMenu()
    {
        if (playerMenuPanel == null) return;

        playerMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        isMenuOpen = false;
    }

    void UpdatePlayerInfo()
    {
        if (portraitImage != null && playerStats != null && playerStats.characterPortrait != null)
            portraitImage.sprite = playerStats.characterPortrait;

        // Left side
        nameText.text = $"NAME: {playerStats.characterName}";
        classText.text = $"CLASS: {playerStats.characterClass}";
        healthText.text = $"HP: {playerStats.currentHealth}/{playerStats.maxHealth}";
        initiativeText.text = $"INITIATIVE: {playerStats.initiative}";
        armorClassText.text = $"AC: {playerStats.armorClass}";

        // Right side - Basic info
        raceText.text = $"RACE: {playerStats.race}";
        backgroundText.text = $"BACKGROUND: {playerStats.background}";
        alignmentText.text = $"ALIGNMENT: {playerStats.alignment}";
        expText.text = $"EXP: {playerStats.currentEXP}/{playerStats.expToNextLevel}";

        // Right side - Attributes
        STRText.text = $"STRENGTH: {playerStats.strength}";
        DEXText.text = $"DEXTERITY: {playerStats.dexterity}";
        CONText.text = $"CONSTITUTION: {playerStats.constitution}";
        INTText.text = $"INTELLIGENCE: {playerStats.intelligence}";
        WISText.text = $"WISDOM: {playerStats.wisdom}";
        CHAText.text = $"CHARISMA: {playerStats.charisma}";

        // Right side - Skills
        athleticsText.text = $"ATHLETICS: {playerStats.athletics}";
        acrobaticsText.text = $"ACROBATICS: {playerStats.acrobatics}";
        soHText.text = $"SLEIGHT OF HAND: {playerStats.sleightOfHand}";
        stealthText.text = $"STEALTH: {playerStats.stealth}";
        arcanaText.text = $"ARCANA: {playerStats.arcana}";
        historyText.text = $"HISTORY: {playerStats.history}";
        investigationText.text = $"INVESTIGATION: {playerStats.investigation}";
        natureText.text = $"NATURE: {playerStats.nature}";
        religionText.text = $"RELIGION: {playerStats.religion}";
        animalHandlingText.text = $"ANIMAL HANDLING: {playerStats.animalHandling}";
        insightText.text = $"INSIGHT: {playerStats.insight}";
        medicineText.text = $"MEDICINE: {playerStats.medicine}";
        perceptionText.text = $"PERCEPTION: {playerStats.perception}";
        survivalText.text = $"SURVIVAL: {playerStats.survival}";
        deceptionText.text = $"DECEPTION: {playerStats.deception}";
        intimidationText.text = $"INTIMIDATION: {playerStats.intimidation}";
        performanceText.text = $"PERFORMANCE: {playerStats.performance}";
        persuasionText.text = $"PERSUASION: {playerStats.persuasion}";
    }
}
