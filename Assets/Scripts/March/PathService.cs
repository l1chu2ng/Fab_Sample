using System.Collections.Generic;
using UnityEngine;

namespace FabSample.March
{
    /// <summary>
    /// A* 路径搜索（4方向），无 NavMesh。
    /// </summary>
    public sealed class PathService
    {
        private readonly WorldGrid _worldGrid;

        public PathService(WorldGrid worldGrid)
        {
            _worldGrid = worldGrid;
        }

        public bool TryFindPath(
            Vector2Int start,
            Vector2Int end,
            out List<Vector2Int> path,
            int maxExpandedNodes = 90000)
        {
            path = new List<Vector2Int>();
            if (!_worldGrid.InBounds(start.x, start.y) || !_worldGrid.InBounds(end.x, end.y))
            {
                return false;
            }

            if (start == end)
            {
                path.Add(start);
                return true;
            }

            var open = new PriorityQueue<Vector2Int, int>();
            var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            var gScore = new Dictionary<Vector2Int, int> { [start] = 0 };
            var closed = new HashSet<Vector2Int>();

            open.Enqueue(start, Heuristic(start, end));

            var expanded = 0;
            while (open.Count > 0)
            {
                var current = open.Dequeue();
                if (current == end)
                {
                    RebuildPath(cameFrom, current, path);
                    return true;
                }

                if (!closed.Add(current))
                {
                    continue;
                }

                expanded++;
                if (expanded >= maxExpandedNodes)
                {
                    return false;
                }

                foreach (var next in _worldGrid.Neighbors4(current))
                {
                    if (closed.Contains(next) || _worldGrid.IsBlocked(next.x, next.y))
                    {
                        continue;
                    }

                    var tentativeG = gScore[current] + 1;
                    if (!gScore.TryGetValue(next, out var oldScore) || tentativeG < oldScore)
                    {
                        cameFrom[next] = current;
                        gScore[next] = tentativeG;
                        var priority = tentativeG + Heuristic(next, end);
                        open.Enqueue(next, priority);
                    }
                }
            }

            return false;
        }

        private static int Heuristic(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        private static void RebuildPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int end, List<Vector2Int> result)
        {
            result.Clear();
            result.Add(end);

            var cursor = end;
            while (cameFrom.TryGetValue(cursor, out var previous))
            {
                result.Add(previous);
                cursor = previous;
            }

            result.Reverse();
        }
    }
}
