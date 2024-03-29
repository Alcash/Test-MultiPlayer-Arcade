﻿using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;


public class GameController : NetworkBehaviour
{
    public GameObject[] hazards;
    public Vector3 spawnValues;
    public int hazardCount;
    public float spawnWait;
    public float startWait;
    public float waveWait;

    
    NetworkManager m_NetWorkManager;
    public Text restartText;
    [SyncVar]
    private bool gameOver;    
    [SyncVar]
    private bool restart;
    public Text gameOverText;
    public Text TextPause;

    public string m_GameoverMessage = "Вы все умерли, все погибло и превратилось в прах";
    public string m_RestartMessage = "Восстань и рази врагов своих, жми кнопку R(к)";
    public string m_PauseMessage = "Один из Вас остановил время в мире. Ожидайте";


    List<PlayerHealth> m_Players;

    [ServerCallback]
    void Start()
    {
        m_NetWorkManager = GetComponent<NetworkManager>();
        //m_NetWorkManager
        gameOver = false;
        gameOverText.text = "";
        restartText.text = "";
        m_Players = new List<PlayerHealth>();

        StartCoroutine(SpawnWaves());
    }
    

    void Update()
    {
        if (restart)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                Restart();
                // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }

    public void Disconnect()
    {
        if (NetworkServer.active && NetworkManager.singleton.IsClientConnected())
        {

            NetworkManager.singleton.StopHost();

        }

        bool noConnection = (NetworkManager.singleton.client == null || NetworkManager.singleton.client.connection == null ||
                                 NetworkManager.singleton.client.connection.connectionId == -1);

        if (!NetworkManager.singleton.IsClientConnected() && !NetworkServer.active && NetworkManager.singleton.matchMaker == null)
        {
            if (!noConnection)
                NetworkManager.singleton.StopClient();
        }
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            TextPause.text = m_PauseMessage;
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
            TextPause.text = "";
        }

        
    }
    [ClientRpc]
    public void RpcPause(bool pause)
    {
        OnApplicationPause(pause);
    }

    IEnumerator SpawnWaves()
    {
        yield return new WaitForSeconds(startWait);
        while (true)
        {
            
            for (int i = 0; i < hazardCount; i++)
            {
                GameObject hazard = hazards[Random.Range(0, hazards.Length)];
                Vector3 spawnPosition = new Vector3(Random.Range(-spawnValues.x, spawnValues.x), spawnValues.y, spawnValues.z);
                Quaternion spawnRotation = Quaternion.identity;
                var enemy = Instantiate(hazard, spawnPosition, spawnRotation);
                NetworkServer.Spawn(enemy);
                yield return new WaitForSeconds(spawnWait);
            }
            yield return new WaitForSeconds(waveWait);
            IsGameOver();
            if (gameOver)
            {
                gameOverText.text = m_GameoverMessage;
                restartText.text = m_RestartMessage;
                restart = true;
                break;
            }
        }
    }

        

    public void IsGameOver()
    {
        m_Players.Clear();

          var clients = NetworkServer.connections ;
          foreach (NetworkConnection client in clients)
          {
              var players = client.playerControllers;
              foreach (UnityEngine.Networking.PlayerController _player in players)
              {
                  m_Players.Add(_player.gameObject.GetComponent<PlayerHealth>());
              }
          }
        
        

        m_Players.AddRange( GetComponents<PlayerHealth>());
        gameOver = true;

        Debug.Log("IsGameOver m_Players.count " + m_Players.Count);
        foreach (PlayerHealth player in m_Players)
        {
            gameOver &= player.isDead();
        }
        


    }

    void Restart()
    {
        gameOver = false;
        gameOverText.text = "";
        restartText.text = "";
        restart = false;       


        m_Players.ForEach(x => x.RpcAlive());
        StartCoroutine(SpawnWaves());
    }    

 }