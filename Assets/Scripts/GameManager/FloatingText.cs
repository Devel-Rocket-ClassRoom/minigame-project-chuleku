using UnityEngine;
using TMPro;
using DG.Tweening;
public enum FloatingTextType
{
    Gain,
    Loss,
}
public class FloatingText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float duration = 0.8f;
    [SerializeField] private float moveUp = 80f;
    public void Show(string content, Color color)
    {
        text.text = content;
        text.color = color;
        canvasGroup.alpha = 1f;
        transform.localScale = Vector3.zero;

        var seq = DOTween.Sequence();
        seq.Append(transform.DOScale(1.2f, 0.15f).SetEase(Ease.OutBack));
        seq.Append(transform.DOScale(1f, 0.1f));
        seq.Append(transform.DOLocalMoveY(transform.localPosition.y + moveUp, duration)
                            .SetRelative(false).SetEase(Ease.OutCubic));
        seq.Join(canvasGroup.DOFade(0f, duration).SetEase(Ease.InQuad));
        seq.OnComplete(() => Destroy(gameObject));
    }
    public void ShowLoss(string content, Color color)
    {
        text.text = content;
        text.color = color;
        canvasGroup.alpha = 1f;
        transform.localScale = Vector3.one * 1.3f; // 처음부터 살짝 크게

        var seq = DOTween.Sequence();
        // 들어오면서 줄어드는 느낌
        seq.Append(transform.DOScale(1f, 0.15f).SetEase(Ease.OutQuad));
        // 아래로 떨어지며 페이드
        seq.Append(transform.DOLocalMoveY(transform.localPosition.y - 50f, 0.5f)
                            .SetEase(Ease.InCubic)); // 중력감
        seq.Join(canvasGroup.DOFade(0f, 0.5f));
        seq.OnComplete(() => Destroy(gameObject));
    }
}
