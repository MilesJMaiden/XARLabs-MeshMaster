using UnityEngine;

#region LissajousCurveMover
/// <summary>
/// Moves the GameObject’s position along a Lissajous curve in the XY-plane,
/// and optionally visualizes the curve via a LineRenderer.
/// Automatically updates the visualization if parameters change at runtime,
/// and blends toward an attractor when it's within range.
/// </summary>
public class LissajousCurveMover : MonoBehaviour
{
    #region Fields

    [Header("Lissajous Curve Parameters")]

    [Tooltip("Amplitude along the X axis (A).")]
    [SerializeField]
    private float m_AmplitudeX = 4f;

    [Tooltip("Amplitude along the Y axis (B).")]
    [SerializeField]
    private float m_AmplitudeY = 3f;

    [Tooltip("Frequency multiplier for X (a).")]
    [SerializeField]
    private float m_FrequencyX = 2f;

    [Tooltip("Frequency multiplier for Y (b).")]
    [SerializeField]
    private float m_FrequencyY = 4f;

    [Tooltip("Phase offset for X (δ), in radians.")]
    [SerializeField]
    private float m_PhaseOffset = 3f;

    [Tooltip("Speed multiplier for movement along the curve.")]
    [SerializeField]
    private float m_Speed = 1f;

    [Header("Curve Visualization")]

    [Tooltip("Enable to draw the Lissajous curve with a LineRenderer.")]
    [SerializeField]
    private bool m_ShowCurve = false;

    [Tooltip("Number of segments used to approximate the curve.")]
    [SerializeField]
    private int m_CurveResolution = 100;

    [Tooltip("Width of the rendered line.")]
    [SerializeField]
    private float m_LineWidth = 0.05f;

    [Tooltip("Material used by the LineRenderer to draw the curve.")]
    [SerializeField]
    private Material m_LineMaterial;

    [Header("Attraction Settings")]

    [Tooltip("Enable attraction blending logic.")]
    [SerializeField]
    private bool m_EnableAttraction = true;

    [Tooltip("Transform of the attractor (e.g., XR controller).")]
    [SerializeField]
    private Transform m_AttractorTransform;

    [Tooltip("Distance threshold within which attraction is applied.")]
    [SerializeField]
    private float m_AttractionRange = 1f;

    [Tooltip("How strongly the object is pulled toward the attractor (0–1).")]
    [SerializeField]
    [Range(0f, 1f)]
    private float m_AttractionStrength = 1f;

    #endregion

    #region Private Members

    /// <summary>
    /// Stored local position at start, used as origin for motion.
    /// </summary>
    private Vector3 m_InitialLocalPosition;

    /// <summary>
    /// Stored world position at start, used as origin for the drawn curve.
    /// </summary>
    private Vector3 m_InitialWorldPosition;

    /// <summary>
    /// Reference to the LineRenderer used to draw the curve.
    /// </summary>
    private LineRenderer m_LineRenderer;

    /// <summary>
    /// Cached amplitude along the X axis (A) from the last frame, for detecting changes.
    /// </summary>
    private float m_LastAmplitudeX;

    /// <summary>
    /// Cached amplitude along the Y axis (B) from the last frame, for detecting changes.
    /// </summary>
    private float m_LastAmplitudeY;

    /// <summary>
    /// Cached frequency multiplier for X (a) from the last frame, for detecting changes.
    /// </summary>
    private float m_LastFrequencyX;

    /// <summary>
    /// Cached frequency multiplier for Y (b) from the last frame, for detecting changes.
    /// </summary>
    private float m_LastFrequencyY;

    /// <summary>
    /// Cached phase offset (δ) from the last frame, for detecting changes.
    /// </summary>
    private float m_LastPhaseOffset;

    /// <summary>
    /// Cached line width of the LineRenderer from the last frame, for detecting changes.
    /// </summary>
    private float m_LastLineWidth;

    /// <summary>
    /// Cached curve resolution from the last frame, for detecting changes.
    /// </summary>
    private int m_LastCurveResolution;

    /// <summary>
    /// Cached flag indicating whether the curve was shown in the last frame, for detecting changes.
    /// </summary>
    private bool m_LastShowCurve;

    /// <summary>
    /// Cached material used by the LineRenderer from the last frame, for detecting changes.
    /// </summary>
    private Material m_LastLineMaterial;

    #endregion

    #region Unity Callbacks

    /// <summary>
    /// Captures initial positions, ensures a LineRenderer exists, and builds the curve.
    /// </summary>
    private void Awake()
    {
        m_InitialLocalPosition = transform.localPosition;
        m_InitialWorldPosition = transform.position;

        SetupLineRenderer();
        CacheParameters();
        GenerateCurve();
        m_LineRenderer.enabled = m_ShowCurve;
    }

    /// <summary>
    /// Moves along the Lissajous path each frame, blends toward attractor if in range,
    /// toggles curve visibility, and regenerates the curve if any parameter has changed.
    /// </summary>
    private void Update()
    {
        Vector3 baseLocal = ComputeBaseCurveLocalPosition();
        baseLocal = BlendAttraction(baseLocal);
        transform.localPosition = baseLocal;
        UpdateVisualization();
    }

    #endregion

    #region Movement Logic

    /// <summary>
    /// Computes the base Lissajous position in local space.
    /// </summary>
    /// <returns>Local position along the curve.</returns>
    private Vector3 ComputeBaseCurveLocalPosition()
    {
        float t = Time.time * m_Speed;
        float x = m_AmplitudeX * Mathf.Sin(m_FrequencyX * t + m_PhaseOffset);
        float y = m_AmplitudeY * Mathf.Sin(m_FrequencyY * t);
        return m_InitialLocalPosition + new Vector3(x, y, 0f);
    }

