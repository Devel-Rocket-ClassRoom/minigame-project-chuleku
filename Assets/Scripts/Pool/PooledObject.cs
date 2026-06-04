using System.Collections;
using UnityEngine;

// 풀에서 꺼낸 인스턴스에 자동으로 붙는 표식 컴포넌트.
// 어느 프리팹 풀 소속인지 기억하고, 지연 회수 예약과 파티클 리셋을 담당한다.
// (PoolManager가 createFunc에서 AddComponent로 직접 붙이므로 프리팹에 미리 붙일 필요 없음)
[DisallowMultipleComponent]
public class PooledObject : MonoBehaviour
{
    private GameObject sourcePrefab;     // 이 인스턴스가 나온 원본 프리팹(= 풀 키)
    private ParticleSystem[] particles;  // Get 시 리셋해 줄 파티클들
    private Coroutine despawnRoutine;     // 지연 회수 코루틴 핸들
    private bool released;                // 중복 회수 가드
    public GameObject SourcePrefab => sourcePrefab;
    public bool IsReleased => released;

    // createFunc에서 1회 호출. 캐싱은 여기서 한 번만.
    public void Init(GameObject prefab)
    {
        sourcePrefab = prefab;
        particles = GetComponentsInChildren<ParticleSystem>(true);
    }

    // Spawn 직후(활성화된 뒤) 호출: 상태 초기화 + 파티클 재생.
    public void OnSpawned()
    {
        released = false;
        if (despawnRoutine != null) { StopCoroutine(despawnRoutine); despawnRoutine = null; }

        if (particles != null)
        {
            foreach (var ps in particles)
            {
                if (ps == null) continue;
                ps.Clear(true);
                ps.Play(true);
            }
        }
    }

    // 풀로 돌아갈 때 호출. 예약된 지연 회수를 취소하고 released 표시.
    public void MarkReleased()
    {
        released = true;
        if (despawnRoutine != null) { StopCoroutine(despawnRoutine); despawnRoutine = null; }
    }

    // delay초 뒤 자기 자신을 풀로 회수 (기존 Destroy(go, t) 대체).
    public void ScheduleDespawn(float delay)
    {
        if (despawnRoutine != null) StopCoroutine(despawnRoutine);
        despawnRoutine = StartCoroutine(DespawnAfter(delay));
    }

    private IEnumerator DespawnAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        despawnRoutine = null;
        PoolManager.Instance.Despawn(gameObject);
    }
}
