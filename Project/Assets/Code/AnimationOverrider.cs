using UnityEngine;

public class AnimationOverrider : MonoBehaviour
{
    private Animator m_animator;

    private void Awake()
    {
        m_animator = GetComponent<Animator>();
    }

    public void SetAnimations(AnimatorOverrideController overrideController)
    {
        m_animator.runtimeAnimatorController = overrideController;
    }
}
