using UnityEngine;

public class PunchBehaviour : StateMachineBehaviour
{
    public PlayerController.TPunchType m_PunchType;
    [Range(0.0f, 1.0f)] public float m_StartPct;
    [Range(0.0f, 1.0f)] public float m_EndPct;
    PlayerController m_PlayerController;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_PlayerController = animator.GetComponent<PlayerController>();
        m_PlayerController.SetActivePunch(m_PunchType, false);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        bool l_Active = stateInfo.normalizedTime >= m_StartPct && stateInfo.normalizedTime <= m_EndPct;
        m_PlayerController.SetActivePunch(m_PunchType, l_Active);
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_PlayerController.SetActivePunch(m_PunchType, false);
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
