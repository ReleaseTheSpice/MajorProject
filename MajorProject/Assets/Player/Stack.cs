using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class Stack : MonoBehaviour
{
    public List<GameObject> cards;
    
    public List<Delegate> cardEffects;
    
    [Header("References")]
    //public GameObject owner;      // Reference to the player GameObject that the stack belongs to (not used)
    public Player owner;            // Reference to the player script that the stack belongs to
    public GameObject lifeText;     // Reference to the player's life text
    public TextMeshProUGUI nameText;    // Reference to the player's name text
    public Transform canvasTransform;   // Reference to this stack's Canvas
    
    private bool displayingCards = false;

    #region Photon

    #endregion
    
    #region Card Functions

    public void AddCard(GameObject card)
    {
        Debug.Log("Adding card " + card.name + " to stack");
        // card.transform.SetParent(transform);
        // Set the home position to the stack (I have no idea why y=40)
        Vector3 newPosition = new Vector3(0, 40, 0);
        card.GetComponent<Interactable>().homePosition = newPosition;
        card.GetComponent<Interactable>().ReturnHome();
        cards.Add(card);
    }

    #endregion

    #region Card Resolving Things

    // Variables to keep track of card effects
    private bool counterNextCard = false;
    private bool counterAllCards = false;
    private bool stabilizeLife = false;
    private bool lifeDoubler = false;
    private bool damageDoubler = false;
    private bool goToHand = false;
    private bool preventCardDraw = false;
    private bool lostLifeThisRound = false;
    private bool gainedLifeThisRound = false;
    private bool drawnCardsThisRound = false;
    private int bonusDamage = 0;

    // Resolve the effects of all cards on the stack, most recent first, and return them to their original owners
    public void ResolveEffects()
    {
        Debug.Log("Resolving effects");
        // Error checking
        if (cardEffects.Count == 0)
        {
            Debug.LogWarning("No effects to resolve");
            return;
        }
        if (cardEffects.Count != cards.Count)
        {
            Debug.LogError("Error: Number of effects does not match number of cards");
            return;
        }
        
        // Loop through the cards in reverse order
        for (int i = cards.Count - 1; i >= 0; i--)
        {
            if (!counterNextCard)
            {
                // Trigger the effect (cardEffects have the same indexes as cards)
                cardEffects[i].DynamicInvoke();
            }
            else
            {
                Debug.Log("Card Countered");
                if (!counterAllCards)
                {
                    counterNextCard = false;
                }
            }
            
            // Card has been resolved, Destroy it and instantiate a new one in the deck
            GameObject originalOwner = cards[i].GetComponent<Card>().owner;
            
            // Only return a card to a deck if this is the client with access to that deck (or hand)
            // Or if the card is supposed to go to a hand, then the owner of that hand will run this
            if (originalOwner.GetComponent<Player>().myDeck != null || goToHand)
            {
                
                if (goToHand) // Some cards will go to a hand instead of returning to a deck after played
                {
                    goToHand = false;
                    Deck localDeck = owner.myDeck.GetComponent<Deck>();
                    GameObject newCard = localDeck.InstantiateNewCard(cards[i]);
                    owner.myHand.GetComponent<Hand>().AddCard(newCard);
                }
                else
                {
                    Deck originalDeck = originalOwner.GetComponent<Player>().myDeck.GetComponent<Deck>();
                    GameObject newCard = originalDeck.InstantiateNewCard(cards[i]);
                    // Call AddCard on the owner's deck
                    originalDeck.AddCard(newCard);
                }
            }
            // Photon RPC to the original owner to destroy the card
            GameManager.NetworkManager.PV.RPC("DestroyCard", 
                originalOwner.GetComponent<PhotonView>().Owner, 
                cards[i].GetComponent<PhotonView>().ViewID);
        }
        // Photon RPC to update the life text
        GameManager.NetworkManager.PV.RPC("SetPlayerLife", RpcTarget.All, 
            owner.gameObject.GetComponent<PhotonView>().ViewID, owner.life);

        // Clear the stack on all clients
        GameManager.NetworkManager.PV.RPC("ClearStack", RpcTarget.All,
            GetComponent<PhotonView>().ViewID);
        
        // Final cleanup
        ResetVariables();
    }

    // Reset all card effect variables to their default values
    private void ResetVariables()
    {
        counterNextCard = false;
        counterAllCards = false;
        stabilizeLife = false;
        lifeDoubler = false;
        damageDoubler = false;
        goToHand = false;
        preventCardDraw = false;
        lostLifeThisRound = false;
        gainedLifeThisRound = false;
        drawnCardsThisRound = false;
        bonusDamage = 0;
    }

    #endregion

    #region Card Effects

    // Card ID: 0
    public void Maul()
    {
        LoseLife(4);
    }
    
    // Card ID: 1
    public void Heal()
    {
        GainLife(4);
    }
    
    // Card ID: 2
    public void Nope()
    {
        counterNextCard = true;
    }
    
    // Card ID: 3
    public void Hibernate()
    {
        lifeDoubler = true;
    }
    
    // Card ID: 4
    public void BearMarket()
    {
        damageDoubler = true;
    }
    
    // Card ID: 5
    public void Stabilize()
    {
        stabilizeLife = true;
    }
    
    // Card ID: 6
    public void Scavenge()
    {
        DrawCards(2);
    }
    
    // Card ID: 7
    public void ProfessorBear()
    {
        LoseLife(6);
        DrawCards(3);
    }
    
    // Card ID: 8
    public void BadBerries()
    {
        LoseLife(5);
        goToHand = true;
    }
    
    // Card ID: 9
    public void GoodBerries()
    {
        GainLife(1);
        goToHand = true;
    }
    
    // Card ID: 10
    public void LockDown()
    {
        preventCardDraw = true;
    }

    // Card ID: 11
    public void Wither()
    {
        DrawCards(2);
        int cardsInHand = owner.myHand.GetComponent<Hand>().cards.Count;
        LoseLife(cardsInHand);
    }
    
    // Card ID: 12
    public void Cull()
    {
        // If you haven't gained life this round, lose 5 life
        if (!gainedLifeThisRound)
        {
            LoseLife(5);
        }
    }
    
    // Card ID: 13
    public void Infestation()
    {
        // Lose 2 life for each card on the stack
        LoseLife(cards.Count * 2);
    }
    
    // Card ID: 14
    public void Hoard()
    {
        // Gain 2 life for each card on the stack
        GainLife(cards.Count * 2);
    }
    
    // Card ID: 15
    public void Steal()
    {
        // If you've drawn cards this round, lose 7 life
        if (drawnCardsThisRound)
        {
            LoseLife(7);
        }
    }
    
    // Card ID: 16
    public void BigHands()
    {
        // Gain 1 life for each card in your hand
        GainLife(owner.myHand.GetComponent<Hand>().cards.Count + 1);
    }
    
    // Card ID: 17
    public void SwiftEnd()
    {
        // If you passed your turn, increase life lose this round by 3
        if (owner.passedTurn)
        {
            bonusDamage += 3;
        }
    }
    
    // Card ID: 18
    public void Regroup()
    {
        // If you have lost life this round, draw 3 cards.
        if (lostLifeThisRound)
        {
            DrawCards(3);
        }
    }
    
    // Card ID: 19
    public void Embolden()
    {
        // If you haven’t lost life this round, gain 10 life.
        if (!lostLifeThisRound)
        {
            GainLife(10);
        }
    }
    
    // Card ID: 20
    public void Trap()
    {
        // If you haven’t lost life this round, lose 7 life.
        if (!lostLifeThisRound)
        {
            LoseLife(7);
        }
    }
    
    // Card ID: 21
    public void Punish()
    {
        // If you passed your turn, lose 10 life.
        if (owner.passedTurn)
        {
            LoseLife(10);
        }
    }
    
    // Card ID: 22
    public void Burn()
    {
        // If you have lost life this round, lose 10 life.
        if (lostLifeThisRound)
        {
            LoseLife(10);
        }
    }
    
    // Card ID: 23
    public void GoToSpace()
    {
        // If you have lost life this round, gain 7 life.
        if (lostLifeThisRound)
        {
            GainLife(7);
        }
    }

    // Card ID: 24
    public void ceab()
    {
        // If you didn’t pass your turn, negate all cards in this stack.
        if (!owner.passedTurn)
        {
            counterAllCards = true;
        }
    }
    
    #endregion

    #region Card Effect Helper Functions
    
    private void GainLife(int amount)
    {
        if (stabilizeLife)
        {
            stabilizeLife = false;
        }
        else
        {
            if (lifeDoubler)
            {
                //TODO: Perhaps multiple doublers should stack multiplicitively
                amount *= 2;
                lifeDoubler = false;
            }
            owner.life += amount;
            gainedLifeThisRound = true;
        }
    }
    
    private void LoseLife(int amount)
    {
        if (stabilizeLife)
        {
            stabilizeLife = false;
        }
        else
        {
            if (damageDoubler)
            {
                amount *= 2;
                damageDoubler = false;
            }
            amount += bonusDamage;
            owner.life -= amount;
            lostLifeThisRound = true;
        }
    }
    
    private void DrawCards(int amount)
    {
        if (preventCardDraw)
        {
            preventCardDraw = false;
        }
        else
        {
            owner.myDeck.GetComponent<Deck>().DrawCard(amount);
        }
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
        cardEffects = new List<Delegate>();
        nameText.text = owner.playerName;
    }

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        lifeText.GetComponent<TextMeshProUGUI>().text = owner.life.ToString();
    }

    void OnMouseDown()
    {
        //Debug.Log("Player " + PhotonNetwork.LocalPlayer.NickName + " clicked on the stack");
        displayingCards = !displayingCards;
        if (displayingCards)
        {
            // Display the cards
            int halfCount = cards.Count / 2;
            for (int i = 0; i < cards.Count; i++)
            {
                Vector3 shiftDistance = new Vector3(220f, 0, 0);
                shiftDistance *= (i - halfCount);
                // If the number of cards is even, shift them all to the right
                if (cards.Count % 2 == 0)
                {
                    shiftDistance += new Vector3(110f, 0, 0);
                }
                cards[i].transform.position += shiftDistance;
            }
        }
        else
        {
            // Hide the cards
            for (int i = 0; i < cards.Count; i++)
            {
                cards[i].GetComponent<Interactable>().ReturnHome();
            }
        }
    }
    
    #endregion
}
