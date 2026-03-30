using System;
using System.Collections.Generic;
using UnityEngine;

namespace FabSample.March
{
    /// <summary>
    /// 4096 x 4096 逻辑地图阻挡层。
    /// </summary>
    public sealed class WorldGrid
    {
        public const int MapSize = 4096;

        private readonly bool[] _terrainBlocks = new bool[MapSize * MapSize];
        private readonly int[] _dynamicBlockRefCounts = new int[MapSize * MapSize];

        private static int ToIndex(int x, int y) => y * MapSize + x;

        public bool InBounds(int x, int y)
        {
            return x >= 0 && y >= 0 && x < MapSize && y < MapSize;
        }

        public void SetTerrainBlock(int x, int y, bool blocked)
        {
            if (!InBounds(x, y))
            {
                return;
            }

            _terrainBlocks[ToIndex(x, y)] = blocked;
        }

        public bool IsBlocked(int x, int y)
        {
            if (!InBounds(x, y))
            {
                return true;
            }

            var index = ToIndex(x, y);
            return _terrainBlocks[index] || _dynamicBlockRefCounts[index] > 0;
        }

        public void AddDynamicBlock(RectInt area)
        {
            ApplyDynamicBlock(area, 1);
        }

        public void RemoveDynamicBlock(RectInt area)
        {
            ApplyDynamicBlock(area, -1);
        }

        private void ApplyDynamicBlock(RectInt area, int delta)
        {
            var xMin = Mathf.Max(0, area.xMin);
            var xMax = Mathf.Min(MapSize, area.xMax);
            var yMin = Mathf.Max(0, area.yMin);
            var yMax = Mathf.Min(MapSize, area.yMax);

            for (var y = yMin; y < yMax; y++)
            {
                for (var x = xMin; x < xMax; x++)
                {
                    var index = ToIndex(x, y);
                    _dynamicBlockRefCounts[index] = Mathf.Max(0, _dynamicBlockRefCounts[index] + delta);
                }
            }
        }

        public IEnumerable<Vector2Int> Neighbors4(Vector2Int node)
        {
            var p0 = new Vector2Int(node.x + 1, node.y);
            var p1 = new Vector2Int(node.x - 1, node.y);
            var p2 = new Vector2Int(node.x, node.y + 1);
            var p3 = new Vector2Int(node.x, node.y - 1);

            if (InBounds(p0.x, p0.y)) yield return p0;
            if (InBounds(p1.x, p1.y)) yield return p1;
            if (InBounds(p2.x, p2.y)) yield return p2;
            if (InBounds(p3.x, p3.y)) yield return p3;
        }
    }
}
