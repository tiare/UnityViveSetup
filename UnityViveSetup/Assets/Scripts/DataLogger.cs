using System;
using System.IO;
using UnityEngine;

public class DataLogger : MonoBehaviour {

    public int ID = 0;
    public string fileName = "data_file", directory = "Data", delimiter = ",";

    private StreamWriter fileWriter;
    private string path = "", line = "", eventString = "", trialString = "";
    private int oldID = 0;

    void Awake()
    {
    }

    // Use this for initialization
    void Start () {
        string now = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        path = directory + "/" + now + "-" + fileName + "-" + ID + ".csv";

        //check if directory exists or create it
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        //create file
        fileWriter = new StreamWriter(@path);

        //if (fileWriter != null)
        //{ 
        //    //write a first line with the column names
        //    fileWriter.WriteLine(
        //        "timestamp" + delimiter +
        //        "handShiftOffset"+ delimiter +
        //        "handShiftAngle"+ delimiter +
        //        "headTilt" + delimiter +
        //        "handPosReal" + delimiter +
        //        "handPosVirtual" + delimiter +
        //        "touchPos" + delimiter +
        //        "touchPosLocal" + delimiter +
        //        "targetPos" + delimiter +
        //        "targetPosLocal" + delimiter +
        //        "sqDistToTarget" + delimiter +
        //        "touchingPad" + delimiter +
        //        "event");
        //    Debug.Log("DataLogger: Writing data to file <" + path + ">");
        //}
    }
	
	// Update is called once per frame
	void Update () {
        line = Time.unscaledTime.ToString("r");

    //if (trialString == "")
    //        trialString = //delimiter + ""//trialNumber
    //        delimiter + ""//V3ToString(handShiftOffset)
    //        + delimiter + ""//handShiftAngle
    //        + delimiter + ""//headTilt
    //        + delimiter + ""//V3ToString(handPosReal)
    //        + delimiter + ""//V3ToString(handPosVirtual)
    //        + delimiter + ""//V3ToString(touchPos)
    //        + delimiter + ""//V3ToString(touchPosLocal)
    //        + delimiter + ""//V3ToString(targetPos)
    //        + delimiter + ""//V3ToString(targetPosLocal)
    //        + delimiter + ""//sqDistToTarget
    //        + delimiter + "";//touchingPad;

        line = line
                + trialString
                + eventString;

        //write line to file
        if (fileWriter != null)
            fileWriter.WriteLine(line);

        //clear strings
        eventString = "";
        trialString = "";
    }

    void OnDestroy()
    {
        line = line + delimiter + "TheEnd";
        //write last (perhaps empty) line to get ending time
        if (fileWriter != null)
            fileWriter.WriteLine(line);

        fileWriter.Close();
        if (fileWriter != null)
            fileWriter.Dispose();
    }

    //public void LogUserCalibrationData (float userHeight, float hmdHeight)
    //{
    //    eventString = eventString + delimiter + "Calibrate" + delimiter + "height:" + userHeight + delimiter + "hmdHeight:" + hmdHeight;
    //}

    //public void LogTrialData(Vector3 handShiftOffset, float handShiftAngle, float headTilt, Vector3 handPosReal, Vector3 handPosVirtual, Vector3 touchPos, Vector3 touchPosLocal, Vector3 markerPosLocal, Vector3 targetPos, Vector3 targetPosLocal, bool touchingPad)
    //{
    //    float sqDistToTarget = (touchPos == Vector3.zero)? Mathf.Infinity:Mathf.Pow(touchPos.x - targetPos.x, 2) + Mathf.Pow(touchPos.y - targetPos.y, 2) + Mathf.Pow(touchPos.z - targetPos.z, 2);

    //    trialString = delimiter + V3ToString(handShiftOffset)
    //                + delimiter + handShiftAngle
    //                + delimiter + headTilt   
    //                + delimiter + V3ToString(handPosReal) 
    //                + delimiter + V3ToString(handPosVirtual)    
    //                + delimiter + V3ToString(touchPos)
    //                + delimiter + V3ToString(touchPosLocal)
    //                + delimiter + V3ToString(targetPos)
    //                + delimiter + V3ToString(targetPosLocal) 
    //                + delimiter + sqDistToTarget 
    //                + delimiter + touchingPad;
    //}

    private string V3ToString(Vector3 vec)
    {
        return "(" + vec.x + " " + vec.y + " " + vec.z + ")";
    }
}
