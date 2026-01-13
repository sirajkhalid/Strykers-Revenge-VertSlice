using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TurnOrderUI : MonoBehaviour
{
    [Header("References")]
    public GameObject turnPortraitPrefab;   
    public Transform portraitContainer;     


    private readonly Dictionary<GameObject, Image> portraitLookup =
        new Dictionary<GameObject, Image>();

    // Visual order of portraits (left to right)
    private readonly List<Image> orderedPortraits = new List<Image>();

    private GameObject currentHighlightedObj;
    private float slotWidth = 100f; 


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
                Destroy(portraitGO);
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

            RectTransform rt = img.rectTransform;
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            img.color = new Color(1f, 1f, 1f, 0f);
            rt.localScale = Vector3.one * 0.7f;

            orderedPortraits.Add(img);
            portraitLookup.Add(obj, img);
        }

        // Determine slot width from first portrait
        if (orderedPortraits.Count > 0)
        {
            RectTransform rt0 = orderedPortraits[0].rectTransform;
            slotWidth = rt0.rect.width > 0 ? rt0.rect.width : 100f;
        }

        for (int i = 0; i < orderedPortraits.Count; i++)
        {
            RectTransform rt = orderedPortraits[i].rectTransform;
            rt.anchoredPosition = new Vector2(slotWidth * i, 0f);

            // Fade in + pop on build
            imgTweenIntro(orderedPortraits[i]);
        }
    }

    private void imgTweenIntro(Image img)
    {
        RectTransform rt = img.rectTransform;
        img.DOFade(1f, 0.3f);
        rt.DOScale(1f, 0.35f).SetEase(Ease.OutBack);
    }

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
                // Highlight — bigger + brighter
                img.rectTransform.DOScale(1.15f, 0.25f).SetEase(Ease.OutQuad);
                img.DOColor(Color.white, 0.2f);
            }
            else
            {
                // Dim others
                img.rectTransform.DOScale(1f, 0.25f);
                img.DOColor(new Color(0.7f, 0.7f, 0.7f), 0.2f);
            }
        }
    }

    // --------------------------------------------------------------------
    // ADVANCE TURN – rotate row & animate
    // --------------------------------------------------------------------
    public void AdvanceTurn(GameObject finishedObj)
    {
        if (!portraitLookup.TryGetValue(finishedObj, out Image finishedImg))
            return;

        if (orderedPortraits.Count <= 1)
            return;

        // Fade + squash animation
        Sequence preSeq = DOTween.Sequence();
        preSeq.Join(finishedImg.DOFade(0.5f, 0.15f));
        preSeq.Join(finishedImg.rectTransform.DOScale(0.9f, 0.15f));

        preSeq.OnComplete(() =>
        {
            // Move finished portrait to end
            orderedPortraits.Remove(finishedImg);
            orderedPortraits.Add(finishedImg);

            // Update layout order
            for (int i = 0; i < orderedPortraits.Count; i++)
                orderedPortraits[i].transform.SetSiblingIndex(i);

            // Animate reposition
            for (int i = 0; i < orderedPortraits.Count; i++)
            {
                Image img = orderedPortraits[i];
                RectTransform rt = img.rectTransform;

                float targetX = slotWidth * i;
                Vector2 targetPos = new Vector2(targetX, 0f);
                Vector2 overshootPos = targetPos + new Vector2(20f, 0f);

                rt.DOKill();

                Sequence seq = DOTween.Sequence();
                seq.Append(rt.DOAnchorPos(overshootPos, 0.12f));
                seq.Append(rt.DOAnchorPos(targetPos, 0.28f).SetEase(Ease.OutBack));

                seq.Join(img.DOFade(1f, 0.25f));
                seq.Join(rt.DOScale(1.05f, 0.25f));
                seq.Append(rt.DOScale(1f, 0.15f));
            }
        });
    }

    // --------------------------------------------------------------------
    // REMOVE PORTRAIT WHEN COMBATANT DIES
    // --------------------------------------------------------------------
    public void RemovePortrait(GameObject deadObj)
    {
        if (!portraitLookup.TryGetValue(deadObj, out Image img))
            return;

        portraitLookup.Remove(deadObj);
        orderedPortraits.Remove(img);

        if (img != null)
        {
            RectTransform rt = img.rectTransform;

            // Fade + shrink, then destroy
            img.DOFade(0f, 0.25f);
            rt.DOScale(0.3f, 0.25f).SetEase(Ease.InQuad)
              .OnComplete(() =>
              {
                  Destroy(img.gameObject);
                  AnimatePortraitReposition(); 
              });
        }
        else
        {
            // Fallback
            AnimatePortraitReposition();
        }
    }

    // --------------------------------------------------------------------
    // CLEAR UI (when battle ends)
    // --------------------------------------------------------------------
    public void ClearUI()
    {
        portraitLookup.Clear();
        orderedPortraits.Clear();

        if (portraitContainer == null) return;

        for (int i = portraitContainer.childCount - 1; i >= 0; i--)
            Destroy(portraitContainer.GetChild(i).gameObject);
    }

    // --------------------------------------------------------------------
    // SWAP PORTRAITS (for player swap)
    // --------------------------------------------------------------------
    public void UpdatePortraitSprite(GameObject obj, Sprite newSprite)
    {
        if (!portraitLookup.TryGetValue(obj, out Image img) || img == null)
            return;

        img.sprite = newSprite;
    }

    public void ReplacePortraitKey(GameObject oldObj, GameObject newObj, Sprite newSprite)
    {
        if (!portraitLookup.TryGetValue(oldObj, out Image img))
            return;

        portraitLookup.Remove(oldObj);
        portraitLookup[newObj] = img;

        if (img != null && newSprite != null)
            img.sprite = newSprite;
    }
    private void AnimatePortraitReposition()
    {
        for (int i = 0; i < orderedPortraits.Count; i++)
        {
            Image img = orderedPortraits[i];
            if (img == null) continue;

            RectTransform rt = img.rectTransform;
            float targetX = slotWidth * i;
            Vector2 targetPos = new Vector2(targetX, 0f);

            rt.DOKill(); // stop previous animations

            // Smooth slide into new position
            rt.DOAnchorPos(targetPos, 0.25f).SetEase(Ease.OutQuad);
        }
    }

}
