using System.Collections.Generic;
using UnityEngine;

#region HexagonalPyramid
/// <summary>
/// Generates a hexagonal pyramid as a child named "ObjectB", with outward-facing normals.
/// </summary>
public class HexagonalPyramid : ProceduralMesh
{
    #region Fields
    [Header("Pyramid Settings")]
    [Tooltip("Radius of the hexagonal base.")]
    [SerializeField]
    private float m_radius = 1f;

    [Tooltip("Height from base plane to apex.")]
    [SerializeField]
    private float m_height = 2f;
    #endregion

    #region Properties
    /// <inheritdoc/>
    protected override string ObjectName => "ObjectB";
    #endregion

    #region Mesh Generation
    /// <inheritdoc/>
    /// <remarks>
    /// Creates:
    /// - A base cap (fan) with normals pointing downwards (outward).
    /// - Side faces (triangles) with normals pointing outward from the apex.
    /// </remarks>
    protected override void BuildMesh(Mesh targetMesh)
    {
        const int sideCount = 6;
        var vertices = new List<Vector3>();
        var triangles = new List<int>();

        // Base ring
        for (int i = 0; i < sideCount; i++)
        {
            float angle = 2f * Mathf.PI * i / sideCount;
            vertices.Add(new Vector3(
                Mathf.Cos(angle) * m_radius,
                0f,
                Mathf.Sin(angle) * m_radius));
        }

        // Apex
        int apexIndex = vertices.Count;
        vertices.Add(new Vector3(0f, m_height, 0f));

        // Base center
        int baseCenterIndex = vertices.Count;
        vertices.Add(Vector3.zero);

        // Base cap (fan, normals outward/down)
        for (int i = 0; i < sideCount; i++)
        {
            triangles.Add(baseCenterIndex);
            triangles.Add(i);
            triangles.Add((i + 1) % sideCount);
        }

        // Side faces (each edge to apex, normals outward)
        for (int i = 0; i < sideCount; i++)
        {
            int next = (i + 1) % sideCount;
            triangles.Add(i);
            triangles.Add(apexIndex);
            triangles.Add(next);
        }

        targetMesh.SetVertices(vertices);
        targetMesh.SetTriangles(triangles, 0);
    }
    #endregion
}
#endregion