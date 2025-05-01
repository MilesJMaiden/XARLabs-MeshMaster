using System.Collections.Generic;
using UnityEngine;

#region PentagonalPrism
/// <summary>
/// Generates a pentagonal prism as a child named "ObjectA", with outward-facing normals,
/// and rotates the prism to face a target Transform at a configurable angular speed.
/// </summary>
public class PentagonalPrism : ProceduralMesh
{
    #region Fields

    [Header("Prism Settings")]

    [Tooltip("Radius of the pentagon base.")]
    [SerializeField]
    private float m_Radius = 1f;

    [Tooltip("Total height of the prism.")]
    [SerializeField]
    private float m_Height = 2f;

    [Header("Rotation Settings")]

    [Tooltip("Transform of the target to face.")]
    [SerializeField]
    private Transform m_TargetTransform;

    [Tooltip("Angular speed (in degrees per second) at which to rotate towards the target.")]
    [SerializeField]
    private float m_AngularSpeed = 90f;

    #endregion

    #region Properties

    /// <inheritdoc/>
    protected override string ObjectName => "ObjectA";

    #endregion

    #region Unity Callbacks

    /// <summary>
    /// Every frame, rotates the prism to look at the assigned target transform.
    /// </summary>
    private void Update()
    {
        if (m_TargetTransform == null)
            return;

        Vector3 directionToTarget = m_TargetTransform.position - transform.position;
        if (directionToTarget.sqrMagnitude < Mathf.Epsilon)
            return;

        Quaternion currentRot = transform.rotation;
        Quaternion targetRot = Quaternion.LookRotation(directionToTarget.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(
            currentRot,
            targetRot,
            m_AngularSpeed * Time.deltaTime
        );
    }

    #endregion

    #region Mesh Generation

    /// <inheritdoc/>
    /// <remarks>
    /// Creates:
    /// - A bottom cap (fan) with normals pointing downwards (outward).
    /// - A top cap (fan) with normals pointing upwards (outward).
    /// - Side faces (quads split into two triangles) with outward normals.
    /// </remarks>
    protected override void BuildMesh(Mesh targetMesh)
    {
        const int sideCount = 5;
        float halfHeight = m_Height * 0.5f;

        var vertices = new List<Vector3>();
        var triangles = new List<int>();

        // Bottom ring
        for (int i = 0; i < sideCount; i++)
        {
            float angle = 2f * Mathf.PI * i / sideCount;
            vertices.Add(new Vector3(
                Mathf.Cos(angle) * m_Radius,
               -halfHeight,
                Mathf.Sin(angle) * m_Radius));
        }

        // Top ring
        for (int i = 0; i < sideCount; i++)
        {
            float angle = 2f * Mathf.PI * i / sideCount;
            vertices.Add(new Vector3(
                Mathf.Cos(angle) * m_Radius,
                 halfHeight,
                Mathf.Sin(angle) * m_Radius));
        }

        // Center points for caps
        int bottomCenterIndex = vertices.Count;
        vertices.Add(new Vector3(0f, -halfHeight, 0f));

        int topCenterIndex = vertices.Count;
        vertices.Add(new Vector3(0f, halfHeight, 0f));

        // Bottom cap (fan, normals outward/down)
        for (int i = 0; i < sideCount; i++)
        {
            triangles.Add(bottomCenterIndex);
            triangles.Add(i);
            triangles.Add((i + 1) % sideCount);
        }

        // Top cap (fan, normals outward/up)
        for (int i = 0; i < sideCount; i++)
        {
            int thisTop = sideCount + i;
            int nextTop = sideCount + ((i + 1) % sideCount);

            triangles.Add(topCenterIndex);
            triangles.Add(nextTop);
            triangles.Add(thisTop);
        }

        // Sides (each quad = two tris, normals outward)
        for (int i = 0; i < sideCount; i++)
        {
            int next = (i + 1) % sideCount;
            int bottomA = i;
            int bottomB = next;
            int topA = sideCount + i;
            int topB = sideCount + next;

            // First triangle
            triangles.Add(bottomA);
            triangles.Add(topA);
            triangles.Add(topB);

            // Second triangle
            triangles.Add(bottomA);
            triangles.Add(topB);
            triangles.Add(bottomB);
        }

        targetMesh.SetVertices(vertices);
        targetMesh.SetTriangles(triangles, 0);
    }

    #endregion
}
#endregion