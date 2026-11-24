using UnityEngine;

/// <summary>Positions the P12 screen-space UI inside the device safe area.</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public class ARUISafeArea : MonoBehaviour
{
    private RectTransform rectTransform;
    private Rect lastSafeArea;
    private int lastWidth;
    private int lastHeight;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    private void Update()
    {
        if (rectTransform == null)
            return;

        if (lastSafeArea != Screen.safeArea || lastWidth != Screen.width || lastHeight != Screen.height)
            ApplySafeArea();
    }

    private void ApplySafeArea()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (Screen.width <= 0 || Screen.height <= 0)
            return;

        Rect safe = Screen.safeArea;
        Vector2 min = safe.position;
        Vector2 max = safe.position + safe.size;

        rectTransform.anchorMin = new Vector2(min.x / Screen.width, min.y / Screen.height);
        rectTransform.anchorMax = new Vector2(max.x / Screen.width, max.y / Screen.height);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        lastSafeArea = safe;
        lastWidth = Screen.width;
        lastHeight = Screen.height;
    }
}
