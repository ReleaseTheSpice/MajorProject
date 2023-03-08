using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEditor;

public class Interactable : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [SerializeField] private Canvas canvas;
    private RectTransform rectTransform;
    public Vector3 homePosition;
    // ^ Maybe this shouldnt be serializable/public?
    
    // Set to true when the card can be dragged, i.e. when it is in the player's hand
    public bool draggable = false;
    // Set to true when it is the current player's turn, and they can play cards
    public bool myTurn = false;
    
    // Index of the card ordered in heirarchy
    private int siblingIndex;

    // True when the card is currently being dragged
    private bool dragging = false;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        homePosition = rectTransform.anchoredPosition3D;
    }
    
    public void ReturnHome()
    {
        rectTransform.anchoredPosition3D = homePosition;
    }

    #region Input Callbacks
    
    // Fires when the player starts dragging the object
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Debug.Log("Begin Drag");
        if (draggable && myTurn)
        {
            //siblingIndex = rectTransform.GetSiblingIndex();
            // Set the sibling index to the highest so the card is on top of the others
            rectTransform.SetSiblingIndex(100);
            dragging = true;
        }
    }
    
    // Fires when the player is dragging the object
    public void OnDrag(PointerEventData eventData)
    {
        if (draggable && myTurn)
        {
            // Debug.Log("Drag");
            
            // eventData.delta is the amount the mouse has moved since the last OnDrag event
            // divide by the canvas scale factor so the mouse distance is the same as screen distance
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
    }
    
    // Fires when the player stops dragging the object (called after OnDrop, before OnPointerExit)
    public void OnEndDrag(PointerEventData eventData)
    {
        // Debug.Log("End Drag");
        if (draggable && myTurn)
        {
            // Return the card to its original position
            rectTransform.anchoredPosition3D = homePosition;
            // Set the sibling index back to its original value
            rectTransform.SetSiblingIndex(siblingIndex);
            dragging = false;
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        // Debug.Log("OnPointerDown");
    }
    
    public void OnDrop(PointerEventData eventData)
    {
        // This is called first after a drag ends
        //Debug.Log(gameObject.name + " Dropped on " + eventData.pointerCurrentRaycast.gameObject.name);
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(eventData.position), Vector2.zero);
        // Only perform the drop action if the card was in the player's hand
        if (gameObject.GetComponentInParent<Hand>() != null)
        {
            if (hit.collider != null && hit.collider.gameObject.CompareTag("DropZone"))
            {
                // Remove the card from the hand
                gameObject.GetComponentInParent<Hand>().RemoveCard(gameObject);
                // Network instantiate the card on the stack
                Stack s = hit.collider.gameObject.GetComponent<Stack>();
                //GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
                GameObject newCard = PhotonNetwork.Instantiate(gameObject.name, s.transform.position, Quaternion.identity);
                // RPC to add the card to the stack on all clients
                int cardID = newCard.GetComponent<PhotonView>().ViewID;
                int stackID = s.GetComponent<PhotonView>().ViewID;
                int playerID = gameObject.GetComponent<Card>().owner.GetComponent<PhotonView>().ViewID;
                GameManager.NetworkManager.PV.RPC(
                    "AddCardToStack", RpcTarget.All, cardID, stackID, playerID, gameObject.name);
                // End the turn
                //gameObject.GetComponentInParent<Player>().EndTurn();
                NetworkManager.LocalPlayer.GetComponent<Player>().EndTurn();
                // Delete the original card object
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("Hit Nothing");
                // ReturnHome();
            }
        }
    }

    //Detect if the Cursor starts to pass over the GameObject
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        // If the card is being dragged, it may have triggered this event by going behind something
        // In this case do nothing
        if (dragging)
        {
            return;
        }
        transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);   // scale up
        transform.position += new Vector3(0, 50f, 0);           // shift up
        siblingIndex = transform.GetSiblingIndex();         // save sibling index
        transform.SetAsLastSibling();                       // move to front
    }

    //Detect when Cursor leaves the GameObject (called last on drag)
    public void OnPointerExit(PointerEventData pointerEventData)
    {
        // If the card is being dragged, it may have triggered this event by going behind something
        // In this case do nothing
        if (dragging)
        {
            return;
        }
        // Debug.Log("Cursor Exiting " + name + " GameObject");
        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);   // scale down
        ReturnHome(); // Return to home position (shift down)
        transform.SetSiblingIndex(siblingIndex);            // restore sibling index
    }

    #endregion
    
    public void updateCanvas(Canvas c)
    {
        canvas = c;
    }
}


// base card script - hand in hand with the cardobj
//
// trap/secret cards?
//
//     board stack? with general effects?
//     card that makes other cards face down?
//
//     base card obj - contains list of "effect" ob