using System.Collections.Generic;
using UnityEngine;

public class EquationDropZone : MonoBehaviour
{
    public bool isLeftSide = true;
    public Transform tokenContainer;
    public List<MathToken> containedTokens = new List<MathToken>();
    
    private void OnTriggerEnter(Collider other)
    {
        MathToken token = other.GetComponent<MathToken>();
        if (token != null && !containedTokens.Contains(token))
        {
            // Position token neatly in container
            if (tokenContainer != null)
            {
                token.transform.position = tokenContainer.position + 
                                           new Vector3(containedTokens.Count * 0.15f, 0, 0);
                token.transform.rotation = tokenContainer.rotation;
            }
            
            containedTokens.Add(token);
            EquationController.Instance.CheckBalance();
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        MathToken token = other.GetComponent<MathToken>();
        if (token != null && containedTokens.Contains(token))
        {
            containedTokens.Remove(token);
            EquationController.Instance.CheckBalance();
        }
    }
    
    public float GetTotalValue()
    {
        float total = 0;
        foreach (var token in containedTokens)
            total += token.value;
        return total;
    }
    
    public void Reset()
    {
        containedTokens.Clear();
    }
}