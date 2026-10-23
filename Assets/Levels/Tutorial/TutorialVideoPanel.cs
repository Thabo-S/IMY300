using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Put this on each tutorial step panel (step1Panel, step2Panel, ...).
/// Plays a short muted looping clip while the panel is active, stops when hidden.
/// No changes needed in TutorialManager: ShowPanel/HidePanel already toggle the panel.
/// </summary>
[RequireComponent(typeof(VideoPlayer))]
public class TutorialVideoPanel : MonoBehaviour
{
    [Header("Assign")]
    [SerializeField] private VideoClip clip;       // the example clip for this step
    [SerializeField] private RawImage display;     // RawImage that replaces the text wall

    [Header("Optional")]
    [SerializeField] private Vector2Int resolution = new Vector2Int(1280, 720);

    private VideoPlayer player;
    private RenderTexture rt;

    private void Awake()
    {
        player = GetComponent<VideoPlayer>();
        rt = new RenderTexture(resolution.x, resolution.y, 0, RenderTextureFormat.ARGB32);
        rt.Create();

        player.playOnAwake = false;
        player.isLooping = true;
        player.waitForFirstFrame = true;
        player.renderMode = VideoRenderMode.RenderTexture;
        player.targetTexture = rt;
        player.audioOutputMode = VideoAudioOutputMode.None;
        player.source = VideoSource.VideoClip;
        player.clip = clip;

        display.texture = rt;
    }

    private void OnEnable()
    {
        if (clip == null) { display.gameObject.SetActive(false); return; }

        // hide until the first frame is ready so the player never sees a black flash
        display.color = new Color(1, 1, 1, 0);
        player.prepareCompleted += OnPrepared;
        player.Prepare();
    }

    private void OnPrepared(VideoPlayer vp)
    {
        vp.prepareCompleted -= OnPrepared;
        display.color = Color.white;
        vp.Play();
    }

    private void OnDisable()
    {
        if (player == null) return;
        player.prepareCompleted -= OnPrepared;
        player.Stop();
    }

    private void OnDestroy()
    {
        if (rt != null) { rt.Release(); Destroy(rt); }
    }
}
