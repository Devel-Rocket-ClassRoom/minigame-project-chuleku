using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PathPreview : MonoBehaviour
{
    [SerializeField] private float drawSpeed = 70f;
    [SerializeField] private float holdDuration = 3f;
    [SerializeField] private float heightOffset = 2f;

    private LineRenderer line;
    private Coroutine cor;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = 0;
        line.useWorldSpace = true;
    }

    public void Show(TileMap map, List<Vector2Int> path)
    {
        if (cor != null) StopCoroutine(cor);
        line.positionCount = 0;
        if (map == null || path == null || path.Count < 2) return;
        cor = StartCoroutine(CoDraw(map, path));
    }

    public void Hide()
    {
        if (cor != null) StopCoroutine(cor);
        cor = null;
        line.positionCount = 0;
    }

    private IEnumerator CoDraw(TileMap map, List<Vector2Int> path)
    {
        var points = new List<Vector3>(path.Count);
        float baseY = map.Origin.y + heightOffset;
        foreach (var g in path)
        {
            var w = map.GridToWorld(g);
            w.y = baseY;
            points.Add(w);
        }

        line.positionCount = 2;
        line.SetPosition(0, points[0]);
        line.SetPosition(1, points[0]);

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 from = points[i];
            Vector3 to = points[i + 1];
            float segLen = Vector3.Distance(from, to);
            float traveled = 0f;

            while (traveled < segLen)
            {
                traveled += drawSpeed * Time.deltaTime;
                float t = Mathf.Clamp01(traveled / segLen);
                Vector3 head = Vector3.Lerp(from, to, t);
                line.SetPosition(line.positionCount - 1, head);
                yield return null;
            }

            line.SetPosition(line.positionCount - 1, to);
            if (i < points.Count - 2)
            {
                line.positionCount += 1;
                line.SetPosition(line.positionCount - 1, to);
            }
        }

        yield return new WaitForSeconds(holdDuration);
        line.positionCount = 0;
        cor = null;
    }
}
