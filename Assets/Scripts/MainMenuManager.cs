﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour {

    [SerializeField]
    Button m_ButtonStart, m_ButtonExit, m_ButtonConnect;
    [SerializeField]
    InputField m_InputFieldAddress;
    
    NetworkManager m_NetworkManager;

    [SerializeField]
    string IpAddress = "localhost";
	// Use this for initialization
	void Start () {
       // m_NetworkManager = GetComponent<NetworkManager>();
        m_InputFieldAddress.onEndEdit.AddListener(onEndEdit);
        m_InputFieldAddress.text = IpAddress;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void m_ButtonStartClicked()
    {
        NetworkManager.singleton.StartHost();
    }
    public void m_ButtonExitClicked()
    {
        Application.Quit();
    }
    public void m_ButtonConnectClicked()
    {
        if (m_InputFieldAddress.gameObject.activeSelf)
        {
            Connect(IpAddress);
        }
        m_InputFieldAddress.gameObject.SetActive(true);
        
    }

    private void Connect(string ipAddress)
    {
        var address = ipAddress.Split(':');
        NetworkManager.singleton.networkAddress = address[0];
        if(address.Length >1 )
            if(address[1] == "" )
                NetworkManager.singleton.networkPort = int.Parse(address[1]);
        NetworkManager.singleton.StartClient();
    }

    public void onEndEdit(string ipAddress)
    {
        IpAddress = ipAddress;
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Connect(ipAddress);
        }
    }
}
