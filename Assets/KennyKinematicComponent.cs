using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class KennyKinematicComponent : MonoBehaviour
{
    [SerializeField] private Transform _rootTransform;
    [SerializeField] private Transform _spineTransform;
    [SerializeField] private GameObject[] _rigConstraints;

    public Vector3 GetRootPosition()
    {
        return _rootTransform.position;
    }

    public void LerpRootRotationTowards(Vector3 targetPosition, float lerpValue = 0.05f)
    {
        Vector3 dir = new Vector3(targetPosition.x, _rootTransform.position.y, targetPosition.z) - _rootTransform.position;

        Quaternion targetRotation = Quaternion.LookRotation(dir);
        _rootTransform.rotation = Quaternion.Lerp(_rootTransform.rotation, targetRotation, lerpValue);
    }

    public void LerpRootRotation(Vector3 eulerAngles, float lerpValue = 0.05f)
    {
        _rootTransform.rotation = Quaternion.Lerp(_rootTransform.rotation, Quaternion.Euler(eulerAngles), lerpValue);
    }

    public void LerpSpineRotation(Vector3 eulerAngles, float lerpValue = 0.05f)
    {
        _spineTransform.rotation = Quaternion.Lerp(_spineTransform.rotation, Quaternion.Euler(eulerAngles), lerpValue);
    }

    public void LerpRigConstraintWeight(float weight, float lerpValue = 0.05f)
    {
        // IRigConstraint
        foreach (GameObject rigConstraint in _rigConstraints)
        {
            IRigConstraint component = rigConstraint.GetComponent<IRigConstraint>();
            if (component != null)
            {
                component.weight = Mathf.Lerp(component.weight, weight, lerpValue);
            }
        }
    }
}
