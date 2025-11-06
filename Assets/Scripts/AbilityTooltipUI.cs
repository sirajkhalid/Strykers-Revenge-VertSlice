using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityTooltipUI : MonoBehaviour
{
    [Header("Tooltip References")]
    public GameObject tooltipPanel; // TooltipPanel prefab
    public TMP_Text abilityTypeText;
    public TMP_Text abilityDamageText;
    public TMP_Text abilityScalingText;
    public TMP_Text abilityEffectText;

    private RectTransform tooltipRect;
    private static AbilityTooltipUI instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        if (tooltipPanel == null)
            tooltipPanel = transform.Find("TooltipPanel")?.gameObject;

        if (tooltipPanel != null)
            tooltipRect = tooltipPanel.GetComponent<RectTransform>();
    }

    void Start()
    {
        // Hide tooltip safely
        StartCoroutine(HideNextFrame());
    }

    System.Collections.IEnumerator HideNextFrame()
    {
        // Wait one frame so Unity UI can finish its internal setup
        yield return null;

        if (tooltipPanel != null && tooltipPanel.activeSelf)
        {
            tooltipPanel.SetActive(false);
            tooltipPanel.SetActive(false);
        }
    }


    void OnEnable()
    {
        // Just in case Unity re-enables the tooltip object
        if (tooltipPanel != null && tooltipPanel.activeSelf)
        {
            tooltipPanel.SetActive(false);
        }
    }

    public static AbilityTooltipUI Get()
    {
        return instance;
    }

    public void Show(Ability ability, Vector2 position)
    {
        if (tooltipPanel == null || ability == null)
            return;

        tooltipPanel.SetActive(true);

        abilityTypeText.text = $"Type: {ability.category}";
        abilityDamageText.text = $"Damage: {ability.baseDamage} ({ability.damageType})";
        abilityScalingText.text = $"Scales with: {ability.scalingAttribute}";
        abilityEffectText.text = ability.appliesStatusEffect
            ? $"Effect: {ability.statusEffectName} ({ability.statusDuration:F1}s)"
            : "Effect: None";

        tooltipRect.position = (Vector2)position + new Vector2(15f, -15f);
    }

    public void Hide()
    {
        if (tooltipPanel != null && tooltipPanel.activeSelf)
        {
            tooltipPanel.SetActive(false);
        }
    }

    void Update()
    {
        // Follows the mouse
        if (tooltipPanel != null && tooltipPanel.activeSelf)
            tooltipRect.position = (Vector2)Input.mousePosition + new Vector2(15f, -15f);
    }
}
