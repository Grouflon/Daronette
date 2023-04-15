using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPCameraEffects : MonoBehaviour
{
    public float sprintFieldOfView = 80.0f;
    private float playerMoveSpeed;
    private float playerSprintSpeed;
    private Camera playerCamera;
    private float defaultFOV;

    // Start is called before the first frame update
    void Start()
    {
        playerCamera = GetComponent<Camera>();
        defaultFOV = playerCamera.fieldOfView;
        playerSprintSpeed = GetComponentInParent<FPSPlayerController>().sprintingMoveSpeed;

    }

    // Update is called once per frame
    void Update()
    {
        playerMoveSpeed = GetComponentInParent<FPSPlayerController>().moveSpeed;

        if (GetComponentInParent<FPSPlayerController>().state == PlayerState.Sprinting )
        {
            //put here headbobs effects
        }

        if (playerMoveSpeed == playerSprintSpeed)
        {
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, sprintFieldOfView, Time.deltaTime * 3);
        }
        else
        {
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, defaultFOV, Time.deltaTime * 3);
        }       
    }
}
