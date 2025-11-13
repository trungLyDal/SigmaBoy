using UnityEngine;
using Cinemachine;
using System.Collections; 

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class DynamicCameraOffset : MonoBehaviour
{
    [Header("Offset Settings")]
    [Tooltip("Screen X when facing right (0 = left edge, 1 = right edge).")]
    public float screenX_FacingRight = 0.25f;

    [Tooltip("Screen X when facing left.")]
    public float screenX_FacingLeft = 0.75f;

    [Tooltip("How long it takes to slide camera when flipping (seconds).")]
    public float transitionDuration = 0.3f;

    [Tooltip("Delay before camera reacts to quick turn (seconds).")]
    public float flipDelay = 0.15f;

    private CinemachineVirtualCamera vcam;
    private CinemachineFramingTransposer transposer;
    private PlayerPlatformerController playerController;

    private bool lastFacingRight = true;
    private float flipTimer = 0f;
    private Coroutine transitionRoutine;

    void Start()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();

        // ✅ Get the new-style Framing Transposer
        transposer = vcam.GetCinemachineComponent(CinemachineCore.Stage.Body) as CinemachineFramingTransposer;

        if (vcam.Follow != null)
            playerController = vcam.Follow.GetComponent<PlayerPlatformerController>();

        if (transposer == null || playerController == null)
        {
            Debug.LogError("DynamicCameraOffset: Missing Transposer or Player. Disabling script.");
            enabled = false;
        }
    }

    void LateUpdate()
    {
        if (playerController == null || transposer == null)
            return;

        bool isRight = playerController.IsFacingRight();

        // Detect direction change
        if (isRight != lastFacingRight)
        {
            flipTimer += Time.deltaTime;
            if (flipTimer >= flipDelay)
            {
                lastFacingRight = isRight;
                flipTimer = 0f;

                float targetScreenX = isRight ? screenX_FacingRight : screenX_FacingLeft;

                // Start smooth transition
                if (transitionRoutine != null)
                    StopCoroutine(transitionRoutine);
                transitionRoutine = StartCoroutine(SmoothTransition(targetScreenX));
            }
        }
        else
        {
            flipTimer = 0f; // reset if facing consistently
        }
    }

    private IEnumerator SmoothTransition(float target)
    {
        float start = transposer.m_ScreenX;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / transitionDuration;
            transposer.m_ScreenX = Mathf.Lerp(start, target, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }

        transposer.m_ScreenX = target;
    }
}
