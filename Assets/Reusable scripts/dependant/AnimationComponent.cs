using UnityEngine;

public class AnimationComponent : MonoBehaviour
{
    [SerializeField]
    Animator animator;
    AnimationClip currentAnimation;


    public void SetBool(string boolName, bool value)
    {
        animator.SetBool(boolName, value);
    }
    public void SetFloat(string floatName, float value)
    {
        animator.SetFloat(floatName, value);
    }
    public void PlayAnimation(string animationName)
    {
        animator.Play(animationName);
    }
    public void PlayAnimation(string animationName, float speedMult)
    {

        animator.SetFloat("SpeedMult", speedMult);
        animator.Play(animationName);
        animator.SetFloat("SpeedMult", 1f);
        currentAnimation = null;
    }
    public void PlayAnimation(string animationName, float speedMult, bool reverse)
    {
        animator.SetFloat("SpeedMult", speedMult);
        animator.Play(animationName);
        animator.SetFloat("SpeedMult", 1f);
    }
}
