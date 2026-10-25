using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Add to the imported camera rig root. Sweeps the pan pivot, draws a live field-of-view
/// cone, and reports colliders inside the selected layer mask and view angle.
/// </summary>
[System.Serializable] public class CameraTargetEvent : UnityEvent<GameObject> { }

public class SecurityCameraController : MonoBehaviour
{
    [Header("Rig")]
    public Transform panPivot;
    public Transform sensorOrigin;

    [Header("Pan")]
    [Range(0f, 85f)] public float panLimitDegrees = 55f;
    [Min(1f)] public float sweepSpeedDegreesPerSecond = 30f;

    [Header("Detection")]
    [Range(1f, 179f)] public float fieldOfViewDegrees = 70f;
    [Min(0.1f)] public float detectionRange = 8f;
    public LayerMask detectionLayers = ~0;
    [Tooltip("Layers that can block the camera view. Include walls and solid level geometry.")]
    public LayerMask obstructionLayers = ~0;
    public string playerTag = "Player";
    public AudioSource alarmAudioSource;
    public AudioClip alarmClip;
    [Min(0.02f)] public float scanInterval = 0.15f;
    public CameraTargetEvent onDetected;
    public CameraTargetEvent onLost;

    [Header("Cone transform (local to Sensor Origin)")]
    public Vector3 coneLocalPosition = Vector3.zero;
    public Vector3 coneLocalRotation = Vector3.zero;
    public Vector3 coneLocalScale = Vector3.one;

    [Header("Cone display")]
    public bool showCone = true;
    [Range(4, 64)] public int coneSegments = 24;
    public Color coneColor = new Color(1f, 0f, 0f, 0.12f);
    public Material coneMaterial;

    private float baseYaw;
    private float nextScan;
    private readonly HashSet<GameObject> currentlySeen = new HashSet<GameObject>();
    private readonly List<GameObject> detectedNow = new List<GameObject>();
    private Transform coneTransform;
    private MaterialPropertyBlock coneProperties;
    private MeshFilter coneFilter;
    private MeshRenderer coneRenderer;
    private Material generatedMaterial;
    private Collider[] overlapBuffer = new Collider[128];

    void Awake()
    {
        if (panPivot == null) panPivot = FindChild("SCAM_PanPivot");
        if (sensorOrigin == null) sensorOrigin = FindChild("SCAM_SensorOrigin");
        if (panPivot == null) panPivot = transform;
        if (sensorOrigin == null) sensorOrigin = panPivot;
        if (alarmAudioSource == null) alarmAudioSource = GetComponent<AudioSource>();
        if (alarmAudioSource == null) alarmAudioSource = gameObject.AddComponent<AudioSource>();
        alarmAudioSource.playOnAwake = false;
        alarmAudioSource.spatialBlend = 0f;
        baseYaw = panPivot.localEulerAngles.y;
        BuildCone();
    }

    Transform FindChild(string exactName)
    {
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
            if (child.name == exactName) return child;
        return null;
    }

    void Update()
    {
        if (panPivot == null || sensorOrigin == null) return;
        float phase = Mathf.PingPong(Time.time * sweepSpeedDegreesPerSecond, panLimitDegrees * 2f) - panLimitDegrees;
        Vector3 angles = panPivot.localEulerAngles;
        angles.y = baseYaw + phase;
        panPivot.localEulerAngles = angles;

        if (coneRenderer != null) coneRenderer.enabled = showCone;
        if (Time.time >= nextScan)
        {
            nextScan = Time.time + scanInterval;
            ScanForTargets();
        }
    }

    void ScanForTargets()
    {
        int count = Physics.OverlapSphereNonAlloc(sensorOrigin.position, detectionRange, overlapBuffer, detectionLayers, QueryTriggerInteraction.Collide);
        detectedNow.Clear();
        Vector3 forward = sensorOrigin.forward;
        float minDot = Mathf.Cos(fieldOfViewDegrees * 0.5f * Mathf.Deg2Rad);
        for (int i = 0; i < count; i++)
        {
            Collider hit = overlapBuffer[i];
            if (hit == null || hit.transform.IsChildOf(transform)) continue;
            Vector3 offset = hit.bounds.center - sensorOrigin.position;
            if (offset.sqrMagnitude < 0.0001f || Vector3.Dot(forward, offset.normalized) < minDot) continue;
            GameObject target = hit.attachedRigidbody != null ? hit.attachedRigidbody.gameObject : hit.gameObject;
            if (!HasLineOfSight(target, hit.bounds.center)) continue;
            if (!detectedNow.Contains(target)) detectedNow.Add(target);
        }
        foreach (GameObject target in detectedNow)
        {
            if (!currentlySeen.Add(target)) continue;
            if (onDetected != null) onDetected.Invoke(target);
            if (target.CompareTag(playerTag))
            {
                Guard nearestGuard = Guard.FindClosestGuard(target.transform.position);
                if (nearestGuard != null)
                {
                    nearestGuard.TriggerLaserAlarm(target.transform.position);
                    Debug.Log($"[{gameObject.name}] Alerted nearest guard '{nearestGuard.gameObject.name}' to investigate the detected player.");
                }
                else
                {
                    Debug.LogWarning($"[{gameObject.name}] Camera detected the player, but there are no active guards to investigate.");
                }

                if (alarmClip != null && alarmAudioSource != null)
                    alarmAudioSource.PlayOneShot(alarmClip);
            }
        }
        currentlySeen.RemoveWhere(target =>
        {
            if (target != null && detectedNow.Contains(target)) return false;
            if (target != null && onLost != null) onLost.Invoke(target);
            return true;
        });
    }

