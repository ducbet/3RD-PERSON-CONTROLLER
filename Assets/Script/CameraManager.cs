using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("Movement attributes")]
    public float cameraSmoothTime = 0.1f;
    [Header("Rotate attributes")]
    public float cameraRotationSpeed = 5f;
    public float minimumVerticalAngle = -3f;
    public float maximumVerticalAngle = 20f;
    [Header("Handle collision attributes")]
    public float spherecastThickness = 0.3f;
    public LayerMask layerMask;
    private float defaultCameraDistance;
    private bool canZoomOut = false;

    private Transform target;
    private Transform movementPivotTransform;
    private Transform rotationPivotTransform;
    private Transform cameraTransform;
    private InputManager inputManager;
    private Vector3 currentMovementVelocity = Vector3.zero;
    private Vector3 currentZoomInVelocity = Vector3.zero;  // Is it ok to reuse currentMovementVelocity?
    private Vector3 currentZoomOutVelocity = Vector3.zero;  // Is it ok to reuse currentMovementVelocity?

    private Vector3 cameraRotatelAngle;
    private Vector3 pivotRotateAngle;

    private void Awake()
    {
        target = FindObjectOfType<PlayerManager>().transform;
        GameObject targetGO = target.gameObject;
        inputManager = targetGO.GetComponent<InputManager>();

        movementPivotTransform = gameObject.transform;
        rotationPivotTransform = movementPivotTransform.GetChild(0);
        cameraTransform = Camera.main.transform;

        cameraRotatelAngle = cameraTransform.localRotation.eulerAngles;
        pivotRotateAngle = movementPivotTransform.localRotation.eulerAngles;

        defaultCameraDistance = cameraTransform.localPosition.magnitude;
    }

    public void HandleCameraAllMovement()
    {
        FollowTarget();
        RotateCamera();
        HandleCameraCollision();
    }

    private void FollowTarget()
    {
        Vector3 cameraPosition = Vector3.SmoothDamp(movementPivotTransform.position, target.position, ref currentMovementVelocity, cameraSmoothTime);
        movementPivotTransform.position = cameraPosition;
    }

    private void RotateCamera()
    {
        // rotate camera on X axis: vertical rotation (rotate camera up and down)
        cameraRotatelAngle.x -= inputManager.verticalCameraInput * cameraRotationSpeed * Time.deltaTime;
        cameraRotatelAngle.x = Mathf.Clamp(cameraRotatelAngle.x, minimumVerticalAngle, maximumVerticalAngle);
        cameraTransform.localRotation = Quaternion.Euler(cameraRotatelAngle);

        // rotate pivot on Y axis: horizontal rotation (rotate camera around pivot)
        pivotRotateAngle.y += inputManager.horizontalCameraInput * cameraRotationSpeed * Time.deltaTime;
        rotationPivotTransform.localRotation = Quaternion.Euler(pivotRotateAngle);
    }
    private void HandleCameraCollision()
    {
        Vector3 direction = cameraTransform.position - rotationPivotTransform.position;
        Vector3 offset = direction.normalized * 0.5f;  // offset is for the case player is to close to the wall -> can't detect
        float sphereCastLength = defaultCameraDistance < direction.magnitude ? defaultCameraDistance + 0.5f : (direction + offset).magnitude;
        RaycastHit hit;
        if (Physics.SphereCast(rotationPivotTransform.position - offset, spherecastThickness, direction, out hit, sphereCastLength, layerMask))
        {
            // hit.point is not on the cameraTransform-rotationPivotTransform line. It has small offset because the thickness of the SphereCast
            // If we zoom in to the hit.point, the forward of the camera is not pointing to the player anymore
            // This trick is still not perfect. Sometime the player is not in the center of the camera
            Vector3 preciseHitPointPrecise = rotationPivotTransform.position + Vector3.Project(hit.point - rotationPivotTransform.position, direction);
            ZoomIn(preciseHitPointPrecise + offset);  // offset is for the camera does not zoom too close to the player
        }
        else
        {
            ZoomOut(rotationPivotTransform.position + (direction.normalized * defaultCameraDistance));
        }
    }

    private void ZoomIn(Vector3 hitPoint)
    {
        Vector3 zoomedInCameraPosition = Vector3.SmoothDamp(cameraTransform.position, hitPoint, ref currentZoomInVelocity, cameraSmoothTime);
        cameraTransform.position = zoomedInCameraPosition;
        canZoomOut = true;
    }

    private void ZoomOut(Vector3 defaultCameraPosition)
    {
        if (!canZoomOut)
        {
            return;
        }
        Vector3 zoomedOutCameraPosition = Vector3.SmoothDamp(cameraTransform.position, defaultCameraPosition, ref currentZoomOutVelocity, cameraSmoothTime);
        cameraTransform.position = zoomedOutCameraPosition;
        if ((defaultCameraPosition - zoomedOutCameraPosition).magnitude < 0.1)
        {
            canZoomOut = false;
        }
    }
}
