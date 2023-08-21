using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveFunctionPropagationBehaviour : StateMachineBehaviour
{
    private WaveFunctionBaseBehaviour baseBehaviour;
    private VertexInd collapsed;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        baseBehaviour = animator.GetBehaviour<WaveFunctionBaseBehaviour>();
        collapsed = animator.GetBehaviour<WaveFunctionSelectionBehaviour>().selection;
        DetermineUpdatedEntropy(collapsed);
        List<PentaCoord> collapsedCycle = baseBehaviour.vertexData.vertexConfigurationTable[collapsed].cycle;
        List<VertexInd> adjacencies = new();
        for (int i = 0; i < collapsedCycle.Count - 1; i++)
            foreach (VertexInd indices in baseBehaviour.vertexData.tileVertexTable[collapsedCycle[i]])
                if (adjacencies.IndexOf(indices) == -1 && !indices.Equals(collapsed))
                    adjacencies.Add(indices);
        foreach (VertexInd adjacency in adjacencies)
            DetermineUpdatedEntropy(adjacency);
        if (baseBehaviour.possibilitySpace.entropyQueue.Count > 0)
            animator.SetTrigger("select");
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

    private void DetermineUpdatedEntropy(VertexInd indices)
    {
        VertexConfiguration vertConfig = baseBehaviour.vertexData.vertexConfigurationTable[indices];
        TypingChain partialTyping = DeterminePartialTyping(vertConfig, baseBehaviour);
        ReduceConformations(indices, partialTyping);
        ReduceConstructions(indices, partialTyping);
        if (partialTyping.IsComplete())
            baseBehaviour.possibilitySpace.entropyQueue.Remove(indices);
        return;
    }

    public static TypingChain DeterminePartialTyping(VertexConfiguration vertConfig, WaveFunctionBaseBehaviour baseBehaviour)
    {
        TypingChain partialTyping = new();
        for (int i = 0; i < vertConfig.cycle.Count - 1; i++)
        {
            TileController tile = baseBehaviour.activeTiles[vertConfig.cycle[i]];
            partialTyping.terrainTypes.Add(tile.properties.terrainType);
            partialTyping.featureTypes.Add(tile.properties.featureType);
        }
        return partialTyping;
    }

    private void ReduceConformations(VertexInd indices, TypingChain partialTyping)
    {
        if (!baseBehaviour.possibilitySpace.conformations.ContainsKey(indices))
            return;
        HashSet<TypingChain> nonconformingTypings = new();
        foreach (TypingChain conformation in baseBehaviour.possibilitySpace.conformations[indices])
            if (!conformation.ConformsTo(partialTyping))
                nonconformingTypings.Add(conformation);
        if (nonconformingTypings.Count > 0)
        {
            HashSet<TypingChain> conformingTypings = new(baseBehaviour.possibilitySpace.conformations[indices]);
            conformingTypings.ExceptWith(nonconformingTypings);
            baseBehaviour.possibilitySpace.conformations[indices] = new(conformingTypings);
            int newEntropy = baseBehaviour.possibilitySpace.conformations[indices].Count;
            baseBehaviour.possibilitySpace.entropyTable[indices] = newEntropy;
            baseBehaviour.possibilitySpace.entropyQueue.DecreaseKey(indices, newEntropy);
        }    
    }

    private void ReduceConstructions(VertexInd indices, TypingChain partialTyping)
    {
        if (!baseBehaviour.possibilitySpace.constructions.ContainsKey(indices))
            throw new System.IndexOutOfRangeException("Vertex " + indices.ToString() + " does not appear to have any constructions.");
        HashSet<List<VertexConfiguration>> nonconformingConfigs = new();
        foreach (List<VertexConfiguration> construction in baseBehaviour.possibilitySpace.constructions[indices])
            if (!TypingChain.DecodeConstruction(construction).ConformsTo(partialTyping))
                nonconformingConfigs.Add(construction);
        if (nonconformingConfigs.Count > 0)
        {
            HashSet<List<VertexConfiguration>> conformingConfigs = new(baseBehaviour.possibilitySpace.constructions[indices]);
            conformingConfigs.ExceptWith(nonconformingConfigs);
            baseBehaviour.possibilitySpace.constructions[indices] = new(conformingConfigs);
            if (!baseBehaviour.possibilitySpace.conformations.ContainsKey(indices) || baseBehaviour.possibilitySpace.conformations[indices].Count == 0) 
            {
                int newEntropy = baseBehaviour.possibilitySpace.constructions[indices].Count;
                baseBehaviour.possibilitySpace.entropyTable[indices] = newEntropy;
                baseBehaviour.possibilitySpace.entropyQueue.DecreaseKey(indices, newEntropy);
            }
        }
    }
}
