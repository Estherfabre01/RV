using UnityEngine;
using Valve.VR;

public class TrackpadMovementToggle : MonoBehaviour
{
    public float normalSpeed = 2.0f;
    public float fastSpeed = 4.0f;
    public float stepTime = 0.5f;

    private float stepTimer;
    private bool isBigLegStep;
    private bool isTrackpadActive;

    public SteamVR_Action_Vector2 trackpadAction; // Assign this in the Inspector
    public SteamVR_Input_Sources handType; // Assign left or right hand in the Inspector

    private CharacterController characterController;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        stepTimer = stepTime;
        isBigLegStep = true;
    }

    void Update()
    {
        // Check if trackpad is being used
        Vector2 trackpadValue = trackpadAction.GetAxis(handType);
        isTrackpadActive = trackpadValue != Vector2.zero;

        // Get input from the trackpad
        float h = trackpadValue.x;
        float v = trackpadValue.y;

        Vector3 move = new Vector3(h, 0, v);
        move = transform.TransformDirection(move);

        // Toggle between normal and fast speed to simulate the leg difference
        stepTimer -= Time.deltaTime;
        if (stepTimer <= 0)
        {
            isBigLegStep = !isBigLegStep;
            stepTimer = stepTime;
        }

        float speed = isTrackpadActive ? (isBigLegStep ? fastSpeed : normalSpeed) : normalSpeed;
        characterController.Move(move * speed * Time.deltaTime);
    }
}
