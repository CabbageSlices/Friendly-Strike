using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Net;
using System.Net.Sockets;

public class NetworkClientTest : MonoBehaviour {

    WebSocket ws;
      
    // Use this for initialization
    void Start() {

        Invoke("Socket", 3);
    }

    void Socket() {
        IPAddress local = IPAddress.Any;
        var addresses = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
        foreach (var address in addresses) {

            if (address.AddressFamily == AddressFamily.InterNetwork)
                local = address;
        }

        ws = new WebSocket("ws://localhost:3000/socket.io/?EIO=2&transport=websocket");
        ws.OnMessage += (sender, e) => {
            var message = "42[\"message\", \"do you how do\"]";
            Debug.Log(e.Data);
            ws.Send(message);
        };

        ws.Connect();
        Debug.Log(ws.IsAlive + "    " + ws.Url);
        

        /*var ws = new WebSocket("ws://localhost:4343/Thing");
        ws.Connect();
        Debug.Log(ws.Origin);
        ws.Send("Client Message");*/
    }

    // Update is called once per frame
    void Update() {

    }

    private void OnApplicationQuit() {
        ws.Close();
    }
}
