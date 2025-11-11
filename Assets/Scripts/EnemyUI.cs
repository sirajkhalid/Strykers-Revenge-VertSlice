using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(EnemyStats))]
public class EnemyUI : MonoBehaviour
{
    [Header("References")]
    public EnemyStats enemyStats;
    public BattleStateManager battleManager;

    [Header("Mini Bar Settings (above enemy)")]
    public Vector3 healthBarOffset = new Vector3(0, 1.5f, 0);
    public Vector2 barSize = new Vector2(1.5f, 0.2f);
    private Texture2D redTex;
    private Texture2D blackTex;

    [Header("Top Hover UI (Enemy Info)")]
    public GameObject enemyInfoBox;        // EnemyInfoBox from BattleUI
    public TMP_Text enemyNameText;         // EnemyNameText
    public TMP_Text enemyHealthNum;        // EnemyHealthNum
    public Image enemyHealthFill;          // EnemyHealthFill (bar fill)
    public TMP_Text enemyTypeText;         // EnemyTypeText
    public GameObject enemyStatusPanel;    // Optional - currently unused

    [Header("Portrait UI")]
    public GameObject enemyPortraitBox;    // Separate portrait box object
    public Image enemyPortrait;            // Image component inside it

    [Header("Highlight Settings")]
    public Color highlightColor = Color.yellow;

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
            battleManager = Object.FindFirstObjectByType<BattleStateManager>();

        // Create textures for mini bar
        redTex = new Texture2D(1, 1);
        redTex.SetPixel(0, 0, Color.red);
        redTex.Apply();

        blackTex = new Texture2D(1, 1);
        blackTex.SetPixel(0, 0, Color.black);
        blackTex.Apply();

        if (enemyInfoBox != null)
            enemyInfoBox.SetActive(false);

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

    void OnMouseEnter()
    {
        if (Time.timeScale == 0f) return; // game paused; ignore hover

        if (enemyInfoBox != null)
            enemyInfoBox.SetActive(true);

        if (enemyPortraitBox != null)
            enemyPortraitBox.SetActive(true);

        UpdateTopBar();

        if (spriteRenderer != null)
            spriteRenderer.color = highlightColor;
    }

    void OnMouseExit()
    {
        if (Time.timeScale == 0f) return; // game paused; ignore hover exit too

        if (enemyInfoBox != null)
            enemyInfoBox.SetActive(false);

        if (enemyPortraitBox != null)
            enemyPortraitBox.SetActive(false);

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }

    void UpdateTopBar()
    {
        if (enemyStats == null) return;

        float ratio = (float)enemyStats.currentHealth / enemyStats.maxHealth;

        if (enemyNameText != null)
            enemyNameText.text = enemyStats.enemyName;

        if (enemyHealthNum != null)
            enemyHealthNum.text = $"{enemyStats.currentHealth}/{enemyStats.maxHealth}";

        if (enemyHealthFill != null)
            enemyHealthFill.rectTransform.sizeDelta =
                new Vector2(350 * ratio, enemyHealthFill.rectTransform.sizeDelta.y);

        if (enemyTypeText != null)
            enemyTypeText.text = enemyStats.creatureType.ToString();

        if (enemyPortrait != null && enemyStats.enemyPortrait != null)
            enemyPortrait.sprite = enemyStats.enemyPortrait;
    }

    void OnGUI()
    {
        if (battleManager == null || enemyStats == null)
            return;

        if (!battleManager.isBattleActive)
            return;

        Vector3 screenPos = cam.WorldToScreenPoint(transform.position + healthBarOffset);
        screenPos.y = Screen.height - screenPos.y;

        float healthPercent = (float)enemyStats.currentHealth / enemyStats.maxHealth;
        float width = barSize.x * 100;
        float height = barSize.y * 100;

        GUI.DrawTexture(new Rect(screenPos.x - width / 2, screenPos.y - height / 2, width, height), blackTex);
        GUI.DrawTexture(new Rect(screenPos.x - width / 2, screenPos.y - height / 2, width * healthPercent, height), redTex);
    }
}
