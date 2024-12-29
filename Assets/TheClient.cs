using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

public class GyroData
{
    public float X;

    public float Y;

    public float Z;
}

public class TheClient : MonoBehaviour
{
    private Label _gyro;

    private TextField _theIP;

    private IPEndPoint _serverEndpoint;

    private Button _theButton;

    // Start is called before the first frame update
    void Start()
    {
        Input.gyro.enabled = true;

        UIDocument doc = GetComponent<UIDocument>();

        _theIP = (TextField)doc.rootVisualElement.Q("TheIP");
        _gyro = (Label)doc.rootVisualElement.Q("GeoData");
        _theButton = (Button)doc.rootVisualElement.Q("TheButton");

        _theButton.clicked += ToggleStartStop;
    }

    private void ToggleStartStop()
    {
        if (_serverEndpoint != null)
            _serverEndpoint = null;
        else
            try
            {
                var parts = _theIP.text.Split(":");
                var ip = IPAddress.Parse(parts[0]);
                var port = ushort.Parse(parts[1]);

                _serverEndpoint = new IPEndPoint(ip, port);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
    }


    // Update is called once per frame
    void Update()
    {
        _theButton.text = _serverEndpoint == null ? "Start" : "Stop";

        if (_serverEndpoint == null) return;

        try
        {
            using var client = new UdpClient();

            var gyro = Input.gyro.attitude;
            var message = JsonUtility.ToJson(new GyroData { X = gyro.x, Y = gyro.y, Z = gyro.z });

            _gyro.text = message;

            var data = Encoding.UTF8.GetBytes(message);

            client.Send(data, data.Length, _serverEndpoint);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    void OnDestroy()
    {
        _theButton.clicked -= ToggleStartStop;
    }
}
