using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Highlights interactables (items, doors, keypads, levers...) whenever the
/// player is within range, so players can SEE what's interactable without
/// having to look straight at it first.
///
/// The "[E] Pick up X" text on interactionTextUI is still handled by
/// Player.PerformContinuousDetection (untouched). This script only owns the
/// "nearby" outline state.
///
/// Put this on the same GameObject as Player.
/// </summary>
public class InteractableHighlighter : MonoBehaviour
{
    [Header("Range")]
    [Tooltip("Interactables within this distance get outlined. Keep this larger than Player.pickUpRange.")]
    [SerializeField] private float highlightRange = 6f;

    [Tooltip("How often (seconds) to rescan for nearby interactables.")]
    [SerializeField] private float scanInterval = 0.15f;

    [Header("What to highlight")]
    [SerializeField] private bool highlightItems = true;
    [SerializeField] private bool highlightDoorsAndDevices = true;

    [Header("Visibility")]
    [Tooltip("If true, interactables behind walls are NOT highlighted (stops x-ray item hunting).")]
    [SerializeField] private bool requireLineOfSight = true;

    [SerializeField] private LayerMask scanMask = ~0;

    private static readonly string[] DeviceTags =
    {
        "Door", "Door_Keycard", "KeyPad", "LockedDoor", "Lever"
    };

    private readonly Collider[] buffer = new Collider[128];
    private readonly HashSet<Outline> active = new HashSet<Outline>();
    private readonly HashSet<Outline> foundThisScan = new HashSet<Outline>();
    private readonly List<Outline> toRemove = new List<Outline>();

    private Camera cam;
    private float scanTimer;

    private void Start()
    {
        cam = Camera.main;
    }

    private void OnDisable()
    {
        foreach (Outline o in active)
            if (o != null) o.enabled = false;

        active.Clear();
    }

    private void Update()
    {
        if (PauseMenu.isGamePause) return;

        scanTimer -= Time.deltaTime;
        if (scanTimer > 0f) return;
        scanTimer = scanInterval;

        Scan();
    }

    // Player.Update() disables the outline of whatever you just looked away from,
    // which would kill our "nearby" highlight. LateUpdate runs after every Update
    // and before rendering, so we just re-assert the outlines every frame (no flicker).
    private void LateUpdate()
    {
        if (PauseMenu.isGamePause) return;

        foreach (Outline o in active)
        {
            if (o != null && !o.enabled)
                o.enabled = true;
        }
    }

    private void Scan()
    {
        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null) return;
        }

        foundThisScan.Clear();

        int count = Physics.OverlapSphereNonAlloc(
            transform.position, highlightRange, buffer, scanMask, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            Collider col = buffer[i];
            if (col == null || !IsInteractable(col)) continue;

            // Same lookup Player.cs uses: Outline lives on the collider's GameObject.
            Outline outline = col.GetComponent<Outline>();
            if (outline == null) continue;

            if (requireLineOfSight && !HasLineOfSight(col)) continue;

            foundThisScan.Add(outline);
        }

        // Turn on new ones
        foreach (Outline o in foundThisScan)
        {
            if (active.Add(o))
                o.enabled = true;
        }

        // Turn off ones that left range / got occluded / were destroyed (picked up)
        toRemove.Clear();
        foreach (Outline o in active)
        {
            if (o == null || !foundThisScan.Contains(o))
                toRemove.Add(o);
        }

        foreach (Outline o in toRemove)
        {
            active.Remove(o);
            if (o != null) o.enabled = false;
        }
    }

    private bool IsInteractable(Collider col)
    {
        if (highlightDoorsAndDevices)
        {
            for (int i = 0; i < DeviceTags.Length; i++)
            {
                if (col.CompareTag(DeviceTags[i])) return true;
            }
        }

        if (highlightItems && col.GetComponentInParent<Item>() != null)
            return true;

        return false;
    }

    private bool HasLineOfSight(Collider target)
    {
        Vector3 from = cam.transform.position;
        Vector3 to = target.bounds.center;

        if (!Physics.Linecast(from, to, out RaycastHit hit, scanMask, QueryTriggerInteraction.Ignore))
            return true;

        Transform h = hit.transform;
        Transform t = target.transform;

        if (h == t || h.IsChildOf(t)) return true;

        Item item = target.GetComponentInParent<Item>();
        return item != null && h.IsChildOf(item.transform);
    }
}
