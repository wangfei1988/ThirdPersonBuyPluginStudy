// © 2016 Mario Lelas
using UnityEngine;

namespace MLSpace
{
    /// <summary>
    /// state machine behaviour used on taking hit animation state
    /// </summary>
    public class TakeHitBehaviour : StateMachineBehaviour
    {
        private IGameCharacter character = null;    // game character reference

        /// <summary>
        /// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        /// </summary>
        /// <param name="animator">current animator</param>
        /// <param name="stateInfo">state info struct</param>
        /// <param name="layerIndex">animator layer index</param>
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (character == null)
            {
                character = animator.gameObject.GetComponent<IGameCharacter>();
                
            }
            if (character != null)
            {
                character.takingHit = true;
            }
        }

        /// <summary>
        ///  OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        /// </summary>
        /// <param name="animator">current animator</param>
        /// <param name="stateInfo">state info struct</param>
        /// <param name="layerIndex">animator layer index</param>
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (character == null)
            {
                character = animator.gameObject.GetComponent<IGameCharacter>();
            }
            if (character != null)
            {
                character.takingHit = false;
            }
        }

    } 
}
