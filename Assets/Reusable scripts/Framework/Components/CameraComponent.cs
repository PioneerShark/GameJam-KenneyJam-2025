using System;
using UnityEngine;

[ExecuteInEditMode]
public class CameraComponent : MonoBehaviour
{
    [Header("Camera Settings")]
    public Vector3 offset = Vector3.zero;
    public Vector3 eulerAngle = Vector3.zero;
    [Range(0f, 100f)] public float distance = 50f;
    [Range(0f, 1f)] public float panWeightX = 0.5f;
    [Range(0f, 1f)] public float panWeightY = 0.5f;
    [Range(0f, 1f)] public float panWeightZ = 0.5f;

    [Header("Interpolation")]
    [Range(0f, 16f)]
    public float smoothSpeed = 4f;
    public bool snapToPosition = false;
    public bool snapToRotation = false;
    public bool snapToDistance = false;

    [Header("Edit Mode")]
    public bool updateInEditMode = false;
    [SerializeField] private Camera _targetCamera;
    [SerializeField] private Transform _targetPanTowards;
    private Vector3 _targetPanTowardsPosition = Vector3.zero;
    private bool _panTowardsPosition = false;

    protected virtual void LateUpdate()
    {
        // Conditions
        ResolveCamera();
        if (_targetCamera == null) return;
        if (!Application.isPlaying && !updateInEditMode) return;

        // Update
        float lerpValue = Application.isPlaying == true ? 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime) : 1f;
        if (!_panTowardsPosition)
        {
            _targetPanTowardsPosition = _targetPanTowards != null ? _targetPanTowards.position : this.transform.position;
        }
        else
        {

        }

        Vector3 targetDistanceOffset = Vector3.zero;

        if (!_targetCamera.orthographic)
        {
            // Predict where camera should be
            Vector3 cachedPosition = this.transform.position;
            Quaternion cachedRotation = this.transform.rotation;

            this.transform.position = Vector3.zero;
            this.transform.rotation = Quaternion.Euler(eulerAngle);
            this.transform.Translate(0, 0, -distance);

            targetDistanceOffset = this.transform.position;
            this.transform.position = cachedPosition;
        }
        else
        {
            _targetCamera.orthographicSize = Mathf.Lerp(_targetCamera.orthographicSize, distance, snapToDistance == false ? lerpValue : 1f);
        }

        // Update
        Vector3 currentPosition = this.transform.position;

        float originX = Mathf.Lerp(currentPosition.x, _targetPanTowardsPosition.x, panWeightX);
        float originY = Mathf.Lerp(currentPosition.y, _targetPanTowardsPosition.y, panWeightY);
        float originZ = Mathf.Lerp(currentPosition.z, _targetPanTowardsPosition.z, panWeightZ);

        Vector3 targetPosition = new Vector3(originX, originY, originZ) + targetDistanceOffset + offset;
        Quaternion targetRotation = Quaternion.Euler(eulerAngle);

        // Move and Rotate _targetCamera
        _targetCamera.transform.position = Vector3.Lerp(_targetCamera.transform.position, targetPosition, snapToPosition == false ? lerpValue : 1f);
        _targetCamera.transform.rotation = Quaternion.Slerp(_targetCamera.transform.rotation, targetRotation, snapToRotation == false ? lerpValue : 1f);
    }

    public virtual void SetCamera(Camera setCamera)
    {
        _targetCamera = setCamera;
    }

    public virtual Camera GetCamera()
    {
        return _targetCamera;
    }

    public virtual void PanTowards(Transform setTargetPanTowards)
    {
        _targetPanTowards = setTargetPanTowards;
        _panTowardsPosition = false;
    }

    public virtual void PanTowards(Vector3 setTargetPanTowardsPosition)
    {
        _targetPanTowards = null;
        _panTowardsPosition = true;
        _targetPanTowardsPosition = setTargetPanTowardsPosition;
    }

    protected virtual Camera ResolveCamera()
    {
        if (_targetCamera != null)
        {
            return _targetCamera;
        }
        else
        {
            _targetCamera = Camera.main;
            return _targetCamera;
        }
    }
}