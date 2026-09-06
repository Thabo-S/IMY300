using System.Collections;
using UnityEngine;

public class laser_security_switch : MonoBehaviour
{
    public Transform lever;
    public GameObject lasers;

    [Header("Rotation Settings")]

    public Vector3 rotationAxis = Vector3.forward;
    public float rotationAmount = 90f;
    public float rotationDuration = 0.5f;

    private bool isToggled = false;
    private Coroutine rotateCoroutine;

    void Start()
    {
        if (lever == null)
        {
            Transform[] children = GetComponentsInChildren<Transform>();

            if (children.Length > 1)
                lever = children[1];
            else
                Debug.LogWarning($"No child transform found buddy.");
        }

        lasers.SetActive(true);

    }
    public void ToggleSwitch()
    {
        lasers.SetActive(isToggled);

        isToggled = !isToggled;

        if (lever == null) return;

        if (rotateCoroutine != null)
            StopCoroutine(rotateCoroutine);

        float signedAmount = isToggled ? -rotationAmount : rotationAmount;
        rotateCoroutine = StartCoroutine(RotateLever(signedAmount));
    }

    private IEnumerator RotateLever(float amount)
    {
        Quaternion startRotation = lever.localRotation;
        Quaternion endRotation = startRotation * Quaternion.AngleAxis(amount, rotationAxis);

        float elapsed = 0f;
        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / rotationDuration);
            lever.localRotation = Quaternion.Slerp(startRotation, endRotation, t);
            yield return null;
        }

        lever.localRotation = endRotation;
        rotateCoroutine = null;
    }
}