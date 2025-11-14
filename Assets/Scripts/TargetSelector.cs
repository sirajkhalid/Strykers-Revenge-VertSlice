using UnityEngine;

public class TargetSelector : MonoBehaviour
{
    [Header("Highlight Colors")]
    public Color hoverColor = Color.yellow;
    public Color selectedColor = Color.red;

    private EnemyUI hoveredEnemyUI;
    private EnemyUI selectedEnemyUI;
    private Camera mainCam;


    public Transform hoverTarget;
    public Transform lockedTarget;

    void Start()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        HandleHover();
        HandleSelection();
    }

    void HandleHover()
    {
        Vector3 mouseWorld = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;

        RaycastHit2D hit = Physics2D.Raycast(mouseWorld, Vector2.zero);

        EnemyUI enemyUI = hit.collider ? hit.collider.GetComponent<EnemyUI>() : null;

        if (enemyUI != hoveredEnemyUI)
        {
            // Remove hover highlight from previous hovered enemy (if not selected)
            if (hoveredEnemyUI != null && hoveredEnemyUI != selectedEnemyUI)
                hoveredEnemyUI.ResetColor();

            hoveredEnemyUI = enemyUI;

            // Apply hover highlight
            if (hoveredEnemyUI != null && hoveredEnemyUI != selectedEnemyUI)
            {
                hoveredEnemyUI.SetTemporaryHighlight(hoverColor);
                hoverTarget = hoveredEnemyUI.transform;

                // Hover UI has highest priority
                hoveredEnemyUI.ShowInfoUI();
                hoveredEnemyUI.UpdateTopBar();
            }
            else
            {
                hoverTarget = null;

                // If nothing hovered → show locked target UI (if exists)
                if (selectedEnemyUI != null)
                {
                    selectedEnemyUI.ShowInfoUI();
                    selectedEnemyUI.UpdateTopBar();
                }
            }
        }
    }

    // -------------------------------------------------------
    // SELECTION (LEFT CLICK)
    // -------------------------------------------------------
    void HandleSelection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Clicked on an enemy
            if (hoveredEnemyUI != null)
            {
                // Remove highlight from old selected
                if (selectedEnemyUI != null && selectedEnemyUI != hoveredEnemyUI)
                    selectedEnemyUI.ResetColor();

                // Set new selected enemy
                selectedEnemyUI = hoveredEnemyUI;
                selectedEnemyUI.SetPermanentHighlight(selectedColor);

                lockedTarget = selectedEnemyUI.transform;

                // Display its UI
                selectedEnemyUI.ShowInfoUI();
                selectedEnemyUI.UpdateTopBar();
            }
            else
            {
                // Clicked empty space → clear target
                ClearSelection();
            }
        }
    }

    public Transform GetCurrentTarget()
    {
        if (selectedEnemyUI == null) return null;
        return selectedEnemyUI.enemyStats != null
            ? selectedEnemyUI.enemyStats.transform
            : null;
    }


    public void ClearSelection()
    {
        lockedTarget = null;
        hoverTarget = null;

        // Clear highlights
        EnemyUI[] all = FindObjectsByType<EnemyUI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var ui in all)
        {
            ui.ResetColor();
            ui.HideInfoUI();
        }

        selectedEnemyUI = null;
        hoveredEnemyUI = null;
    }

    public void LockTarget(Transform t)
    {
        lockedTarget = t;
        hoverTarget = null;

        EnemyUI ui = t.GetComponent<EnemyUI>();
        if (ui != null)
        {
            ui.ShowInfoUI();
            ui.UpdateTopBar();
            ui.SetPermanentHighlight(selectedColor);
        }
    }
}