    #endregion

    #region Attraction Blend

    /// <summary>
    /// Blends the base position toward the attractor if within range.
    /// </summary>
    /// <param name="baseLocal">Original curve position.</param>
    /// <returns>Blended local position.</returns>
    private Vector3 BlendAttraction(Vector3 baseLocal)
    {
        if (!m_EnableAttraction || m_AttractorTransform == null) { return baseLocal; }

        float dist = Vector3.Distance(transform.position, m_AttractorTransform.position);
        if (dist > m_AttractionRange)
        {
            return baseLocal;
        }

        float weight = 1f - (dist / m_AttractionRange);
        weight = weight * m_AttractionStrength;

        Vector3 attractorLocal;
        if (transform.parent != null)
        {
            attractorLocal = transform.parent.InverseTransformPoint(
                m_AttractorTransform.position
            );
        }
        else
        {
            attractorLocal = m_AttractorTransform.position;
        }

        return Vector3.Lerp(
            baseLocal,
            attractorLocal,
            weight
        );
    }

    #endregion

    #region Visualization & Regeneration

    /// <summary>
    /// Toggles the LineRenderer and regenerates the curve if parameters changed.
    /// </summary>
    private void UpdateVisualization()
    {
        if (m_LineRenderer.enabled != m_ShowCurve)
        {
            m_LineRenderer.enabled = m_ShowCurve;
        }

        if (ParametersChanged())
        {
            UpdateLineRendererSettings();
            GenerateCurve();
            CacheParameters();
        }
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Adds/configures a LineRenderer for drawing the curve.
    /// </summary>
    private void SetupLineRenderer()
    {
        m_LineRenderer = GetComponent<LineRenderer>();
        if (m_LineRenderer == null)
        {
            m_LineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        m_LineRenderer.loop = false;
        m_LineRenderer.useWorldSpace = true;
        m_LineRenderer.widthMultiplier = m_LineWidth;

        if (m_LineMaterial != null) 
        {
            m_LineRenderer.material = m_LineMaterial;
        }

    }

    #endregion

    #region Curve Generation

    /// <summary>
    /// Samples the Lissajous formula and assigns those points to the LineRenderer.
    /// </summary>
    private void GenerateCurve()
    {
        int resolution = Mathf.Max(2, m_CurveResolution);

        // time to complete one full sin(a t) cycle = 2π/a
        float periodX = 2f * Mathf.PI / m_FrequencyX;
        float periodY = 2f * Mathf.PI / m_FrequencyY;

        // pick whichever is longer so you cover both
        float samplePeriod = Mathf.Max(periodX, periodY);

        var points = new Vector3[resolution + 1];
        for (int i = 0; i <= resolution; i++)
        {
            float t = samplePeriod * i / resolution;
            float x = m_AmplitudeX * Mathf.Sin(m_FrequencyX * t + m_PhaseOffset);
            float y = m_AmplitudeY * Mathf.Sin(m_FrequencyY * t);
            points[i] = m_InitialWorldPosition + new Vector3(x, y, 0f);
        }

        m_LineRenderer.positionCount = points.Length;
        m_LineRenderer.SetPositions(points);
    }

    #endregion

    #region Parameter Tracking

    /// <summary>
    /// Caches current inspector parameters for change detection.
    /// </summary>
    private void CacheParameters()
    {
        m_LastAmplitudeX = m_AmplitudeX;
        m_LastAmplitudeY = m_AmplitudeY;
        m_LastFrequencyX = m_FrequencyX;
        m_LastFrequencyY = m_FrequencyY;
        m_LastPhaseOffset = m_PhaseOffset;
        m_LastCurveResolution = m_CurveResolution;
        m_LastLineWidth = m_LineWidth;
        m_LastShowCurve = m_ShowCurve;
        m_LastLineMaterial = m_LineMaterial;
    }

    /// <summary>
    /// Returns true if any of the curve or visualization parameters have changed.
    /// </summary>
    private bool ParametersChanged()
    {
        return !Mathf.Approximately(m_LastAmplitudeX, m_AmplitudeX)
            || !Mathf.Approximately(m_LastAmplitudeY, m_AmplitudeY)
            || !Mathf.Approximately(m_LastFrequencyX, m_FrequencyX)
            || !Mathf.Approximately(m_LastFrequencyY, m_FrequencyY)
            || !Mathf.Approximately(m_LastPhaseOffset, m_PhaseOffset)
            || m_LastCurveResolution != m_CurveResolution
            || !Mathf.Approximately(m_LastLineWidth, m_LineWidth)
            || m_LastShowCurve != m_ShowCurve
            || m_LastLineMaterial != m_LineMaterial;
    }

    /// <summary>
    /// Applies any changed visualization settings (resolution, width, material) to the LineRenderer.
    /// </summary>
    private void UpdateLineRendererSettings()
    {
        m_LineRenderer.widthMultiplier = m_LineWidth;
        if (m_LineMaterial != null && m_LineRenderer.material != m_LineMaterial)
        {
            m_LineRenderer.material = m_LineMaterial;
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Resets both motion and curve origins to the current transform,
    /// then rebuilds the visualization. Call this if you manually move
    /// the object in editor or change parameters at runtime.
    /// </summary>
    public void ResetAndGenerate()
    {
        m_InitialLocalPosition = transform.localPosition;
        m_InitialWorldPosition = transform.position;
        GenerateCurve();
        CacheParameters();
    }

    #endregion
}
#endregion