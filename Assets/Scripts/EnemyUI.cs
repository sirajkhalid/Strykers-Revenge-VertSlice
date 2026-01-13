using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(EnemyStats))]
public class EnemyUI : MonoBehaviour
{
    [Header("References")]
    public EnemyStats enemyStats;
    public BattleStateManager battleManager;

    private Texture2D redTex;
    private Texture2D blackTex;

    [Header("Mini Bar Animation")]
    private float miniDisplayedHP = 1f; 

    [Header("Top Hover UI (Enemy Info)")]
    public GameObject enemyInfoBox;
    public TMP_Text enemyNameText;
    public TMP_Text enemyHealthNum;
    public Image enemyHealthFill;
    public TMP_Text enemyTypeText;

    [Header("Portrait UI")]
    public GameObject enemyPortraitBox;
    public Image enemyPortrait;

    private Camera cam;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Awake()
    {
        cam = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        if (enemyStats == null)
            enemyStats = GetComponent<EnemyStats>();

        if (battleManager == null)
            battleManager = FindFirstObjectByType<BattleStateManager>();

        redTex = new Texture2D(1, 1);
        redTex.SetPixel(0, 0, Color.red);
        redTex.Apply();

        blackTex = new Texture2D(1, 1);
        blackTex.SetPixel(0, 0, Color.black);
        blackTex.Apply();

    if (enemyInfoBox != null)
            {
                Image bg = enemyInfoBox.GetComponent<Image>();
                if (bg != null)
                {
                    Color c = bg.color;
                    c.a = 0f;
                    bg.color = c;
                }
        }


        if (enemyPortraitBox != null)
            enemyPortraitBox.SetActive(false);
    }

    void Start()
    {
        if (enemyStats != null)
            enemyStats.OnHealthChanged += UpdateTopBar;
    }

    void OnDestroy()
    {
        if (enemyStats != null)
            enemyStats.OnHealthChanged -= UpdateTopBar;
    }

    // TOP BAR UI

    public void UpdateTopBar()
    {
        if (enemyStats == null) return;

        float hpPercent = Mathf.Clamp01((float)enemyStats.currentHealth / enemyStats.maxHealth);

        // Name
        if (enemyNameText != null)
            enemyNameText.text = enemyStats.enemyName;

        // Health Text
        if (enemyHealthNum != null)
            enemyHealthNum.text = $"{enemyStats.currentHealth}/{enemyStats.maxHealth}";

        // Smooth fill animation
        if (enemyHealthFill != null)
        {
            RectTransform rt = enemyHealthFill.rectTransform;

            rt.DOKill();
            rt.DOSizeDelta(
                new Vector2(350f * hpPercent, rt.sizeDelta.y),
                0.28f
            ).SetEase(Ease.OutCubic);
        }

        // Creature Type
        if (enemyTypeText != null)
            enemyTypeText.text = enemyStats.creatureType.ToString();

        // Portrait
        if (enemyPortrait != null && enemyStats.enemyPortrait != null)
            enemyPortrait.sprite = enemyStats.enemyPortrait;
    }


    // MINI BAR 
    void OnGUI()
    {
        if (battleManager == null || enemyStats == null)
            return;

        if (!battleManager.isBattleActive)
            return;

        if (enemyStats.currentHealth <= 0)
            return;

        if (spriteRenderer == null)
            return;

        Bounds b = spriteRenderer.bounds;

        Vector3 worldPos = new Vector3(
            b.center.x,
            b.max.y + 0.30f,       
            b.center.z
        );

        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);
        screenPos.y = Screen.height - screenPos.y;


        float width = b.size.x * 60f;   
        float height = b.size.y * 5f;   

        float realHP = Mathf.Clamp01((float)enemyStats.currentHealth / enemyStats.maxHealth);


        miniDisplayedHP = Mathf.Lerp(miniDisplayedHP, realHP, Time.deltaTime * 10f);


        GUI.DrawTexture(
            new Rect(screenPos.x - width / 2, screenPos.y - height / 2, width, height),
            blackTex
        );


        GUI.DrawTexture(
            new Rect(screenPos.x - width / 2, screenPos.y - height / 2, width * miniDisplayedHP, height),
            redTex
        );
    }



    public void ShowInfoUI()
    {
        if (enemyInfoBox != null)
            enemyInfoBox.SetActive(true);
        if (enemyPortraitBox != null)
            enemyPortraitBox.SetActive(true);

        FadeInfoUI(true); 
        UpdateTopBar();
    }

    public void HideInfoUI()
    {
        FadeInfoUI(false); 

        // turn off objects after fade completes
        StartCoroutine(DisableAfterFade());
    }

    private IEnumerator DisableAfterFade()
    {
        yield return new WaitForSeconds(0.25f);

        if (enemyInfoBox != null)
            enemyInfoBox.SetActive(false);

        if (enemyPortraitBox != null)
            enemyPortraitBox.SetActive(false);
    }

    public void ResetColor()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }


    // HIGHLIGHT METHODS

    public void SetTemporaryHighlight(Color color)
    {
        if (spriteRenderer != null)
            spriteRenderer.color = color;
    }

    public void SetPermanentHighlight(Color color)
    {
        if (spriteRenderer != null)
            spriteRenderer.color = color;
    }

    // DOTWEEN UI FADE
    public void FadeInfoUI(bool fadeIn)
    {
        float target = fadeIn ? 1f : 0f;
        float duration = 0.25f;

        if (fadeIn)
        {
            if (enemyInfoBox != null)
                enemyInfoBox.SetActive(true);

            if (enemyPortraitBox != null)
                enemyPortraitBox.SetActive(true);
        }

        // Name
        if (enemyNameText != null)
        {
            enemyNameText.DOKill();                     
            enemyNameText.DOFade(target, duration);
        }

        // HP Number
        if (enemyHealthNum != null)
        {
            enemyHealthNum.DOKill();
            enemyHealthNum.DOFade(target, duration);
        }

        // Type Text
        if (enemyTypeText != null)
        {
            enemyTypeText.DOKill();
            enemyTypeText.DOFade(target, duration);
        }

        // Portrait
        if (enemyPortrait != null)
        {
            enemyPortrait.DOKill();
            enemyPortrait.DOFade(target, duration);
        }

        // Portrait BG
        if (enemyPortraitBox != null)
        {
            Image bg = enemyPortraitBox.GetComponent<Image>();
            if (bg != null)
            {
                bg.DOKill();
                bg.DOFade(target, duration);
            }
        }

        // Health Fill bar
        if (enemyHealthFill != null)
        {
            var img = enemyHealthFill.GetComponent<Image>();
            if (img != null)
            {
                img.DOKill();
                img.DOFade(target, duration);
            }
        }

        // After fade OUT, fully disable the UI 
        if (!fadeIn)
        {
            DOVirtual.DelayedCall(duration, () =>
            {
                if (enemyInfoBox != null)
                    enemyInfoBox.SetActive(false);

                if (enemyPortraitBox != null)
                    enemyPortraitBox.SetActive(false);
            });
        }
    }


    public void PunchUI()
    {
        if (enemyInfoBox == null) return;

        RectTransform rt = enemyInfoBox.GetComponent<RectTransform>();
        if (rt == null) return;

        rt.DOKill();
        rt.localScale = Vector3.one;

        rt.DOPunchScale(new Vector3(0.12f, 0.12f, 0f), 0.25f, 10, 1);
    }



}
