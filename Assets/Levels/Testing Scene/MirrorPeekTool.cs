using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A low-cost corner-peek camera initialized and driven by the player inventory.
/// </summary>
public class MirrorPeekTool : MonoBehaviour
{
    [Header("View")]
    public Transform mirrorViewOrigin;
    public Camera playerCamera;
    [Range(30f, 90f)] public float viewFieldOfView = 55f;
    [Min(64)] public int textureWidth = 512;
    [Min(64)] public int textureHeight = 288;

    [Header("Limits")]
    [Min(0.25f)] public float maximumPeekSeconds = 3.5f;
    public Vector2 panelSize = new Vector2(360f, 210f);
    public Vector2 panelMargin = new Vector2(24f, 24f);

    public UnityEvent onPeekStarted;
    public UnityEvent onPeekEnded;

    public bool IsPeeking { get; private set; }
    private Camera peekCamera;
    private RenderTexture peekTexture;
    private bool ownsCamera;
    private Coroutine timeoutRoutine;

    void Awake()
    {
        // Pickup objects do not need a preview camera; Inventory adds this component to the player.
        if (GetComponent<Inventory>() == null) return;
        if (playerCamera == null) playerCamera = Camera.main;
        if (mirrorViewOrigin == null && playerCamera != null)
        {
            // Fallback view point for the inventory-driven version of the tool.
            Transform existingOrigin = playerCamera.transform.Find("Mirror View Origin");
            if (existingOrigin == null)
            {
                GameObject originObject = new GameObject("Mirror View Origin");
                existingOrigin = originObject.transform;
                existingOrigin.SetParent(playerCamera.transform, false);
                existingOrigin.localPosition = new Vector3(0.18f, -0.12f, 0.25f);
                existingOrigin.localRotation = Quaternion.Euler(0f, 35f, 0f);
            }
            mirrorViewOrigin = existingOrigin;
        }
        if (mirrorViewOrigin == null)
            Debug.LogWarning("MirrorPeekTool needs a Mirror View Origin transform and a MainCamera-tagged player camera.", this);
        CreatePeekCamera();
    }

    void CreatePeekCamera()
    {
        if (mirrorViewOrigin == null) return;
        if (peekCamera == null)
        {
            GameObject cameraObject = new GameObject("Mirror Peek Camera");
            peekCamera = cameraObject.AddComponent<Camera>();
            ownsCamera = true;
        }
        peekCamera.transform.SetPositionAndRotation(mirrorViewOrigin.position, mirrorViewOrigin.rotation);
        if (playerCamera != null)
        {
            peekCamera.CopyFrom(playerCamera);
            peekCamera.cullingMask = playerCamera.cullingMask;
        }
        peekCamera.fieldOfView = viewFieldOfView;
        peekCamera.enabled = false;

        peekTexture = new RenderTexture(Mathf.Max(64, textureWidth), Mathf.Max(64, textureHeight), 16, RenderTextureFormat.ARGB32);
        peekTexture.name = "Mirror Peek Preview";
        peekTexture.filterMode = FilterMode.Bilinear;
        peekTexture.Create();
        peekCamera.targetTexture = peekTexture;
    }

    public void SetPeeking(bool pressed)
    {
        if (pressed) BeginPeek();
        else EndPeek();
    }

    public void BeginPeek()
    {
        if (IsPeeking || peekCamera == null || mirrorViewOrigin == null) return;
        IsPeeking = true;
        peekCamera.enabled = true;
        if (timeoutRoutine != null) StopCoroutine(timeoutRoutine);
        timeoutRoutine = StartCoroutine(StopAfterLimit());
        if (onPeekStarted != null) onPeekStarted.Invoke();
    }

    public void EndPeek()
    {
        if (!IsPeeking) return;
        IsPeeking = false;
        if (peekCamera != null) peekCamera.enabled = false;
        if (timeoutRoutine != null)
        {
            StopCoroutine(timeoutRoutine);
            timeoutRoutine = null;
        }
        if (onPeekEnded != null) onPeekEnded.Invoke();
    }

    IEnumerator StopAfterLimit()
    {
        yield return new WaitForSeconds(maximumPeekSeconds);
        timeoutRoutine = null;
        EndPeek();
    }

    void LateUpdate()
    {
        if (IsPeeking && peekCamera != null && mirrorViewOrigin != null)
            peekCamera.transform.SetPositionAndRotation(mirrorViewOrigin.position, mirrorViewOrigin.rotation);
    }

    void OnGUI()
    {
        if (!IsPeeking || peekTexture == null) return;
        float width = Mathf.Min(panelSize.x, Screen.width - panelMargin.x * 2f);
        float height = Mathf.Min(panelSize.y, Screen.height - panelMargin.y * 2f);
        Rect panel = new Rect(Screen.width - width - panelMargin.x, Screen.height - height - panelMargin.y, width, height);
        GUI.Box(panel, "MIRROR PEEK");
        Rect image = new Rect(panel.x + 8f, panel.y + 24f, panel.width - 16f, panel.height - 32f);
        GUI.DrawTexture(image, peekTexture, ScaleMode.ScaleToFit, false);
    }

    void OnDisable() { EndPeek(); }

    void OnDestroy()
    {
        EndPeek();
        if (peekCamera != null) peekCamera.targetTexture = null;
        if (peekTexture != null)
        {
            peekTexture.Release();
            Destroy(peekTexture);
        }
        if (ownsCamera && peekCamera != null) Destroy(peekCamera.gameObject);
    }
}
