using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public GameObject myDeck;
    
    // Start is called before the first frame update
    void Start()
    {
        //Deck myDeck = new Deck(1);
        // Instantiate a gameobject from a prefab, grab its deck component, and assign it to myDeck
        // Player does not need to instantiate a deck, why not just make it part of the player prefab?
        // Deck myDeck = Instantiate(deck, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<Deck>();
        
        List<int> deckCode = new List<int>(){ 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1};
        myDeck.GetComponent<Deck>().GenerateDeck(deckCode);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
