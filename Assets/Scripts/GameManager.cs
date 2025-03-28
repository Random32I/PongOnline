using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using System;
using System.Net;
using System.Text;
using System.Net.Sockets;

public class GameManager : MonoBehaviour
{
    //GameManager will be responsible for being the client

    [SerializeField] int score;
    [SerializeField] int oppScore;
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI oppScoreText;
    [SerializeField] GameObject[] paddles;
    [SerializeField] GameObject Ball;
    [SerializeField] Ball refToBall;
    public float ballXDir = 1;
    public float ballYDir = -1;

    // Server Stuff
    private static byte[] buffer = new byte[2048];
    private static byte[] outBuffer = new byte[2048];
    private static IPEndPoint remoteEP;
    private static EndPoint remoteServer;

    private static Socket client;

    public int ClientIndex = 0;

    public bool serverBeingUpdated = false;

    public static void StartClient()
    {
        try
        {
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            remoteEP = new IPEndPoint(ip, 1111);
            remoteServer = (EndPoint)remoteEP;

            client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Blocking = false;
            client.ReceiveBufferSize = 2048;

        }
        catch (Exception e)
        {
            Debug.Log("Exception: " + e.ToString());
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        StartClient();
        refToBall = Ball.GetComponent<Ball>();
    }

    // Update is called once per frame
    void Update()
    {
        //Sending Blank
        //byte[] bufferNoUpdate = new byte[2048];
        //client.SendTo(bufferNoUpdate, remoteEP);

        //Sending
        buffer = Encoding.ASCII.GetBytes($"{paddles[0].transform.position.x},{ClientIndex},{Ball.transform.position.x},{Ball.transform.position.y},{ballXDir},{ballYDir}");
        client.SendTo(buffer, remoteEP);

        //Recieving
        int recv = client.ReceiveFrom(outBuffer, ref remoteServer);

        Vector2 BallPos;
        Vector2 BallDir;
        int index;
        float newPos = StringToData(Encoding.ASCII.GetString(outBuffer, 0, recv), out index, out BallPos, out BallDir);



        Debug.Log($"Recieved: {newPos} from {index}");

        if (ClientIndex == 0)
        {
            ClientIndex = index;
        }

        paddles[1].transform.position = new Vector3(-newPos, 4, 0);

        if (ClientIndex == 2)
        {
            /*if (refToBall.enabled)
            {
                refToBall.enabled = false;
            }*/
            Ball.transform.position = -BallPos;
            ballXDir = -BallDir.x;
            ballYDir = -BallDir.y;
        }

    }

    public void UpdateServer()
    {
        //Sending
        serverBeingUpdated = true;
    }

    float StringToData(string str, out int paddleIndex, out Vector2 BallPos, out Vector2 BallDir)
    {
        float pos;

        string[] strings = str.Split(',');

        pos = float.Parse(strings[0]);
        paddleIndex = int.Parse(strings[1]);

        BallPos = new Vector2(float.Parse(strings[2]), float.Parse(strings[3]));

        BallDir = new Vector2(float.Parse(strings[4]), float.Parse(strings[5]));

        return pos;
    }

    public int GetScore()
    {
        return score;
    }

    public void AddScore(int GoalIndex)
    {
        if (GoalIndex == 0)
        {
            score++;
            scoreText.text = $"{score}";
        }
        else
        {
            oppScore++;
            oppScoreText.text = $"{oppScore}";
        }
        paddles[0].transform.position = new Vector3(0, -4, 0);
        paddles[1].transform.position = new Vector3(0, 4, 0);

        //Add Networking stuff here to update the server that it reset, both through TCP and UDP
    }
}
