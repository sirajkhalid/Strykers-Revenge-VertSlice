using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

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
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        Vector3 mouseWorld = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;

        RaycastHit2D hit = Physics2D.Raycast(mouseWorld, Vector2.zero);

        EnemyUI enemyUI = hit.collider ? hit.collider.GetComponent<EnemyUI>() : null;

        if (enemyUI != hoveredEnemyUI)
        {

            if (hoveredEnemyUI != null && hoveredEnemyUI != selectedEnemyUI)
            {
                hoveredEnemyUI.ResetColor();
                hoveredEnemyUI.FadeInfoUI(false);
            }

            hoveredEnemyUI = enemyUI;


            if (hoveredEnemyUI != null && hoveredEnemyUI != selectedEnemyUI)
            {
                hoveredEnemyUI.SetTemporaryHighlight(hoverColor);
                hoverTarget = hoveredEnemyUI.transform;

                hoveredEnemyUI.ShowInfoUI();
                hoveredEnemyUI.FadeInfoUI(true);
                hoveredEnemyUI.UpdateTopBar();
            }
            else
            {
                hoverTarget = null;


                if (selectedEnemyUI != null)
                {
                    selectedEnemyUI.ShowInfoUI();
                    selectedEnemyUI.FadeInfoUI(true);
                    selectedEnemyUI.UpdateTopBar();
                }
            }
        }
    }

    // SELECTION (LEFT CLICK)
    void HandleSelection()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;
        if (Input.GetMouseButtonDown(0))
        {
            // Clicked on an enemy
            if (hoveredEnemyUI != null)
            {
                // Remove highlight & fade from old selected
                if (selectedEnemyUI != null && selectedEnemyUI != hoveredEnemyUI)
                {
                    selectedEnemyUI.ResetColor();
                    selectedEnemyUI.FadeInfoUI(false);
                }

                // Set new selected enemy
                selectedEnemyUI = hoveredEnemyUI;
                selectedEnemyUI.SetPermanentHighlight(selectedColor);
                lockedTarget = selectedEnemyUI.transform;

                selectedEnemyUI.ShowInfoUI();
                selectedEnemyUI.FadeInfoUI(true);
                selectedEnemyUI.UpdateTopBar();

                selectedEnemyUI.PunchUI();
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

        EnemyUI[] all = FindObjectsByType<EnemyUI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var ui in all)
        {
            ui.ResetColor();
            ui.FadeInfoUI(false);
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
            ui.FadeInfoUI(true);
            ui.UpdateTopBar();
            ui.SetPermanentHighlight(selectedColor);
            ui.PunchUI();
        }
    }
}
