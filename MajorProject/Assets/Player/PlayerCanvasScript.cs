using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCanvasScript : MonoBehaviour
{
    // Make sure the player canvas can survive the scene reload when a new player joins
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
