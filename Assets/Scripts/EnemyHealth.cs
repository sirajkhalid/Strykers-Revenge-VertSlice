using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHealth : MonoBehaviour
{
    [Header("Enemy Info")]
    public string enemyName = "Enemy Name";
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Mini Bar Settings (above enemy)")]
    public Vector3 healthBarOffset = new Vector3(0, 1.5f, 0);
    public Vector2 barSize = new Vector2(1.5f, 0.2f);
    private Texture2D redTex;
    private Texture2D blackTex;

    [Header("Top Hover UI (Enemy Info Box)")]
    public GameObject enemyInfoBox;   // EnemyInfoBox from BattleUI
    public TMP_Text enemyNameText;    // EnemyNameText (TMP)
    public TMP_Text enemyHealthNum;   // EnemyHealthNum (TMP)
    public Image enemyHealthFill;     // EnemyHealthFill (Image)

    private BattleStateManager battleManager;
    private Camera cam;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    public Color highlightColor = Color.yellow; 


    void Start()
    {
        cam = Camera.main;
        currentHealth = maxHealth;
        battleManager = Object.FindFirstObjectByType<BattleStateManager>();

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        // Create solid-color textures for drawing the mini bar
        redTex = new Texture2D(1, 1);
        redTex.SetPixel(0, 0, Color.red);
        redTex.Apply();

        blackTex = new Texture2D(1, 1);
        blackTex.SetPixel(0, 0, Color.black);
        blackTex.Apply();

        if (enemyInfoBox != null)
            enemyInfoBox.SetActive(false);
    }

    void Update()
    {
        // Quick test: press H to apply damage
        if (Input.GetKeyDown(KeyCode.H))
            TakeDamage(10);
    }

    public void TakeDamage(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
        UpdateTopBar();
    }

    void OnMouseEnter()
    {
        if (battleManager != null && battleManager.isBattleActive && enemyInfoBox != null)
        {
            enemyInfoBox.SetActive(true);
            UpdateTopBar();
        }
        // Highlight sprite on hover
        if (spriteRenderer != null)
            spriteRenderer.color = highlightColor;
    }

    void OnMouseExit()
    {
        if (enemyInfoBox != null)
            enemyInfoBox.SetActive(false);

        // Reset sprite color when mouse leaves
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }

    void UpdateTopBar()
    {
        float ratio = currentHealth / maxHealth;

        if (enemyNameText != null)
            enemyNameText.text = enemyName;

        if (enemyHealthNum != null)
            enemyHealthNum.text = $"{Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(maxHealth)}";

        if (enemyHealthFill != null)
            enemyHealthFill.rectTransform.sizeDelta =
                new Vector2(541 * ratio, enemyHealthFill.rectTransform.sizeDelta.y);
    }

    void OnGUI()
    {
        
        if (battleManager == null || !battleManager.isBattleActive)
            return;

        Vector3 screenPos = cam.WorldToScreenPoint(transform.position + healthBarOffset);
        screenPos.y = Screen.height - screenPos.y; // flip for GUI coordinates

        float healthPercent = currentHealth / maxHealth;
        float width = barSize.x * 100;
        float height = barSize.y * 100;

        // Draw black background
        GUI.DrawTexture(new Rect(screenPos.x - width / 2, screenPos.y - height / 2, width, height), blackTex);
        // Draw red fill
        GUI.DrawTexture(new Rect(screenPos.x - width / 2, screenPos.y - height / 2, width * healthPercent, height), redTex);
    }
}
