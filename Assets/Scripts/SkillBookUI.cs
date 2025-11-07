using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;


public class SkillBookUI : MonoBehaviour
{
    [Header("References")]
    public GameObject abilityEntryPrefab;
    public Transform abilityEntryParent;
    public GameObject skillBookPanel;
    public Button closeButton;
    public List<Ability> allAbilities;

    private bool isOpen = false;

    void Start()
    {
        // Hide panel when game starts
        if (skillBookPanel != null)
            skillBookPanel.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseSkillBook);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (isOpen) CloseSkillBook();
            else OpenSkillBook();
        }
    }

    void OpenSkillBook()
    {
        if (skillBookPanel == null) return;

         skillBookPanel.SetActive(true);
         isOpen = true;
         Time.timeScale = 0f;
         PopulateAbilities();

    }

    void CloseSkillBook()
    {
        if (skillBookPanel == null) return;

         skillBookPanel.SetActive(false);
         isOpen = false;
         Time.timeScale = 1f;

         foreach (Transform child in abilityEntryParent)
         {
             if (child.name != "AbilityEntryBox")
                 Destroy(child.gameObject);
         }

    }


    void PopulateAbilities()
    {
        
        
        if (abilityEntryPrefab == null || abilityEntryParent == null)
        {
            Debug.LogWarning("SkillBookUI: Missing prefab or parent reference.");
            return;
        }

        // Clear existing entries
        foreach (Transform child in abilityEntryParent)
            Destroy(child.gameObject);

        // Create entries for each ability
        foreach (Ability ability in allAbilities)
        {
            GameObject entry = Instantiate(abilityEntryPrefab, abilityEntryParent);

            // Assign visuals
            Image icon = entry.transform.Find("AbilityIcon")?.GetComponent<Image>();
            TMP_Text nameText = entry.transform.Find("RightSidePanel/AbilityNameText")?.GetComponent<TMP_Text>();
            TMP_Text costText = entry.transform.Find("RightSidePanel/AbilityCostText")?.GetComponent<TMP_Text>();
            TMP_Text descText = entry.transform.Find("RightSidePanel/AbilityDescriptionText")?.GetComponent<TMP_Text>();

            if (icon != null) icon.sprite = ability.abilityIcon;
            if (nameText != null) nameText.text = ability.abilityName;
            if (costText != null) costText.text = $"Cost: {ability.resourceCost}";
            if (descText != null) descText.text = ability.abilityDescription;

            // Tooltip hover logic
            EventTrigger trigger = entry.AddComponent<EventTrigger>();

            EventTrigger.Entry enterEvent = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enterEvent.callback.AddListener((_) =>
            {
                Vector2 mousePos = Input.mousePosition;
                FindFirstObjectByType<AbilityTooltipUI>()?.Show(ability, mousePos);
            });
            trigger.triggers.Add(enterEvent);

            EventTrigger.Entry exitEvent = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exitEvent.callback.AddListener((_) =>
            {
                FindFirstObjectByType<AbilityTooltipUI>()?.Hide();
            });
            trigger.triggers.Add(exitEvent);

            // Drag & Drop Pro integration
            ObjectSettings settings = entry.GetComponent<ObjectSettings>();
            if (settings != null)
            {
                // Give each entry a unique ID (can be the ability name)
                settings.Id = ability.abilityName;

                // Make sure the DragDropManager reference exists
                if (DragDropManager.DDM == null)
                    DragDropManager.DDM = FindFirstObjectByType<DragDropManager>();

                // Register this ability with DDM if not already in list
                if (DragDropManager.DDM != null)
                {
                    if (!DragDropManager.DDM.AllObjects.Contains(settings))
                    {
                        DragDropManager.DDM.AllObjects.Add(settings);

                    }
                }


            }
        }
    }

}
