using System.Collections.Generic;
using UnityEngine;

namespace FabSample.March
{
    /// <summary>
    /// 管理玩家主城占地与中立建筑连线边界。
    /// </summary>
    public sealed class TerritorySystem
    {
        private readonly WorldGrid _worldGrid;
        private readonly Dictionary<int, RectInt> _playerTerritories = new();
        private readonly Dictionary<int, HashSet<int>> _alliances = new();

        public TerritorySystem(WorldGrid worldGrid)
        {
            _worldGrid = worldGrid;
        }

        public void SetAlliance(int playerId, IEnumerable<int> allies)
        {
            _alliances[playerId] = new HashSet<int>(allies) { playerId };
        }

        public void MoveOrResizeCity(int playerId, Vector2Int cityCenter, int cityLevel)
        {
            if (_playerTerritories.TryGetValue(playerId, out var oldRect))
            {
                _worldGrid.RemoveDynamicBlock(oldRect);
            }

            var side = Mathf.Clamp(5 + cityLevel * 2, 5, 31);
            var half = side / 2;
            var rect = new RectInt(cityCenter.x - half, cityCenter.y - half, side, side);
            _playerTerritories[playerId] = rect;
            _worldGrid.AddDynamicBlock(rect);
        }

        public void LinkNeutralBuildings(Vector2Int a, Vector2Int b, int wallThickness = 2)
        {
            var line = RasterizeLine(a, b);
            foreach (var cell in line)
            {
                var r = new RectInt(cell.x - wallThickness / 2, cell.y - wallThickness / 2, wallThickness, wallThickness);
                _worldGrid.AddDynamicBlock(r);
            }
        }

        public bool CanPassTerritory(int teamOwnerPlayerId, int territoryOwnerPlayerId)
        {
            if (teamOwnerPlayerId == territoryOwnerPlayerId)
            {
                return true;
            }

            return _alliances.TryGetValue(territoryOwnerPlayerId, out var allies) && allies.Contains(teamOwnerPlayerId);
        }

        private static IEnumerable<Vector2Int> RasterizeLine(Vector2Int start, Vector2Int end)
        {
            var points = new List<Vector2Int>();
            var dx = Mathf.Abs(end.x - start.x);
            var dy = Mathf.Abs(end.y - start.y);
            var sx = start.x < end.x ? 1 : -1;
            var sy = start.y < end.y ? 1 : -1;
            var err = dx - dy;

            var x = start.x;
            var y = start.y;
            while (true)
            {
                points.Add(new Vector2Int(x, y));
                if (x == end.x && y == end.y)
                {
                    break;
                }

                var e2 = err * 2;
                if (e2 > -dy)
                {
                    err -= dy;
                    x += sx;
                }

                if (e2 < dx)
                {
                    err += dx;
                    y += sy;
                }
            }

            return points;
        }
    }
}
