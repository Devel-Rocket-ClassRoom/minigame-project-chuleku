using UnityEngine;

// 월드 스페이스 UI(체력바 등)가 항상 카메라를 바라보도록 유지하는 빌보드.
// 몬스터 체력바의 World Space Canvas(또는 바 루트)에 붙인다.
public class Billboard : MonoBehaviour
{
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    // 몬스터 이동/애니메이션이 끝난 뒤 회전을 맞추기 위해 LateUpdate에서 처리.
    void LateUpdate()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        // 카메라의 회전을 그대로 따라가게 해 화면에 항상 정면으로 보이게 한다.
        // (기울어진 탑다운 카메라에서도 바가 화면 기준 똑바로 보임)
        transform.rotation = cam.transform.rotation;
    }
}
