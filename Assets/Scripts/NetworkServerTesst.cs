using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using WebSocketSharp;
//using WebSocketSharp.Server;
using System.Net;
using System.Text;
using System.Net.Sockets;
using Fleck2;

/*public class Thing : WebSocketBehavior {

    protected override void OnMessage(MessageEventArgs e) {

        var msg = e.Data;
        Debug.Log(msg);
    }
}*/

public class NetworkServerTesst : MonoBehaviour {

    // Use this for initialization
    /*void Start () {

        IPAddress local = IPAddress.Any;
        var addresses = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
        foreach (var address in addresses) {

            if (address.AddressFamily == AddressFamily.InterNetwork)
                local = address;
        }

        var wssv = new HttpServer(8183);

        wssv.OnGet += (sender ,e) => {
            
            foreach(string h in e.Request.Headers)
                Debug.Log(h + "     " + e.Request.Headers[h]);
            
            string data = "Hello";
            List<byte> bytes = new List<byte>();
            bytes.Add(0x81);
            bytes.Add( (byte)data.Length );
            bytes.AddRange(Encoding.ASCII.GetBytes(data));

            //e.Response.ContentType = "text/html";
            //e.Response.ContentEncoding = Encoding.UTF8;
            e.Response.Headers.Add("Access-Control-Allow-Origin", @"http://localhost:3000");
            
            e.Response.WriteContent(bytes.ToArray());
        };

        wssv.AddWebSocketService<Thing> ("/Thing");
        wssv.Start();
        Debug.Log(wssv.Address);
	}*/

    WebSocketServer server = new WebSocketServer("ws://192.168.1.75:8183");
    private void Start() {

        //server.Certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2("Data/Certificates/localhost.pfx", "garfield1");
        server.Start(socket =>
        {
            socket.OnOpen = () => Debug.Log("open");
            socket.OnMessage = message => Debug.Log(message);
        });
    }

    private void OnApplicationQuit() {

        server.Dispose();
    }
}
