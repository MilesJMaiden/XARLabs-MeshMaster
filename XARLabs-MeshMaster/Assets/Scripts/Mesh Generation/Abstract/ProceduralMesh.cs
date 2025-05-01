using UnityEngine;

#region ProceduralMesh
/// <summary>
/// Base class for generating a child GameObject with a procedural mesh.
/// </summary>
public abstract class ProceduralMesh : MonoBehaviour
{
    #region Fields
    [Tooltip("Material used for the generated mesh.")]
    [SerializeField]
    private Material m_material;

    #endregion

    #region Properties
    /// <summary>
    /// Name to assign to the generated child GameObject.
    /// </summary>
    protected abstract string ObjectName { get; }

    #endregion

    #region Unity Callbacks
    /// <summary>
    /// Creates child, adds MeshFilter/MeshRenderer, and invokes mesh build.
    /// </summary>
    private void Awake()
    {
        if (m_material == null)
        {
            Debug.LogError($"[{name}] ProceduralMesh requires a material.");
            return;
        }

        GameObject meshObject = new GameObject(ObjectName);
        meshObject.transform.SetParent(transform, worldPositionStays: false);

        var meshFilter = meshObject.AddComponent<MeshFilter>();
        var meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshRenderer.material = m_material;

        var mesh = new Mesh { name = $"{ObjectName}Mesh" };
        BuildMesh(mesh);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;
    }
    #endregion

    #region Mesh Generation
    /// <summary>
    /// Subclasses override to populate <paramref name="targetMesh"/> with vertices & triangles.
    /// </summary>
    /// <param name="targetMesh">Mesh instance to receive geometry.</param>
    protected abstract void BuildMesh(Mesh targetMesh);
    #endregion
}
#endregion
