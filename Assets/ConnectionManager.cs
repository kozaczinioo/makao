﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Web;
using UnityEngine;
using BestHTTP;
using BestHTTP.Extensions;
using BestHTTP.WebSocket;
using Newtonsoft.Json;

public class Item
{
    public bool is_game_on;
    public int whos_turn;
    public CardsConfig game_data;
}

public class Config
{
    public string player_id;
    public string room_id;
    public string server_address;
}
public class ConnectionManager : MonoBehaviour
{
    private static WebSocket webSocket;
    [SerializeField] private bool isMyTurn;
    [SerializeField] private Config config;
    private GameObject[] disableUIs;
    private SetCards setCards;

    private const float connectTimeout = 5;
    private float time_from_last_connection_request = connectTimeout;

    void Start()
    {
        config = new Config();
        setCards = GameObject.Find("SpriteCollection").GetComponent<SetCards>(); //todo change set Cards
//        disableUIs = GameObject.FindGameObjectsWithTag("DisableUI");
//        foreach (GameObject go in disableUIs)
//        {
//            go.SetActive(false);
//        }

        config = new Config {player_id = "1", room_id = "1", server_address = "ws://localhost/test/"}; // todo
    }

    private void Update()
    {
        if (!string.IsNullOrEmpty(config.player_id) && webSocket == null ||
            !string.IsNullOrEmpty(config.player_id) && !webSocket.IsOpen)
        {
            if (time_from_last_connection_request >= connectTimeout)
            {
                Debug.Log("Opening connection!");
                webSocket = ConnectToServer(config);

                time_from_last_connection_request = 0;
            }
            else
            {
                time_from_last_connection_request += Time.deltaTime;
            }
        }
//        else if (isMyTurn)
//        {
//            //try_send_turn(); todo send my state
//            Debug.Log("sending not implemented");
//        }
    }
    
    private WebSocket ConnectToServer(Config config)
    {
        string full_address = Path.Combine(config.server_address + config.room_id + "/" + config.player_id);
        Debug.Log("full_path: " + full_address);

        webSocket = new WebSocket(new Uri(full_address));
        webSocket.OnMessage += OnMessageRecieved;
        webSocket.Open();

        return webSocket;
    }

    private void OnMessageRecieved(WebSocket webSocket, string message)
    {
        Debug.Log(message);
        Item item = JsonConvert.DeserializeObject<Item>(message);
        if (item.is_game_on)
            setCards.setCards(item.game_data);

        IsMyTurn = item.whos_turn == config.player_id.ToInt32();
        // todo strzałka na tego co teraz gra
    }
    
    public void SendUpdateToServer(byte[] pixels)
    {
        // pixels_waiting_to_be_send = pixels; todo
        // are_there_pixels_to_be_send = true; todo
    }

    public void ConfigFromJson(string json)
    {
        config = JsonUtility.FromJson<Config>(json);
    }

    public void ChangeIsMyTurnFalse()
    {
        config.player_id = null;
    }
    
    public bool IsMyTurn
    {
        get => isMyTurn;
        set => isMyTurn = value;
    }
}