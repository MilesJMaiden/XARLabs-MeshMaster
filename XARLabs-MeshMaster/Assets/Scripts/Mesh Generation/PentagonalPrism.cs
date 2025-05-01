using System.Collections.Generic;
using UnityEngine;

#region PentagonalPrism
/// <summary>
/// Generates a pentagonal prism as a child named "ObjectA", with outward-facing normals,
/// rotates the prism to face a target Transform at a configurable angular speed,
/// and interpolates its base material color between frontColor and backColor
/// based on the angle to the target (red when in front, blue when behind).
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

    [Header("Color Settings")]

    [Tooltip("Color when the target is directly in front.")]
    [SerializeField]
    private Color m_FrontColor = Color.red;

    [Tooltip("Color when the target is directly behind.")]
    [SerializeField]
    private Color m_BackColor = Color.blue;

    #endregion

    #region Private Members

    /// <summary>
    /// MeshRenderer on the generated child, used to update the material color.
    /// </summary>
    private MeshRenderer m_meshRenderer;

    /// <summary>
    /// Instance material retrieved from the child’s MeshRenderer.
    /// </summary>
    private Material m_instanceMaterial;

    #endregion

    #region Properties

    protected override string ObjectName => "ObjectA";

    #endregion

    #region Unity Callbacks

    /// <summary>
    /// After the mesh is built by the base class, grab the child’s MeshRenderer
    /// and instantiate its material for color updates.
    /// </summary>
    private void Start()
    {
        Transform child = transform.Find(ObjectName);
        if (child != null)
        {
            m_meshRenderer = child.GetComponent<MeshRenderer>();
            if (m_meshRenderer != null)
                m_instanceMaterial = m_meshRenderer.material;
            else
                Debug.LogWarning($"[{name}] MeshRenderer not found on child '{ObjectName}'.");
        }
        else
        {
            Debug.LogWarning($"[{name}] Child '{ObjectName}' not found for color manipulation.");
        }
    }

    /// <summary>
    /// Each frame, rotates to face the target and updates the base color.
    /// </summary>
    private void Update()
    {
        if (m_TargetTransform == null || m_instanceMaterial == null)
            return;

        Vector3 toTarget = m_TargetTransform.position - transform.position;
        if (toTarget.sqrMagnitude < Mathf.Epsilon)
            return;

        RotateTowardsTarget(toTarget);
        UpdateColorBasedOnAngle(toTarget);
    }

    #endregion

    #region Rotation Logic

    /// <summary>
    /// Smoothly rotates the prism to face the target direction.
    /// </summary>
    /// <param name="toTarget">Vector from prism to target.</param>
    private void RotateTowardsTarget(Vector3 toTarget)
    {
        Quaternion currentRot = transform.rotation;
        Quaternion targetRot = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(
            currentRot,
            targetRot,
            m_AngularSpeed * Time.deltaTime
        );
    }

    #endregion

    #region Color Logic

    /// <summary>
    /// Updates the material color by interpolating between backColor and frontColor
    /// based on the angle between the prism's forward vector and the target direction.
    /// </summary>
    /// <param name="toTarget">Vector from prism to target.</param>
    private void UpdateColorBasedOnAngle(Vector3 toTarget)
    {
        float dot = Vector3.Dot(transform.forward, toTarget.normalized);
        float t = (dot + 1f) * 0.5f;  // maps [-1,1] → [0,1]
        Color c = Color.Lerp(m_BackColor, m_FrontColor, t);
        m_instanceMaterial.color = c;
    }

    #endregion

    #region Mesh Generation

    /// <summary>
    /// Creates:
    /// - A bottom cap (fan) with normals pointing downwards (outward).
    /// - A top cap (fan) with normals pointing upwards (outward).
    /// - Side faces (quads split into two triangles) with outward normals.
    /// </summary>
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