using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    CameraManager cameraManager;
    PlayerLocomotion playerLocomotion;

    // Start is called before the first frame update
    void Awake()
    {
        playerLocomotion = GetComponent<PlayerLocomotion>();
        cameraManager = FindObjectOfType<CameraManager>();
    }

    private void Update()
    {
        playerLocomotion.HandleMovementAnimation();
    }

    private void FixedUpdate()
    {
        playerLocomotion.HandleMovement();
    }

    private void LateUpdate()
    {
        cameraManager.HandleCameraAllMovement();
    }
}
