using UnityEngine;
using Cinemachine;

public class CameraZoneTrigger : MonoBehaviour
{
    [Header("Cameras")]
    public CinemachineVirtualCamera aboveGroundCamera;
    public CinemachineVirtualCamera undergroundCamera;

    [Header("Backgrounds")]
    public GameObject aboveGroundBackgrounds;
    public GameObject undergroundBackgrounds;

    [Header("Trigger Settings")]
    [Tooltip("The Y-position of the player must be BELOW this line to switch to the underground cam.")]
    public float verticalThreshold; 

    void Start()
    {
        if (verticalThreshold == 0)
        {
            verticalThreshold = transform.position.y;
        }
        
        // ⭐ 2. ADD THIS
        // Ensure we start with the correct backgrounds active
        aboveGroundBackgrounds.SetActive(true);
        undergroundBackgrounds.SetActive(false);
    }

    void OnTriggerStay2D(Collider2D other) 
    {
        if (other.transform.root.CompareTag("Player"))
        {
            if (other.transform.position.y < verticalThreshold)
            {
                // Player is BELOW threshold: Activate Underground
                if (undergroundCamera.Priority != 20)
                {
                    Debug.Log("Player is BELOW threshold. Activating Underground Cam.");
                    undergroundCamera.Priority = 20;
                    aboveGroundCamera.Priority = 10;
                    
                    // ⭐ 3. ADD THESE TWO LINES
                    undergroundBackgrounds.SetActive(true);
                    aboveGroundBackgrounds.SetActive(false);
                }
            }
            else
            {
                // Player is ABOVE threshold: Activate AboveGround
                if (aboveGroundCamera.Priority != 20)
                {
                    Debug.Log("Player is ABOVE threshold. Activating AboveGround Cam.");
                    aboveGroundCamera.Priority = 20;
                    undergroundCamera.Priority = 10;
                    
                    // ⭐ 4. ADD THESE TWO LINES
                    undergroundBackgrounds.SetActive(false);
                    aboveGroundBackgrounds.SetActive(true);
                }
            }
        }
    }
}