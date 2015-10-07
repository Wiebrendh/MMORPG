using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Text;

public class ClientSocket : MonoBehaviour
{

    public Socket socket;
    public string ipString;

    // Threads
    public Thread receiveThread;

    [Space(10)]

    // Needed scripts
    public Game game;
    public PacketHandler handler;
    public PacketSender sender;

    void Start()
    {
        game = this.GetComponent<Game>();
        handler = this.GetComponent<PacketHandler>();
        sender = this.GetComponent<PacketSender>();

        StartConnect();
    }

    public void StartConnect()
    {
        Thread connectionThread = new Thread(ConnectServer);
        connectionThread.Start(connectionThread);
    }

    private void ConnectServer(object _thread)
    {
        Debug.Log("Connecting...");
        Thread thread = (Thread)_thread;

        try
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ip = new IPEndPoint(IPAddress.Parse(ipString), 25565);

            socket.Connect(ip);

            if (socket.Connected)
            {
                // Start a thread for receiveing player data
                receiveThread = new Thread(ReceiveData);
                //receiveThread.Start();

                // Send a packet containg the player his name
                List<byte> packet = new List<byte>();
                packet.AddRange(BitConverter.GetBytes((ushort)/*game.playerName.Length*/5));
                packet.AddRange(Encoding.ASCII.GetBytes(/*game.playerName*/"12345"));
                socket.Send(packet.ToArray(), SocketFlags.None);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Could not connect.. :: " + e);
        }
        thread.Abort();
    }

    private void ReceiveData()
    {
        try
        {
            while (true)
            {
                try
                {
                    byte[] buffer = new byte[socket.SendBufferSize];
                    int bufferLength = socket.Receive(buffer);

                    if (bufferLength > 0)
                    {
                        handler.Handle(buffer, socket);
                    }
                }
                catch
                {
                    Debug.LogError("Disconnected from server");
                    receiveThread.Abort();
                }
            }
        }
        catch (ThreadAbortException e)
        {
            Debug.Log(e);
        }
        
    }

    void OnApplicationQuit()
    {
        receiveThread.Abort();
        receiveThread = null;

        Debug.Log(receiveThread.IsAlive);
    }
}
