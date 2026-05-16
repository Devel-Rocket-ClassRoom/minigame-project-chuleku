using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveEnemy : MonoBehaviour
{
    private TileMap tilemap;
    public float moveSpeed = 5f;

    private List<Vector2Int> path;
    private Coroutine cor;

    private void OnEnable()
    {
        GameObject gm = GameObject.FindWithTag("TileMap");
        tilemap = gm.GetComponent<TileMap>();
    }

    public void SetPath(List<Vector2Int> newPath)
    {
        if (cor != null) StopCoroutine(cor);
        path = newPath;
        if (path == null || path.Count == 0) return;

        transform.position = tilemap.GridToWorld(path[0]);
        cor = StartCoroutine(CoFollow());
    }

    private IEnumerator CoFollow()
    {
        for (int i = 0; i < path.Count - 1; i++)
        {
            var from = tilemap.GridToWorld(path[i]);
            var to = tilemap.GridToWorld(path[i + 1]);
            float duration = Vector3.Distance(from, to) / moveSpeed;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                transform.position = Vector3.Lerp(from, to, t);
                yield return null;
            }
            transform.position = to;
        }
        cor = null;
    }
}
