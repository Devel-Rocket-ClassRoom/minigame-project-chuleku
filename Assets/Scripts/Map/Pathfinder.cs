using System.Collections.Generic;
using UnityEngine;

// Vector2Int.y는 그리드의 z축(세로)을 의미한다 — TileMap의 (x,z) 좌표계와 동일
public static class Pathfinder
{
    private static bool missingTrack;
    static readonly Vector2Int[] Dirs =
    {
        new(1, 0), new(-1, 0), new(0, 1), new(0, -1)
    };

    public static List<Vector2Int> FindPath(TileMap map, Vector2Int start, Vector2Int goal)
    {
        if (!map.IsWalkable(start.x, start.y) || !map.IsWalkable(goal.x, goal.y))
            return null;

        var open = new List<Vector2Int> { start };
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, int> { [start] = 0 };
        var fScore = new Dictionary<Vector2Int, int> { [start] = Heuristic(start, goal) };

        while (open.Count > 0)
        {
            int bestIdx = 0;
            for (int i = 1; i < open.Count; i++)
                if (fScore[open[i]] < fScore[open[bestIdx]]) bestIdx = i;

            var current = open[bestIdx];
            if (current == goal) return Reconstruct(cameFrom, current);

            open.RemoveAt(bestIdx);

            foreach (var d in Dirs)
            {
                var nb = current + d;
                if (!map.IsWalkable(nb.x, nb.y)) continue;

                int tentative = gScore[current] + 1;
                if (!gScore.TryGetValue(nb, out int gNb) || tentative < gNb)
                {
                    cameFrom[nb] = current;
                    gScore[nb] = tentative;
                    fScore[nb] = tentative + Heuristic(nb, goal);
                    if (!open.Contains(nb)) open.Add(nb);
                }
            }
        }
        return null;
    }

    // 목표까지 완주하면 전체 경로를, 막혀 있으면 시작점에서 목표에 "가장 가까이"
    // 도달 가능한 칸까지의 부분 경로를 돌려준다. reachedGoal 로 완주 여부를 알려준다.
    // 시작점 자체가 막혀 있으면 null.
    public static List<Vector2Int> FindPathPartial(TileMap map, Vector2Int start, Vector2Int goal, out bool reachedGoal)
    {
        reachedGoal = false;
        if (!map.IsWalkable(start.x, start.y)) return null;

        var open = new List<Vector2Int> { start };
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, int> { [start] = 0 };
        var fScore = new Dictionary<Vector2Int, int> { [start] = Heuristic(start, goal) };

        // 목표에 가장 가까이(휴리스틱 최소) 도달한 칸 — 막혔을 때 부분 경로의 끝점이 된다.
        var best = start;
        int bestH = Heuristic(start, goal);

        while (open.Count > 0)
        {
            int bestIdx = 0;
            for (int i = 1; i < open.Count; i++)
                if (fScore[open[i]] < fScore[open[bestIdx]]) bestIdx = i;

            var current = open[bestIdx];
            if (current == goal) { reachedGoal = true; return Reconstruct(cameFrom, current); }

            int h = Heuristic(current, goal);
            if (h < bestH) { bestH = h; best = current; }

            open.RemoveAt(bestIdx);

            foreach (var d in Dirs)
            {
                var nb = current + d;
                if (!map.IsWalkable(nb.x, nb.y)) continue;

                int tentative = gScore[current] + 1;
                if (!gScore.TryGetValue(nb, out int gNb) || tentative < gNb)
                {
                    cameFrom[nb] = current;
                    gScore[nb] = tentative;
                    fScore[nb] = tentative + Heuristic(nb, goal);
                    if (!open.Contains(nb)) open.Add(nb);
                }
            }
        }

        // 목표 도달 실패 → 가장 가까이 간 칸까지의 부분 경로
        return Reconstruct(cameFrom, best);
    }

    static int Heuristic(Vector2Int a, Vector2Int b)
        => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

    static List<Vector2Int> Reconstruct(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        var path = new List<Vector2Int> { current };
        while (cameFrom.TryGetValue(current, out var prev))
        {
            current = prev;
            path.Add(current);
        }
        path.Reverse();
        return path;
    }
}
