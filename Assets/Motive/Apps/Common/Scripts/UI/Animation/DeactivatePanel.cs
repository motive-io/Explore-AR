// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.UI.Framework;
using UnityEngine;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Used to deactivate a panel at the end of a pop animation.
    /// </summary>
    class DeactivatePanel : StateMachineBehaviour
    {
        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var panel = animator.gameObject.GetComponentInParent<Panel>();
            panel.SetActive(false);
            panel.gameObject.transform.localScale = Vector3.one;
        }
    }

}