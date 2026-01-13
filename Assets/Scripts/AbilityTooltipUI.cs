using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class AbilityTooltipUI : MonoBehaviour
{
    [Header("Tooltip References")]
    public GameObject tooltipPanel;
    public TMP_Text abilityNameText;
    public TMP_Text abilityDescriptionText;

    [Header("Main Lines")]
    public TMP_Text damageText;            
    public TMP_Text targetRangeText;       
    public TMP_Text scalingAndEffectText;  
    public TMP_Text usesPerBattleText;


    [Header("Icons")]
    public Image categoryIcon;             // Melee / Ranged / Magic / etc
    public Image damageTypeIcon;           // Physical / Fire / etc

    [Header("Action Icons")]
    public Image actionIconImage;          // normal Action
    public Image bonusActionIconImage;     // Bonus Action
    public Image spellSlotIconImage;       // Spell slot I / II / Cantrip

    [Header("Category Sprites")]
    public Sprite meleeCategorySprite;
    public Sprite rangedCategorySprite;
    public Sprite magicCategorySprite;
    public Sprite supportCategorySprite;
    public Sprite utilityCategorySprite;
    public Sprite passiveCategorySprite;

    [Header("Damage Type Sprites")]
    public Sprite physicalSprite;
    public Sprite fireSprite;
    public Sprite coldSprite;
    public Sprite lightningSprite;
    public Sprite poisonSprite;
    public Sprite holySprite;
    public Sprite shadowSprite;

    [Header("Action Sprites")]
    public Sprite actionSprite;
    public Sprite bonusActionSprite;
    public Sprite freeActionSprite;
    public Sprite spellSlotLevel1Sprite;
    public Sprite spellSlotLevel2Sprite;

    [Header("Category Colors (optional)")]
    public Color meleeColor = Color.red;
    public Color rangedColor = new Color(1f, 0.85f, 0.2f);
    public Color magicColor = new Color(0.4f, 0.6f, 1f);
    public Color supportColor = new Color(0.4f, 1f, 0.4f);
    public Color utilityColor = Color.gray;
    public Color passiveColor = new Color(0.7f, 0.5f, 1f);

    private RectTransform tooltipRect;
    private CanvasGroup canvasGroup;
    private static AbilityTooltipUI instance;
    private Tween showTween;
    private Outline borderOutline;
    private Tween glowTween;


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
        {
            tooltipRect = tooltipPanel.GetComponent<RectTransform>();
            canvasGroup = tooltipPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = tooltipPanel.AddComponent<CanvasGroup>();
        }
        borderOutline = tooltipPanel.GetComponent<Outline>();

    }

    void Start()
    {
        StartCoroutine(HideNextFrame());
    }

    System.Collections.IEnumerator HideNextFrame()
    {
        yield return null;
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
            canvasGroup.alpha = 0f;
        }
    }

    void OnEnable()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;
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

        // Stop any previous tween
        showTween?.Kill();

        // Position near mouse
        tooltipRect.position = position + new Vector2(15f, -15f);

        // Fill all UI
        SetupHeader(ability);
        SetupIcons(ability);
        SetupDamageLine(ability);
        SetupTargetRangeLine(ability);
        SetupScalingAndEffectLine(ability);
        SetupActionIcons(ability);
        SetupUsesPerBattle(ability);

        tooltipRect.localScale = Vector3.one * 0.8f;
        canvasGroup.alpha = 0f;

        showTween = DOTween.Sequence()

            .Join(canvasGroup.DOFade(1f, 0.20f))

            .Join(tooltipRect.DOScale(1.05f, 0.22f).SetEase(Ease.OutQuad))
           
            .Append(tooltipRect.DOScale(1f, 0.10f).SetEase(Ease.OutSine));

        StartGlowEffect(ability);

    }

    public void Hide()
    {
        if (tooltipPanel == null)
            return;

        showTween?.Kill();
        tooltipPanel.SetActive(false);
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        glowTween?.Kill();

    }

    void Update()
    {
        // Follows the mouse
        if (tooltipPanel != null && tooltipPanel.activeSelf && tooltipRect != null)
        {
            tooltipRect.position = (Vector2)Input.mousePosition + new Vector2(15f, -15f);
        }
    }

    // SETUP HELPERS 

    private void SetupHeader(Ability ability)
    {
        if (abilityNameText != null)
            abilityNameText.text = ability.abilityName;

        if (abilityDescriptionText != null)
            abilityDescriptionText.text = ability.abilityDescription;
    }

    private void SetupIcons(Ability ability)
    {
        // Category icon + color
        if (categoryIcon != null)
        {
            Sprite s = null;
            Color c = Color.white;

            switch (ability.category)
            {
                case Ability.AbilityCategory.Melee:
                    s = meleeCategorySprite;
                    c = meleeColor;
                    break;
                case Ability.AbilityCategory.Ranged:
                    s = rangedCategorySprite;
                    c = rangedColor;
                    break;
                case Ability.AbilityCategory.Magic:
                    s = magicCategorySprite;
                    c = magicColor;
                    break;
                case Ability.AbilityCategory.Support:
                    s = supportCategorySprite;
                    c = supportColor;
                    break;
                case Ability.AbilityCategory.Utility:
                    s = utilityCategorySprite;
                    c = utilityColor;
                    break;
                case Ability.AbilityCategory.Passive:
                    s = passiveCategorySprite;
                    c = passiveColor;
                    break;
            }

            categoryIcon.sprite = s;
            categoryIcon.gameObject.SetActive(s != null);

            // Tint name with category color 
            if (abilityNameText != null)
                abilityNameText.color = c;
        }

        // Damage type icon 
        if (damageTypeIcon != null)
        {
            Sprite s = null;
            switch (ability.damageType)
            {
                case Ability.DamageType.Physical: s = physicalSprite; break;
                case Ability.DamageType.Fire: s = fireSprite; break;
                case Ability.DamageType.Cold: s = coldSprite; break;
                case Ability.DamageType.Lightning: s = lightningSprite; break;
                case Ability.DamageType.Poison: s = poisonSprite; break;
                case Ability.DamageType.Holy: s = holySprite; break;
                case Ability.DamageType.Shadow: s = shadowSprite; break;
            }

            damageTypeIcon.sprite = s;
            damageTypeIcon.gameObject.SetActive(s != null);
        }

        if (borderOutline != null)
        {
            switch (ability.category)
            {
                case Ability.AbilityCategory.Melee:
                    borderOutline.effectColor = meleeColor;
                    break;

                case Ability.AbilityCategory.Ranged:
                    borderOutline.effectColor = rangedColor;
                    break;

                case Ability.AbilityCategory.Magic:
                    borderOutline.effectColor = magicColor;
                    break;

                case Ability.AbilityCategory.Support:
                    borderOutline.effectColor = supportColor;
                    break;

                case Ability.AbilityCategory.Utility:
                    borderOutline.effectColor = utilityColor;
                    break;

                case Ability.AbilityCategory.Passive:
                    borderOutline.effectColor = passiveColor;
                    break;
            }
        }

    }

    private void SetupDamageLine(Ability ability)
    {
        if (damageText == null)
            return;

        CharacterStats stats = GetActiveStats();

        // --- SUPPORT / UTILITY / PASSIVE ---  
        if (ability.category == Ability.AbilityCategory.Support ||
            ability.category == Ability.AbilityCategory.Utility ||
            ability.category == Ability.AbilityCategory.Passive)
        {
            // SPECIAL CASE: HEALING
            if (ability.abilityName.ToLower().Contains("heal"))
            {
                int mod = GetScalingValue(stats, ability.scalingAttribute);

                int minHeal = Mathf.RoundToInt(Mathf.Abs(ability.baseDamage) + mod);
                int maxHeal = Mathf.RoundToInt(Mathf.Abs(ability.baseDamage) + mod * 2);

                damageText.text = $"Heals: {minHeal} – {maxHeal}";
                
                damageText.gameObject.SetActive(true);
            }
            else
            {
                // No damage line at all
                damageText.gameObject.SetActive(false);
            }

            return;
        }

        // --- NORMAL DAMAGE ABILITIES ---
        bool hasDamage = (ability.baseDamage > 0) ||
                         (ability.numberOfDice > 0 && ability.diceSides > 0);

        if (!hasDamage)
        {
            damageText.gameObject.SetActive(false);
            return;
        }

        int attackMod = GetScalingValue(stats, ability.scalingAttribute);

        int minDice = ability.numberOfDice > 0 ? ability.numberOfDice * 1 : 0;
        int maxDice = ability.numberOfDice > 0 ? ability.numberOfDice * ability.diceSides : 0;

        int minTotal = Mathf.Max(0, ability.baseDamage + minDice + attackMod);
        int maxTotal = Mathf.Max(0, ability.baseDamage + maxDice + attackMod);

        damageText.text = $"Damage: {minTotal} – {maxTotal}";
        
        damageText.gameObject.SetActive(true);
    }



    private void SetupTargetRangeLine(Ability ability)
    {
        if (targetRangeText == null)
            return;

        // Target type
        string target = ability.targetType.ToString(); 

        // Range & radius
        string range = ability.range > 0.01f
            ? $"Range {ability.range:0.#}m"
            : "Melee";

        string radius = ability.areaRadius > 0.01f
            ? $"Radius {ability.areaRadius:0.#}m"
            : "";

        // Build compact line
        string line = target;

        if (!string.IsNullOrEmpty(range))
            line += $" • {range}";

        if (!string.IsNullOrEmpty(radius))
            line += $" • {radius}";

        if (string.IsNullOrWhiteSpace(line) || line == ability.targetType.ToString())
        {
            targetRangeText.gameObject.SetActive(false);
        }
        else
        {
            targetRangeText.text = line;
            targetRangeText.gameObject.SetActive(true);
        }

    }

    private void SetupScalingAndEffectLine(Ability ability)
    {
        if (ability.category == Ability.AbilityCategory.Utility ||
            ability.category == Ability.AbilityCategory.Passive)
        {
            if (ability.appliesStatusEffect && !string.IsNullOrEmpty(ability.statusEffectName))
            {
                scalingAndEffectText.text = ability.statusEffectName;
                scalingAndEffectText.gameObject.SetActive(true);
            }
            else
            {
                scalingAndEffectText.gameObject.SetActive(false);
            }
            return;
        }
        if (scalingAndEffectText == null)
            return;

        string scaling = GetScalingShort(ability.scalingAttribute);
        string effect = "";

        if (ability.appliesStatusEffect && !string.IsNullOrEmpty(ability.statusEffectName))
            effect = ability.statusEffectName;

        if (!string.IsNullOrEmpty(scaling) && !string.IsNullOrEmpty(effect))
            scalingAndEffectText.text = $"{scaling} • {effect}";
        else if (!string.IsNullOrEmpty(scaling))
            scalingAndEffectText.text = scaling;
        else if (!string.IsNullOrEmpty(effect))
            scalingAndEffectText.text = effect;
        else
        {
            scalingAndEffectText.gameObject.SetActive(false);
            return;
        }

        scalingAndEffectText.gameObject.SetActive(true);
    }

    private void SetupActionIcons(Ability ability)
    {
        if (actionIconImage != null)
        {
            actionIconImage.gameObject.SetActive(false);
            actionIconImage.sprite = null;
        }
        if (bonusActionIconImage != null)
        {
            bonusActionIconImage.gameObject.SetActive(false);
            bonusActionIconImage.sprite = null;
        }
        if (spellSlotIconImage != null)
        {
            spellSlotIconImage.gameObject.SetActive(false);
            spellSlotIconImage.sprite = null;
        }

        // Action type icons
        if (ability.consumesAction && ability.actionType == Ability.ActionType.Action &&
            actionIconImage != null && actionSprite != null)
        {
            actionIconImage.sprite = actionSprite;
            actionIconImage.gameObject.SetActive(true);
        }

        if (ability.consumesBonusAction && ability.actionType == Ability.ActionType.BonusAction &&
            bonusActionIconImage != null && bonusActionSprite != null)
        {
            bonusActionIconImage.sprite = bonusActionSprite;
            bonusActionIconImage.gameObject.SetActive(true);
        }

        // Spell slot icon
        if (ability.usesSpellSlot)
        {
            Sprite s = null;

            if (ability.spellLevel == 1)
                s = spellSlotLevel1Sprite;
            else if (ability.spellLevel >= 2)
                s = spellSlotLevel2Sprite;

            if (s != null)
            {
                spellSlotIconImage.sprite = s;
                spellSlotIconImage.gameObject.SetActive(true);
            }
        }

    }

    //HELPERS 

    private CharacterStats GetActiveStats()
    {
        var party = FindFirstObjectByType<PlayerPartyController>();
        if (party == null || party.activeMember == null)
            return null;

        return party.activeMember.GetComponent<CharacterStats>();
    }

    private int GetScalingValue(CharacterStats stats, Ability.ScalingAttribute attr)
    {
        if (stats == null)
            return 0;

        switch (attr)
        {
            case Ability.ScalingAttribute.Strength: return stats.strength;
            case Ability.ScalingAttribute.Dexterity: return stats.dexterity;
            case Ability.ScalingAttribute.Constitution: return stats.constitution;
            case Ability.ScalingAttribute.Intelligence: return stats.intelligence;
            case Ability.ScalingAttribute.Wisdom: return stats.wisdom;
            case Ability.ScalingAttribute.Charisma: return stats.charisma;
            default: return 0;
        }
    }

    private string GetScalingShort(Ability.ScalingAttribute attr)
    {
        switch (attr)
        {
            case Ability.ScalingAttribute.Strength: return "STR";
            case Ability.ScalingAttribute.Dexterity: return "DEX";
            case Ability.ScalingAttribute.Constitution: return "CON";
            case Ability.ScalingAttribute.Intelligence: return "INT";
            case Ability.ScalingAttribute.Wisdom: return "WIS";
            case Ability.ScalingAttribute.Charisma: return "CHA";
            default: return "";
        }
    }
    private void SetupUsesPerBattle(Ability ability)
    {
        if (usesPerBattleText == null)
            return;

        // LIFETIME USES
        if (ability.MaxLifetimeUses > 0)
        {
            int left = Mathf.Max(0, ability.MaxLifetimeUses - ability.LifetimeUses);
            usesPerBattleText.text = $"Uses Left: {left}/{ability.MaxLifetimeUses}";
            usesPerBattleText.gameObject.SetActive(true);
            return;
        }

        // PER BATTLE USES 
        if (ability.maxUsesPerBattle > 0)
        {
            int left = Mathf.Max(0, ability.maxUsesPerBattle - ability.usesThisBattle);
            usesPerBattleText.text = $"Uses Left: {left}/{ability.maxUsesPerBattle}";
            usesPerBattleText.gameObject.SetActive(true);
            return;
        }

        // No limits → hide
        usesPerBattleText.gameObject.SetActive(false);
    }

    private void StartGlowEffect(Ability ability)
    {
        if (borderOutline == null)
            return;

        glowTween?.Kill();

        // Determine base category color
        Color baseColor = Color.white;

        switch (ability.category)
        {
            case Ability.AbilityCategory.Melee:
                baseColor = meleeColor;
                break;

            case Ability.AbilityCategory.Ranged:
                baseColor = rangedColor;
                break;

            case Ability.AbilityCategory.Magic:
                baseColor = magicColor;
                break;

            case Ability.AbilityCategory.Support:
                baseColor = supportColor;
                break;

            case Ability.AbilityCategory.Utility:
                baseColor = utilityColor;
                break;

            case Ability.AbilityCategory.Passive:
                baseColor = passiveColor;
                break;
        }

        // Start with solid category color
        borderOutline.effectColor = baseColor;

        // Animate glow 
        glowTween = DOTween.Sequence()
            .Append(borderOutline.DOFade(0.25f, 0.6f))  // dim
            .Append(borderOutline.DOFade(1f, 0.6f))     // bright
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

}
