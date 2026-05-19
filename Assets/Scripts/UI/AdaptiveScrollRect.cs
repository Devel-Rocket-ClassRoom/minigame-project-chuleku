using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public class AdaptiveScrollRect : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private int maxVisibleItems = 5;
    [SerializeField] private float manualItemHeight = 50f;
    
    private RectTransform _rectTransform;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        if (scrollRect == null) scrollRect = GetComponent<ScrollRect>();
    }

    private void LateUpdate()
    {
        if (scrollRect == null || scrollRect.content == null) return;

        float contentHeight = scrollRect.content.rect.height;
        
        // Calculate max allowed height based on first child or manual setting
        float itemHeight = manualItemHeight;
        float spacing = 0;
        float paddingTopBottom = 0;

        var layoutGroup = scrollRect.content.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup != null)
        {
            spacing = layoutGroup.spacing;
            paddingTopBottom = layoutGroup.padding.top + layoutGroup.padding.bottom;
            
            if (scrollRect.content.childCount > 0)
            {
                var firstChild = scrollRect.content.GetChild(0) as RectTransform;
                if (firstChild != null)
                {
                    itemHeight = firstChild.rect.height;
                }
            }
        }

        float maxHeight = (itemHeight * maxVisibleItems) + (spacing * Mathf.Max(0, maxVisibleItems - 1)) + paddingTopBottom;
        
        // Final height is the smaller of content height or calculated max height
        float targetHeight = Mathf.Min(contentHeight, maxHeight);
        
        // Only update if change is significant to avoid jitter
        if (!Mathf.Approximately(_rectTransform.sizeDelta.y, targetHeight))
        {
            Vector2 size = _rectTransform.sizeDelta;
            size.y = targetHeight;
            _rectTransform.sizeDelta = size;
        }
    }
}
