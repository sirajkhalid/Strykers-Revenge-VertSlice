using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TurnOrderUI : MonoBehaviour
{
    [Header("References")]
    public GameObject turnPortraitPrefab;     // Prefab with ONLY an Image component
    public Transform portraitContainer;       // Horizontal container

    private readonly Dictionary<GameObject, Image> portraitLookup
        = new Dictionary<GameObject, Image>();

    private GameObject currentHighlightedObj;

    void Awake()
    {
        ClearUI();
    }

    // --------------------------------------------------------------------
    // BUILD TURN ORDER BAR
    // --------------------------------------------------------------------
    public void BuildTurnOrder(List<GameObject> combatants)
    {
        ClearUI();

        if (combatants == null) return;

        foreach (GameObject obj in combatants)
        {
            if (obj == null) continue;

            GameObject portraitGO = Instantiate(turnPortraitPrefab, portraitContainer);
            Image img = portraitGO.GetComponent<Image>();

            if (img == null)
            {
                Debug.LogError("TurnPortraitPrefab MUST have an Image component!");
                continue;
            }

            // Assign correct portrait sprite
            if (obj.TryGetComponent<CharacterStats>(out var cs))
            {
                if (cs.characterSquarePortrait != null)
                    img.sprite = cs.characterSquarePortrait;
            }
            else if (obj.TryGetComponent<EnemyStats>(out var es))
            {
                if (es.enemyPortrait != null)
                    img.sprite = es.enemyPortrait;
            }

            // Start invisible
            img.color = new Color(1f, 1f, 1f, 0f);
            img.transform.localScale = Vector3.one * 0.7f;

            // Fade in + pop
            img.DOFade(1f, 0.3f);
            img.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack);

            portraitLookup.Add(obj, img);
        }
    }


    // --------------------------------------------------------------------
    // HIGHLIGHT CURRENT TURN
    // --------------------------------------------------------------------
    public void UpdateTurnHighlight(GameObject currentObj)
    {
        currentHighlightedObj = currentObj;

        foreach (var kvp in portraitLookup)
        {
            GameObject obj = kvp.Key;
            Image img = kvp.Value;

            if (img == null) continue;

            img.DOKill();

            if (obj == currentObj)
            {
                // Highlight the active portrait — bigger + brighter
                img.transform.DOScale(1.15f, 0.25f).SetEase(Ease.OutQuad);
                img.DOColor(Color.white, 0.2f);
            }
            else
            {
                // Dim the rest
                img.transform.DOScale(1f, 0.25f).SetEase(Ease.OutQuad);
                img.DOColor(new Color(0.7f, 0.7f, 0.7f), 0.2f);
            }
        }
    }

    // --------------------------------------------------------------------
    // REMOVE PORTRAIT WHEN COMBATANT DIES
    // --------------------------------------------------------------------
    public void RemovePortrait(GameObject deadObj)
    {
        if (!portraitLookup.ContainsKey(deadObj))
            return;

        Image img = portraitLookup[deadObj];

        portraitLookup.Remove(deadObj);

        if (img != null)
        {
            // Fade-out & shrink, then destroy
            img.DOFade(0f, 0.25f);
            img.transform.DOScale(0.3f, 0.25f).SetEase(Ease.InQuad)
                .OnComplete(() => Destroy(img.gameObject));
        }
    }

    public void ClearUI()
    {
        portraitLookup.Clear();

        for (int i = portraitContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(portraitContainer.GetChild(i).gameObject);
        }
    }
    public void UpdatePortraitSprite(GameObject obj, Sprite newSprite)
    {
        if (!portraitLookup.ContainsKey(obj))
            return;

        Image img = portraitLookup[obj];
        if (img == null) return;

        img.sprite = newSprite;
    }

}
