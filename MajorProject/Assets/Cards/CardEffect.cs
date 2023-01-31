using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CardEffect : MonoBehaviour
{
    // public bool uniqueEffect = false;
    // public int lifeChange = 0;
    // public int preventLifeLoss = 0;
    // public int preventLifeGain = 0;
    // public int preventCardDraw = 0;
    // public int negateCard;
    
    public UnityEvent effect;
    

    // Make functions for each effect, then hook them up to another 
    // script that will call them when the card is played.

    // public void Damage()
    // {
    //     GetComponentInParent<Stack>().owner.GetComponent<Player>().life -= 4;
    // }
    //
    // public void Heal()
    // {
    //     GetComponentInParent<Stack>().owner.GetComponent<Player>().life += 4;
    // }
}
