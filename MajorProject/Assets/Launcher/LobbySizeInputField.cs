using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

/// <summary>
/// Lobby size input field. Let the user input lobby size, will be used to create a room.
/// </summary>
[RequireComponent(typeof(TMP_InputField))]
public class LobbySizeInputField : MonoBehaviour
{
    // Hold a reference to the Launcher
    [SerializeField]
    private Launcher launcher;
    
    // Store the PlayerPref Key to avoid typos
    const string lobbySizePrefKey = "4";

    private TMP_InputField _inputField;
    
    // Start is called before the first frame update
    void Start()
    {
        string defaultSize = string.Empty;
        _inputField = this.GetComponent<TMP_InputField>();
        if (_inputField != null)
        {
            if (PlayerPrefs.HasKey(lobbySizePrefKey))
            {
                defaultSize = PlayerPrefs.GetString(lobbySizePrefKey);
                _inputField.text = lobbySizePrefKey;
            }
        }
        // Convert the string byte for the max players per room
        int size = int.Parse(defaultSize);
        byte sizeByte = (byte) size;

        launcher.maxPlayersPerRoom = sizeByte;
    }
    
    // Set the lobby size, and save it in PlayerPrefs for future sessions
    public void SetLobbySize(string size)
    {
        // Important
        if (string.IsNullOrEmpty(size))
        {
            Debug.LogError("Lobby Size is null or empty");
            return;
        }
        if (ValidateLobbySize(size))
        {
            Debug.Log("Lobby Size is valid");
            PlayerPrefs.SetString(lobbySizePrefKey, size);
        }
        else
        {
            Debug.LogError("Lobby Size not valid, resetting");
            _inputField.text = lobbySizePrefKey;
        }
    }

    // Check if lobby size is a number
    private bool ValidateLobbySize(string size)
    {
        bool isNumber = int.TryParse(size, out _);
        if (!isNumber)
        {
            Debug.LogError("Lobby Size is not a number");
            return false;
        }
        else
        {
            return true;
        }
    }
}
