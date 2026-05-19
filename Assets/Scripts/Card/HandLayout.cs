using UnityEngine;

[ExecuteAlways]
public class HandLayout : MonoBehaviour
{
    [SerializeField] private float radius = 600f;          // 부채꼴 반지름 (커질수록 곡률 완만)
    [SerializeField] private float maxArcDegrees = 60f;    // 카드가 많아도 부채꼴이 펼쳐지는 최대 각도
    [SerializeField] private float spacingDegrees = 8f;    // 카드 사이 기본 각도 (총합이 maxArc 이내일 때 사용)
    [SerializeField] private float verticalLift = 0f;      // 가운데 카드가 솟아오르는 정도(0이면 호 그대로)
    [SerializeField] private bool rotateCards = true;      // 카드가 부채꼴 접선 방향으로 회전

    void Start() => Arrange();
    void OnTransformChildrenChanged() => Arrange();
    void OnValidate() => Arrange();

    public void Arrange()
    {
        int n = transform.childCount;
        if (n == 0) return;

        float arc = Mathf.Min((n - 1) * spacingDegrees, maxArcDegrees);
        float step = n > 1 ? arc / (n - 1) : 0f;
        float start = -arc / 2f;
        Vector2 center = new Vector2(0f, -radius);

        for (int i = 0; i < n; i++)
        {
            var child = transform.GetChild(i) as RectTransform;
            if (child == null) continue;

            float angle = start + step * i;
            float rad = angle * Mathf.Deg2Rad;

            Vector2 pos = center + new Vector2(Mathf.Sin(rad), Mathf.Cos(rad)) * radius;
            pos.y += verticalLift * Mathf.Cos(rad);

            child.anchoredPosition = pos;
            child.localRotation = rotateCards ? Quaternion.Euler(0f, 0f, -angle) : Quaternion.identity;
        }
    }
}
