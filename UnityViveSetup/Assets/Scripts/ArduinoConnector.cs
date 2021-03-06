//Code inspired by Alan Zucconi (http://www.alanzucconi.com/?p=2979)
//Edited by Tiare Feuchtner

using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System;

public class ArduinoConnector: MonoBehaviour {
	[Tooltip("The serial port where the Arduino is connected")]
	public string port = "COM2";
	[Tooltip("The baud rate of the serial port")]
	public int baudRate = 9600;
    public float brightness = 0;
    public int zeroPoint = 127; //ERM motor = 127, LRA motor = 0


    private SerialPort stream;
    private bool portExists = false;

	void Awake () {
		stream = new SerialPort(port, baudRate);
		stream.ReadTimeout = 50;
	}

	// Use this for initialization
	void Start () {
		if (stream != null) {
			if(stream.IsOpen)
				stream.Close();

            String[] ports = SerialPort.GetPortNames();
            portExists = false;
            foreach(String p in ports)
            {
                if (p.Equals(port))
                    portExists = true;
            }

            if(portExists)
            {
                Debug.Log("ArduinoConnector: Connecting to device on port " + stream.PortName + "...");
                stream.Open();
            }   
            else
                Debug.LogWarning("ArduinoConnector: No device connected on port " + stream.PortName + ".");
        }

        //WriteToArduino("PING");
    }

    // Update is called once per frame
    void Update()
    {
        //AdjustBrightness(brightness);

        //StartCoroutine
        //(
        //    AsynchReadFromArduino
        //    ((string s) => HandleResponse(s),//InterpretMsg(s),     // Callback
        //        () => Debug.LogWarning("Error reading from arduino: timeout reached"), // Error callback
        //        10f                             // Timeout (seconds)
        //    )
        //);
    }

    private void HandleResponse(string msg)
    {
        //Debug.Log("> Arduino: " + msg);
    }

    public void AdjustHapticFeedback(float intensity)
    {
        int val = (zeroPoint == 0)? Mathf.RoundToInt(intensity*255) : Mathf.RoundToInt(intensity * zeroPoint) + zeroPoint;//200; //(int) intensity * (255-zeroPoint);
        WriteToArduino("LED " + val);
    }

    public void EnableHapticFeedback(bool val)
    {
        WriteToArduino("ENABLE " + ((val)?1:0));
    }

    private void WriteToArduino (string msg) {
        //Debug.Log("> Unity: "+msg);
        if (stream.IsOpen)
        {
            stream.WriteLine(msg);
            stream.BaseStream.Flush(); //flush to make sure the data is sent without any buffering
        }
	}

    private IEnumerator AsynchReadFromArduino(Action<string> callback, Action fail = null, float timeout = float.PositiveInfinity)
    {
        System.DateTime initialTime = DateTime.Now;
        DateTime nowTime;
        TimeSpan diff = default(TimeSpan);

        string dataString = null;

        do
        {
            try
            {
                if(stream.IsOpen)
                {
                    dataString = stream.ReadLine();
                    Debug.Log(dataString);
                }
            }
            catch (TimeoutException)
            {
                dataString = null;
            }

            if (dataString != null)
            {
                callback(dataString);
                yield return null; //similar to WaitForEndOfFrame call
            }
            else
                yield return new WaitForSeconds(0.05f);

            nowTime = DateTime.Now;
            diff = nowTime - initialTime;

        } while (diff.Milliseconds < timeout);

        if (fail != null)
            yield return null;
    }

    private void Close()
	{
        EnableHapticFeedback(false);
        //WriteToArduino("LED " + (0+zeroPoint));
        //WriteToArduino("ENABLE " + (0));
        if (stream != null) {
			if(stream.IsOpen)
				stream.Close();
			stream.Dispose();
		}
	}

	// Clean up 
	void OnDestroy() {
		Close();
	}
}
