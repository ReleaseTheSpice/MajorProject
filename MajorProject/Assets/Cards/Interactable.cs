using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEditor;

public class Interactable : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [SerializeField] private Canvas canvas;
    private RectTransform rectTransform;
    public Vector3 homePosition;
    // ^ Maybe this shouldnt be serializable/public?
    
    // Set to true when the card can be dragged, i.e. when it is in the player's hand
    public bool draggable = false;
    // Set to true when it is the current player's turn, and they can play cards
    public bool myTurn = false;
    
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
    
    // Fires when the player stops dragging the object
    public void OnEndDrag(PointerEventData eventData)
    {
        // Debug.Log("End Drag");
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        // Debug.Log("OnPointerDown");
    }
    
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log(gameObject.name + " Dropped on " + eventData.pointerCurrentRaycast.gameObject.name);
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(eventData.position), Vector2.zero);
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
                // Debug.Log("Hit Nothing");
                ReturnHome();
            }
        }
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