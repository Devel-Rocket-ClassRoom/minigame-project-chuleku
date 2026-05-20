using UnityEngine;

public class CricleLiner : MonoBehaviour
{
private LineRenderer lineRenderer;

    [Header("원 설정")]
    public int segments = 50; // 원을 구성할 선의 개수 (높을수록 부드러워짐)
    
    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        
        // 라인 렌더러 기본 설정
        lineRenderer.positionCount = segments + 1;
        lineRenderer.loop = true; // 선의 시작과 끝을 연결
        lineRenderer.useWorldSpace = false; // 오브젝트 중심 기준으로 그리기
        
        // 처음에는 안 보이게 숨김
        HideCircle();
    }

    // 마법 버튼을 눌렀을 때 호출할 함수 (중심 위치, 사거리 반직경)
    public void ShowCircle(Vector3 centerPosition, float radius)
    {
        transform.position = centerPosition;
        lineRenderer.enabled = true;

        float angle = 0f;
        for (int i = 0; i <= segments; i++)
        {
            // 삼각함수(Cos, Sin)로 원 위의 점 X, Z 좌표 계산 (3D 탑다운 뷰 기준)
            float x = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
            float z = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;

            // 2D 게임이라면 (x, y, 0f)로 변경하세요!
            lineRenderer.SetPosition(i, new Vector3(x, 1f, z)); 

            angle += (360f / segments);
        }
    }

    public void HideCircle()
    {
        lineRenderer.enabled = false;
    }
}
