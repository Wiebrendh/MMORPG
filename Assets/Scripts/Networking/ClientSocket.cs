using UnityEngine;
using UnityEngine.UI;
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
    public Text nameInputField;

    // Threads
    public Thread receiveThread;

    [Space(10)]

    // Needed scripts
    public Game game;
    public PacketHandler handler;
    public PacketSender sender;

    // Packet receiver
    public List<byte[]> packetQeue = new List<byte[]>();

    void Start()
    {
        game = this.GetComponent<Game>();
        handler = this.GetComponent<PacketHandler>();
        sender = this.GetComponent<PacketSender>();
    }

    void Update ()
    {
        if (packetQeue.Count > 0)
        {
            handler.Handle(packetQeue[0], socket);
            packetQeue.RemoveAt(0);
        }
    }

    public void StartConnect()
    {
        if (nameInputField.text.Length >= 5)
        {
            ipString = GameObject.Find("InputField_ServerIP").GetComponent<InputField>().text;

            game.playerName = nameInputField.text;
            Thread connectionThread = new Thread(ConnectServer);
            connectionThread.Start(connectionThread);
        }
        else
            Debug.Log("Name field is empty or name is to short.");
    }

    private void ConnectServer(object _thread)
    {
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
                receiveThread.IsBackground = true;
                receiveThread.Start();
                
                // Send a packet containg the player his name
                List<byte> packet = new List<byte>();
                packet.AddRange(BitConverter.GetBytes((ushort)game.playerName.Length));
                packet.AddRange(Encoding.ASCII.GetBytes(game.playerName));
                socket.Send(packet.ToArray(), SocketFlags.None);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Could not connect.. :: " + e);
        }
        thread.Abort();
    }

    bool shouldExecute = true;
    private void ReceiveData()
    {
        while (shouldExecute)
        {
            try
            {
                byte[] buffer = new byte[socket.SendBufferSize];
                int bufferLength = socket.Receive(buffer);
                if (bufferLength > 0)
                {
                    //handler.Handle(buffer, socket);
                    packetQeue.Add(buffer);
                }
            }
            catch { receiveThread.Abort(); }
        }
    }

    void OnApplicationQuit()
    {
        sender.Disconnect(game.playerID);
        receiveThread.Abort();
        shouldExecute = false;
    }
}
