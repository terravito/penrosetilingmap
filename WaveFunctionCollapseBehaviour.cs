using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveFunctionCollapseBehaviour : StateMachineBehaviour
{
    private WaveFunctionBaseBehaviour baseBehaviour;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        baseBehaviour = animator.GetBehaviour<WaveFunctionBaseBehaviour>();
        VertexInd selection = animator.GetBehaviour<WaveFunctionSelectionBehaviour>().selection;
        VertexConfiguration vertConfig = baseBehaviour.vertexData.vertexConfigurationTable[selection];
        Debug.Log("Vertex primitive chain: " + vertConfig.primitiveChain);
        TypingChain typingChain = DetermineRandomTyping(selection);
        Debug.Log("Collapsed typing: " + typingChain.ToString());
        vertConfig.Set(typingChain);
        for (int i = 0; i < vertConfig.cycle.Count - 1; i++)
        {
            TileController tile = baseBehaviour.activeTiles[vertConfig.cycle[i]];
            if (typingChain.terrainTypes[i] != TerrainType.None)
                tile.properties.SetTerrain(typingChain.terrainTypes[i]);
            else
                tile.properties.SetFeature(typingChain.featureTypes[i]);
        }
        if (baseBehaviour.possibilitySpace.entropyQueue.Count > 0)
            animator.SetTrigger("propagate");
        else
        {
            Debug.Log("Wave function collapse is complete");
            animator.SetTrigger("complete");
        }
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

    private TypingChain DetermineRandomTyping(VertexInd selection)
    {
        if (baseBehaviour.possibilitySpace.conformations.ContainsKey(selection) && baseBehaviour.possibilitySpace.conformations[selection].Count > 0)
        {
            int typingChainIndex = Random.Range(0, baseBehaviour.possibilitySpace.conformations[selection].Count);
            return baseBehaviour.possibilitySpace.conformations[selection][typingChainIndex];
        } else if (baseBehaviour.possibilitySpace.constructions[selection].Count > 0)
        {
            int typingChainIndex = baseBehaviour.possibilitySpace.constraints.RandomConstructionIndexOfMaxLength(baseBehaviour.possibilitySpace.constructions[selection]);
            return TypingChain.DecodeConstruction(baseBehaviour.possibilitySpace.constructions[selection][typingChainIndex]);
        } else
        {
            VertexConfiguration vertConfig = baseBehaviour.vertexData.vertexConfigurationTable[selection];
            vertConfig.Set(WaveFunctionPropagationBehaviour.DeterminePartialTyping(vertConfig, baseBehaviour));
            return baseBehaviour.possibilitySpace.constraints.YieldRandomReconstruction(vertConfig);
        }
    }
}
