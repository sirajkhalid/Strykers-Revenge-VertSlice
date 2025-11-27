using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerInfoMenu : MonoBehaviour
{
    [Header("References")]
    public GameObject playerMenuPanel;
    public Image portraitImage;

    private PlayerPartyController party;
    private bool isMenuOpen = false;

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

    void Start()
    {
        party = FindFirstObjectByType<PlayerPartyController>();

        if (playerMenuPanel != null)
            playerMenuPanel.SetActive(false);
    }

    private CharacterStats GetActiveStats()
    {
        if (party == null)
            party = FindFirstObjectByType<PlayerPartyController>();

        if (party == null || party.activeMember == null)
            return null;

        return party.activeMember.GetComponent<CharacterStats>();
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

    void OpenMenu()
    {
        if (playerMenuPanel == null) return;

        // Allow menu to open even if stats are temporarily null
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
        CharacterStats stats = GetActiveStats();
        if (stats == null) return; // safely ignore instead of breaking the menu

        // Portrait
        if (portraitImage != null && stats.characterPortrait != null)
            portraitImage.sprite = stats.characterPortrait;

        // Left side
        nameText.text = $"NAME: {stats.characterName}";
        classText.text = $"CLASS: {stats.characterClass}";
        healthText.text = $"HP: {stats.currentHealth}/{stats.maxHealth}";
        initiativeText.text = $"INITIATIVE: {stats.initiative}";
        armorClassText.text = $"AC: {stats.armorClass}";

        // Basic info
        raceText.text = $"RACE: {stats.race}";
        backgroundText.text = $"BACKGROUND: {stats.background}";
        alignmentText.text = $"ALIGNMENT: {stats.alignment}";
        expText.text = $"EXP: {stats.currentEXP}/{stats.expToNextLevel}";

        // Attributes
        STRText.text = $"STRENGTH: {stats.strength}";
        DEXText.text = $"DEXTERITY: {stats.dexterity}";
        CONText.text = $"CONSTITUTION: {stats.constitution}";
        INTText.text = $"INTELLIGENCE: {stats.intelligence}";
        WISText.text = $"WISDOM: {stats.wisdom}";
        CHAText.text = $"CHARISMA: {stats.charisma}";

        // Skills
        athleticsText.text = $"ATHLETICS: {stats.athletics}";
        acrobaticsText.text = $"ACROBATICS: {stats.acrobatics}";
        soHText.text = $"SLEIGHT OF HAND: {stats.sleightOfHand}";
        stealthText.text = $"STEALTH: {stats.stealth}";
        arcanaText.text = $"ARCANA: {stats.arcana}";
        historyText.text = $"HISTORY: {stats.history}";
        investigationText.text = $"INVESTIGATION: {stats.investigation}";
        natureText.text = $"NATURE: {stats.nature}";
        religionText.text = $"RELIGION: {stats.religion}";
        animalHandlingText.text = $"ANIMAL HANDLING: {stats.animalHandling}";
        insightText.text = $"INSIGHT: {stats.insight}";
        medicineText.text = $"MEDICINE: {stats.medicine}";
        perceptionText.text = $"PERCEPTION: {stats.perception}";
        survivalText.text = $"SURVIVAL: {stats.survival}";
        deceptionText.text = $"DECEPTION: {stats.deception}";
        intimidationText.text = $"INTIMIDATION: {stats.intimidation}";
        performanceText.text = $"PERFORMANCE: {stats.performance}";
        persuasionText.text = $"PERSUASION: {stats.persuasion}";
    }
}
