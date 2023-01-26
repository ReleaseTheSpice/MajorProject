using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stack : MonoBehaviour
{
    public List<GameObject> cards;
    
    [Header("References")]
    public GameObject owner;    // Reference to the player GameObject that the stack belongs to
    public Transform canvasTransform;   // Reference to this stack's Canvas

    #region Card Functions

    public void AddCard(GameObject card)
    {
        Debug.Log("Adding card");
        cards.Add(card);
    }

    #endregion
    
    public Transform GetCanvasTransform()
    {
        return canvasTransform;
    }

    #region Callback Handlers

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #endregion
}
