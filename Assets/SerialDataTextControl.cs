using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO.Ports;

public class SerialDataTextControl : MonoBehaviour
{

    SerialPort sp = new SerialPort("COM4", 9600);
    private string receiveddata;
    //private bool[] flags = new bool[3];
    Text pulsetext;
    Text temptext;
    Text gastext;
    Text motiontext;

    // Start is called before the first frame update
    void Start()
    {

        pulsetext = GameObject.Find("PulseSensor").GetComponent<Text>();
        temptext = GameObject.Find("TempSensor").GetComponent<Text>();
        gastext = GameObject.Find("GasSensor").GetComponent<Text>();
        motiontext = GameObject.Find("MotionSensor").GetComponent<Text>();
        sp.Open();
    }

    // Update is called once per frame
    void Update()
    {
        receiveddata = sp.ReadLine();
        string[] datanumber = receiveddata.Split(';');
        pulsetext.text = "Heart Rate: " + datanumber[0];
        temptext.text = "Temperature: " + datanumber[1] + "\n" + "Humidity: " + datanumber[2];
        gastext.text = "TVOC: " + datanumber[3] + "\n" + "eCO2: " + datanumber[4];
        motiontext.text = "Position    x:" + datanumber[5] + " y:" + datanumber[6] + " z:" + datanumber[7] + "\n" + "Acceleration    x:" + datanumber[8] + " y:" + datanumber[9] + " z:" + datanumber[10];
    }
}
