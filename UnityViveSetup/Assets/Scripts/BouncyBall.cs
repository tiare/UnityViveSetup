using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncyBall : MonoBehaviour {
    public float amplitude = 0.5f;
    public float frequency = 2;
    public float skinThickness = 0.1f;
    [Range(0, 1)]
    public float intensity = 0.5f; //values between 0 and 1
    public bool bounce = false;
    public Transform anchor;
    public Vector3 anchorOffset = Vector3.zero;

    private ArduinoConnector arduino;
    private Vector3 startPosition, offset;
    private bool hitting = false, oldbounce = false;
    private Renderer[] rend;
    private float oldIntensity = 0;

    // Use this for initialization
    void Start () {
        arduino = FindObjectOfType<ArduinoConnector>();
        startPosition = transform.position;
        
        rend = GetComponentsInChildren<Renderer>();

        //if (arduino != null)
        //{
        //    arduino.EnableHapticFeedback(false);
        //    arduino.AdjustHapticFeedback(intensity);
        //    //oldIntensity = intensity;
        //}
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("BallToggle"))
            bounce = !bounce;

        if(oldbounce != bounce)
        {
            ShowBall(bounce);

            if(!bounce && arduino != null)
                arduino.EnableHapticFeedback(false);
            oldbounce = bounce;
        }

        if (oldIntensity != intensity && arduino != null)
        {
            arduino.AdjustHapticFeedback(intensity);
            oldIntensity = intensity;
        }

        if (bounce)
        {
            if (anchor != null)
                startPosition = anchor.position + anchorOffset;

            float ballHeight = Mathf.Abs(Mathf.Sin(frequency * Time.time));
            offset = Vector3.zero;
            offset.y = amplitude * ballHeight;
            transform.position = startPosition + offset;

            if (arduino != null)
            {
                //arduino.AdjustHapticFeedback(intensity);
                if (ballHeight < skinThickness)
                {
                    if (!hitting)
                    {
                        hitting = true;
                        arduino.EnableHapticFeedback(true);
                    }
                    //arduino.AdjustHapticFeedback(intensity - ballHeight);
                }
                else
                {

                    if (hitting)
                    {
                        hitting = false;
                       arduino.EnableHapticFeedback(false);
                    }
                    //arduino.AdjustHapticFeedback(0);
                }
            }
        }
    }

    private void EnableRenderers (bool enable)
    {
        foreach (Renderer r in rend)
            r.enabled = enable;

        if (arduino != null)
            arduino.EnableHapticFeedback(false);
    }

    private void ShowBall (bool show)
    {
        if (rend == null)
            return;

        foreach (Renderer r in rend)
            r.enabled = show;
    }
}
