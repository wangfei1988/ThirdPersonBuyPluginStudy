// © 2016 Mario Lelas
using UnityEngine;

namespace MLSpace
{
    /// <summary>
    /// state machine behaviour used on dive roll animation state
    /// </summary>
    [SharedBetweenAnimators]
    public class DiveRollBehaviour : StateMachineBehaviour
    {
        private TPCharacter character = null;    // game character reference

        /// <summary>
        /// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="stateInfo"></param>
        /// <param name="layerIndex"></param>
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (character == null)
            {
                character = animator.transform.GetComponent<TPCharacter>();
            }
            if (character != null)
            {
                character.turnToDiveRollDIrection();
            }
        }
    } 
}
