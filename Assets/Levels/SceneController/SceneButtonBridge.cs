using UnityEngine;

/// <summary>
/// Lives in each scene (NOT persistent) purely so Inspector-wired UI
/// buttons have something stable to reference. Forwards every call to
/// whatever SceneController.Instance currently is at click time - never
/// caches it, since the actual singleton object can be swapped/destroyed
/// between scene loads (see SceneController's Bootstrap/duplicate-guard).
/// </summary>
public class SceneButtonBridge : MonoBehaviour
{
    public void GoToMainMenu() => SceneController.Instance?.GoToMainMenu();
    public void GoToStore() => SceneController.Instance?.GoToStore();
    public void GoToTutorial() => SceneController.Instance?.GoToTutorial();
    public void StartSelectedLevel() => SceneController.Instance?.StartSelectedLevel();
    public void RestartLevel() => SceneController.Instance?.RestartLevel();
    public void NextLevel() => SceneController.Instance?.NextLevel();
    public void SelectLevel(int levelIndex) => SceneController.Instance?.SelectLevel(levelIndex);
}