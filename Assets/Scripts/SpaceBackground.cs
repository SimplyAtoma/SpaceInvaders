using UnityEngine;

/// <summary>
/// Attach to a Quad GameObject.
/// Automatically sizes the quad to fill the orthographic camera
/// and pushes it to the far background.
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
public class SpaceBackground : MonoBehaviour
{
    [Header("Auto-fit")]
    [Tooltip("How far behind the scene to place the background quad")]
    [SerializeField] private float zDepth = 10f;

    [Header("Shader Overrides (optional — also editable on the Material)")]
    [SerializeField] private float scrollSpeed1 = 0.04f;
    [SerializeField] private float scrollSpeed2 = 0.09f;
    [SerializeField] private float scrollSpeed3 = 0.18f;
    [SerializeField] [Range(0,1)] private float nebulaStrength = 0.55f;
    [SerializeField] private Color nebulaColorA = new Color(0.08f, 0.0f, 0.18f);
    [SerializeField] private Color nebulaColorB = new Color(0.0f,  0.05f, 0.2f);

    private Material mat;
    private Camera   cam;

    private void Awake()
    {
        cam = Camera.main;
        mat = GetComponent<MeshRenderer>().material;
    }

    private void Start()
    {
        FitToCamera();
        PushToBackground();
        ApplyShaderProperties();
    }

    private void FitToCamera()
    {
        if (cam == null || !cam.orthographic) return;

        float h = cam.orthographicSize * 2f;
        float w = h * cam.aspect;
        transform.localScale = new Vector3(w, h, 1f);
    }

    private void PushToBackground()
    {
        Vector3 pos = cam != null ? cam.transform.position : Vector3.zero;
        pos.z = zDepth;
        transform.position = pos;
    }

    private void ApplyShaderProperties()
    {
        if (mat == null) return;
        mat.SetFloat("_ScrollSpeed1",   scrollSpeed1);
        mat.SetFloat("_ScrollSpeed2",   scrollSpeed2);
        mat.SetFloat("_ScrollSpeed3",   scrollSpeed3);
        mat.SetFloat("_NebulaStrength", nebulaStrength);
        mat.SetColor("_NebulaColor1",   nebulaColorA);
        mat.SetColor("_NebulaColor2",   nebulaColorB);
    }

#if UNITY_EDITOR
    // Re-fit if camera is resized in editor
    private void OnValidate()
    {
        if (!Application.isPlaying) return;
        FitToCamera();
        ApplyShaderProperties();
    }
#endif
}
