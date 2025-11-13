using UnityEngine;
using Cinemachine;
using System.Collections;
[RequireComponent(typeof(CinemachineVirtualCamera))]
public class DynamicCameraOffset : MonoBehaviour
{
    [Header("Follow Settings")]
    [Tooltip("Horizontal lead distance in world units when moving fast.")]
    public float maxLookAhead = 1.5f;

    [Tooltip("How smoothly camera moves horizontally.")]
    public float horizontalDamping = 5f;

    [Tooltip("How smoothly camera follows vertically.")]
    public float verticalDamping = 3f;

    [Tooltip("Minimum horizontal speed before look-ahead starts.")]
    public float velocityThreshold = 0.05f;

    [Tooltip("Vertical offset to keep more ground visible.")]
    public float verticalOffset = 1.0f;

    private CinemachineVirtualCamera vcam;
    private CinemachineFramingTransposer transposer;
    private Rigidbody2D playerRb;

    private Vector3 targetOffset;
    private Vector3 currentOffset;

    void Start()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();
        transposer = vcam.GetCinemachineComponent(CinemachineCore.Stage.Body) as CinemachineFramingTransposer;

        if (vcam.Follow == null)
        {
            Debug.LogError("DanTheManCamera: Virtual Camera needs a Follow target (player).");
            enabled = false;
            return;
        }

        playerRb = vcam.Follow.GetComponent<Rigidbody2D>();
        if (transposer == null || playerRb == null)
        {
            Debug.LogError("DanTheManCamera: Missing FramingTransposer or Rigidbody2D.");
            enabled = false;
            return;
        }

        currentOffset = transposer.m_TrackedObjectOffset;
        targetOffset = currentOffset;
    }

    void LateUpdate()
    {
        if (playerRb == null) return;

        float speedX = playerRb.velocity.x;

        // Horizontal lookahead (momentum based)
        float lookAheadX = 0f;
        if (Mathf.Abs(speedX) > velocityThreshold)
        {
            lookAheadX = Mathf.Sign(speedX) * Mathf.Lerp(0, maxLookAhead, Mathf.InverseLerp(0, 5f, Mathf.Abs(speedX)));
        }

        // Target offset (with ground bias)
        targetOffset = new Vector3(lookAheadX, verticalOffset, 0f);

        // Smoothly interpolate
        currentOffset.x = Mathf.Lerp(currentOffset.x, targetOffset.x, Time.deltaTime * horizontalDamping);
        currentOffset.y = Mathf.Lerp(currentOffset.y, targetOffset.y, Time.deltaTime * verticalDamping);

        transposer.m_TrackedObjectOffset = currentOffset;
    }
}
