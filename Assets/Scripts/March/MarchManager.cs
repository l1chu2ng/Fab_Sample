using System;
using System.Collections.Generic;
using UnityEngine;

namespace FabSample.March
{
    /// <summary>
    /// 队伍调度器：支持最多 300 玩家 * 18 队并行路线。
    /// </summary>
    public sealed class MarchManager : MonoBehaviour
    {
        private const int MaxPlayers = 300;
        private const int MaxTeamsPerPlayer = 18;
        public const int MaxTeamCount = MaxPlayers * MaxTeamsPerPlayer;

        [SerializeField] private RouteMeshRenderer playerRoutePrefab;
        [SerializeField] private RouteMeshRenderer otherRoutePrefab;
        [SerializeField] private Transform routeRoot;

        private readonly Dictionary<long, MarchTeam> _teams = new(MaxTeamCount);
        private readonly Dictionary<long, RouteMeshRenderer> _routeViews = new(MaxTeamCount);

        private WorldGrid _worldGrid;
        private PathService _pathService;
        private TerritorySystem _territorySystem;

        public WorldGrid WorldGrid => _worldGrid;
        public TerritorySystem TerritorySystem => _territorySystem;

        private void Awake()
        {
            _worldGrid = new WorldGrid();
            _pathService = new PathService(_worldGrid);
            _territorySystem = new TerritorySystem(_worldGrid);
        }

        private void Update()
        {
            var dt = Time.deltaTime;
            foreach (var team in _teams.Values)
            {
                TickTeam(team, dt);
            }
        }

        public bool TryDispatchTeam(long teamId, int ownerPlayerId, bool isAlly, Vector2Int start, Vector2Int target, float speed)
        {
            if (_teams.Count >= MaxTeamCount && !_teams.ContainsKey(teamId))
            {
                return false;
            }

            if (!_pathService.TryFindPath(start, target, out var path))
            {
                return false;
            }

            var team = new MarchTeam
            {
                TeamId = teamId,
                OwnerPlayerId = ownerPlayerId,
                IsAlly = isAlly,
                CellsPerSecond = Mathf.Max(0.25f, speed),
                Path = path,
                SegmentIndex = 0,
                SegmentProgress = 0f
            };

            _teams[teamId] = team;
            UpsertRouteView(team);
            return true;
        }

        public bool TryRepath(long teamId, Vector2Int from, Vector2Int target)
        {
            if (!_teams.TryGetValue(teamId, out var team))
            {
                return false;
            }

            if (!_pathService.TryFindPath(from, target, out var newPath))
            {
                return false;
            }

            team.Path = newPath;
            team.SegmentIndex = 0;
            team.SegmentProgress = 0f;
            UpsertRouteView(team);
            return true;
        }

        private void UpsertRouteView(MarchTeam team)
        {
            if (!_routeViews.TryGetValue(team.TeamId, out var view))
            {
                var prefab = team.IsAlly ? playerRoutePrefab : otherRoutePrefab;
                view = Instantiate(prefab, routeRoot != null ? routeRoot : transform);
                _routeViews[team.TeamId] = view;
            }

            view.Rebuild(team.Path);
        }

        private static void TickTeam(MarchTeam team, float deltaTime)
        {
            if (team.IsArrived)
            {
                return;
            }

            var distanceLeft = team.CellsPerSecond * deltaTime;
            while (distanceLeft > 0f && !team.IsArrived)
            {
                var step = 1f - team.SegmentProgress;
                if (distanceLeft >= step)
                {
                    team.SegmentProgress = 0f;
                    team.SegmentIndex++;
                    distanceLeft -= step;
                }
                else
                {
                    team.SegmentProgress += distanceLeft;
                    distanceLeft = 0f;
                }
            }
        }
    }
}
