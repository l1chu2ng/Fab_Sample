using System;
using System.Collections.Generic;
using UnityEngine;

namespace FabSample.March
{
    [Serializable]
    public sealed class MarchTeam
    {
        public long TeamId;
        public int OwnerPlayerId;
        public bool IsAlly;
        public float CellsPerSecond;
        public List<Vector2Int> Path = new();
        public int SegmentIndex;
        public float SegmentProgress;

        public bool IsArrived => Path == null || Path.Count <= 1 || SegmentIndex >= Path.Count - 1;

        public Vector3 GetWorldPosition(float y = 0f)
        {
            if (Path == null || Path.Count == 0)
            {
                return Vector3.zero;
            }

            if (IsArrived)
            {
                var final = Path[^1];
                return new Vector3(final.x, y, final.y);
            }

            var from = Path[SegmentIndex];
            var to = Path[SegmentIndex + 1];
            var p = Vector2.Lerp(from, to, SegmentProgress);
            return new Vector3(p.x, y, p.y);
        }
    }
}