    bool HasLineOfSight(GameObject target, Vector3 targetPoint)
    {
        Vector3 toTarget = targetPoint - sensorOrigin.position;
        float distance = toTarget.magnitude;
        if (distance <= 0.001f) return true;
        RaycastHit[] hits = Physics.RaycastAll(sensorOrigin.position, toTarget / distance, distance, obstructionLayers, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (RaycastHit hit in hits)
        {
            Transform hitTransform = hit.transform;
            if (hitTransform == transform || hitTransform.IsChildOf(transform)) continue;
            return hitTransform.gameObject == target || hitTransform.IsChildOf(target.transform);
        }
        return true;
    }

    void BuildCone()
    {
        GameObject cone = new GameObject("Detection Cone");
        cone.transform.SetParent(sensorOrigin, false);
        coneTransform = cone.transform;
        ApplyConeTransform();
        coneFilter = cone.AddComponent<MeshFilter>();
        coneRenderer = cone.AddComponent<MeshRenderer>();
        coneRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        coneRenderer.receiveShadows = false;
        coneRenderer.sharedMaterial = coneMaterial != null ? coneMaterial : CreateTransparentMaterial();
        coneFilter.sharedMesh = MakeConeMesh();
        ApplyConeColor();
    }

    Material CreateTransparentMaterial()
    {
        bool usingScriptableRenderPipeline = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null;
        Shader shader = usingScriptableRenderPipeline
            ? Shader.Find("Universal Render Pipeline/Unlit")
            : Shader.Find("Unlit/Color");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        if (shader == null) shader = Shader.Find("Standard");
        generatedMaterial = new Material(shader);
        generatedMaterial.color = coneColor;
        if (generatedMaterial.HasProperty("_BaseColor")) generatedMaterial.SetColor("_BaseColor", coneColor);
        if (generatedMaterial.HasProperty("_Color")) generatedMaterial.SetColor("_Color", coneColor);
        if (generatedMaterial.HasProperty("_Surface")) generatedMaterial.SetFloat("_Surface", 1f);
        if (generatedMaterial.HasProperty("_Mode")) generatedMaterial.SetFloat("_Mode", 3f);
        if (generatedMaterial.HasProperty("_SrcBlend")) generatedMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        if (generatedMaterial.HasProperty("_DstBlend")) generatedMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        if (generatedMaterial.HasProperty("_ZWrite")) generatedMaterial.SetInt("_ZWrite", 0);
        generatedMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        generatedMaterial.EnableKeyword("_ALPHABLEND_ON");
        generatedMaterial.renderQueue = 3000;
        return generatedMaterial;
    }
    Mesh MakeConeMesh()
    {
        int segments = Mathf.Max(4, coneSegments);
        float halfAngle = fieldOfViewDegrees * 0.5f * Mathf.Deg2Rad;
        float radius = detectionRange * Mathf.Tan(halfAngle);
        // A closed 3D cone: apex, circular rim, and base cap, aligned with local +Z.
        Vector3[] vertices = new Vector3[segments + 2];
        int[] triangles = new int[segments * 12];
        vertices[0] = Vector3.zero;
        for (int i = 0; i < segments; i++)
        {
            float a = 2f * Mathf.PI * i / segments;
            vertices[i + 1] = new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, detectionRange);
        }
        vertices[segments + 1] = new Vector3(0f, 0f, detectionRange);
        int t = 0;
        for (int i = 0; i < segments; i++)
        {
            int current = i + 1;
            int next = ((i + 1) % segments) + 1;
            // Duplicate both windings so the translucent volume is visible from inside and out.
            triangles[t++] = 0; triangles[t++] = current; triangles[t++] = next;
            triangles[t++] = 0; triangles[t++] = next; triangles[t++] = current;
            triangles[t++] = segments + 1; triangles[t++] = next; triangles[t++] = current;
            triangles[t++] = segments + 1; triangles[t++] = current; triangles[t++] = next;
        }
        Mesh mesh = new Mesh { name = "3D Detection Cone" };
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }
    void ApplyConeTransform()
    {
        if (coneTransform == null) return;
        coneTransform.localPosition = coneLocalPosition;
        coneTransform.localEulerAngles = coneLocalRotation;
        coneTransform.localScale = coneLocalScale;
    }

    void ApplyConeColor()
    {
        if (coneRenderer == null) return;
        if (generatedMaterial != null)
        {
            // The generated material belongs only to this cone, so set its shader tint directly.
            generatedMaterial.color = coneColor;
            if (generatedMaterial.HasProperty("_BaseColor")) generatedMaterial.SetColor("_BaseColor", coneColor);
            if (generatedMaterial.HasProperty("_Color")) generatedMaterial.SetColor("_Color", coneColor);
            coneRenderer.SetPropertyBlock(null);
            return;
        }

        // Preserve shared project materials and tint only this renderer instance.
        if (coneProperties == null) coneProperties = new MaterialPropertyBlock();
        coneProperties.SetColor("_BaseColor", coneColor);
        coneProperties.SetColor("_Color", coneColor);
        coneRenderer.SetPropertyBlock(coneProperties);
    }

    void OnValidate()
    {
        ApplyConeTransform();
        if (Application.isPlaying && coneFilter != null) coneFilter.sharedMesh = MakeConeMesh();
        if (generatedMaterial != null)
        {
            generatedMaterial.color = coneColor;
            if (generatedMaterial.HasProperty("_BaseColor")) generatedMaterial.SetColor("_BaseColor", coneColor);
            if (generatedMaterial.HasProperty("_Color")) generatedMaterial.SetColor("_Color", coneColor);
        }
        ApplyConeColor();
    }

    void OnDestroy()
    {
        if (generatedMaterial != null) Destroy(generatedMaterial);
        if (coneFilter != null && coneFilter.sharedMesh != null) Destroy(coneFilter.sharedMesh);
    }
}




