using UnityEngine;

public class WizardAnimationAttack : StateMachineBehaviour
{
     public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponentInParent<Wizard>().TakeDamageOn();
    }
}
