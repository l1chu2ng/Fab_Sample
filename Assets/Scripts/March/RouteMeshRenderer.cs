using System.Collections.Generic;
using UnityEngine;

namespace FabSample.March
{
    /// <summary>
    /// 使用 Mesh 构建路线带状几何，替代 LineRenderer。
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class RouteMeshRenderer : MonoBehaviour
    {
        [SerializeField] private float width = 0.18f;
        [SerializeField] private float yOffset = 0.04f;

        private Mesh _mesh;

        private void Awake()
        {
            _mesh = new Mesh { name = "RouteMesh" };
            _mesh.MarkDynamic();
            GetComponent<MeshFilter>().sharedMesh = _mesh;
        }

        public void Rebuild(IReadOnlyList<Vector2Int> path)
        {
            _mesh.Clear();
            if (path == null || path.Count < 2)
            {
                return;
            }

            var vertices = new List<Vector3>(path.Count * 2);
            var uvs = new List<Vector2>(path.Count * 2);
            var triangles = new List<int>((path.Count - 1) * 6);

            for (var i = 0; i < path.Count; i++)
            {
                var current = new Vector3(path[i].x, yOffset, path[i].y);
                var tangent = GetTangent(path, i);
                var normal = new Vector3(-tangent.z, 0f, tangent.x) * (width * 0.5f);

                vertices.Add(current - normal);
                vertices.Add(current + normal);

                var t = i / (float)(path.Count - 1);
                uvs.Add(new Vector2(0f, t));
                uvs.Add(new Vector2(1f, t));

                if (i == 0)
                {
                    continue;
                }

                var root = i * 2;
                triangles.Add(root - 2);
                triangles.Add(root - 1);
                triangles.Add(root + 0);

                triangles.Add(root + 0);
                triangles.Add(root - 1);
                triangles.Add(root + 1);
            }

            _mesh.SetVertices(vertices);
            _mesh.SetUVs(0, uvs);
            _mesh.SetTriangles(triangles, 0);
            _mesh.RecalculateBounds();
            _mesh.RecalculateNormals();
        }

        private static Vector3 GetTangent(IReadOnlyList<Vector2Int> path, int index)
        {
            if (index == 0)
            {
                var next = path[1] - path[0];
                return new Vector3(next.x, 0f, next.y).normalized;
            }

            if (index == path.Count - 1)
            {
                var prev = path[index] - path[index - 1];
                return new Vector3(prev.x, 0f, prev.y).normalized;
            }

            var fromPrev = path[index] - path[index - 1];
            var toNext = path[index + 1] - path[index];
            var sum = new Vector2(fromPrev.x + toNext.x, fromPrev.y + toNext.y).normalized;
            return new Vector3(sum.x, 0f, sum.y);
        }
    }
}
