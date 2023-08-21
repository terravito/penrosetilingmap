using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveFunctionSelectionBehaviour : StateMachineBehaviour
{
    public VertexInd selection { get; private set; }

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        WaveFunctionBaseBehaviour baseBehaviour = animator.GetBehaviour<WaveFunctionBaseBehaviour>();
        List<VertexInd> extractedIndices = new();
        extractedIndices.Add(baseBehaviour.possibilitySpace.entropyQueue.ExtractMin());
        int lowestEntropy = baseBehaviour.possibilitySpace.entropyTable[extractedIndices[0]];
        while (baseBehaviour.possibilitySpace.entropyTable[extractedIndices[extractedIndices.Count - 1]] == lowestEntropy && baseBehaviour.possibilitySpace.entropyQueue.Count > 0)
            extractedIndices.Add(baseBehaviour.possibilitySpace.entropyQueue.ExtractMin());
        baseBehaviour.possibilitySpace.entropyQueue.Insert(extractedIndices[extractedIndices.Count - 1], baseBehaviour.possibilitySpace.entropyTable[extractedIndices[extractedIndices.Count - 1]]);
        int selectedIndex = Random.Range(0, extractedIndices.Count - 1);
        for (int i = 0; i < extractedIndices.Count - 1; i++)
            if (i != selectedIndex)
                try { baseBehaviour.possibilitySpace.entropyQueue.Insert(extractedIndices[i], lowestEntropy); }
                catch (System.ArgumentException e) { throw new System.ArgumentException(); }
        selection = extractedIndices[selectedIndex];
        Debug.Log("Selected vertex " + selection.ToString() + " for collapse.");
        animator.SetTrigger("collapse");
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

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
