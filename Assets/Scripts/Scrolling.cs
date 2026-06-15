using UnityEngine;
using UnityEngine.UI;

public class Scrolling : MonoBehaviour
{
    [Header("UI References")]
    public ScrollRect scrollRect; // Assign your ScrollRect in Inspector

    [Header("Scroll Settings")]
    public float scrollSpeed = 0.5f; // Units per second (0-1 range for normalized position)
    public bool autoStart = true;    // Start scrolling automatically

    private bool isScrolling = false;

    void Start()
    {
        if (scrollRect == null)
        {
            Debug.LogError("ScrollRect not assigned!");
            enabled = false;
            return;
        }

        if (autoStart)
            StartScrolling();
    }

    void Update()
    {
        if (!isScrolling) return;

        // Scroll down over time
        scrollRect.verticalNormalizedPosition -= scrollSpeed * Time.deltaTime;

        // Clamp and stop at bottom
        if (scrollRect.verticalNormalizedPosition <= 0f)
        {
            scrollRect.verticalNormalizedPosition = 0f;
            isScrolling = false;
        }
    }

    /// <summary>
    /// Call this to start auto-scrolling from current position.
    /// </summary>
    public void StartScrolling()
    {
        isScrolling = true;
    }

    /// <summary>
    /// Call this to stop auto-scrolling.
    /// </summary>
    public void StopScrolling()
    {
        isScrolling = false;
    }
}