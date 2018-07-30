using Leap.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.Attributes;

public class HandModelSwitcher : MonoBehaviour {
    private HandModelManager manager;
    private int activeModel = 0;

    // Use this for initialization
    void Start () {
        manager = GetComponent<HandModelManager>();

        if (manager == null)
        {
            Debug.LogWarning("HandModelSwitcher:  No HandModelManager component found on GameObject. Will not switch through Leap hand models.");
            return;
        }

        UpdateHands();
    }
	
	// Update is called once per frame
	void Update () {
        if (manager == null)
            return;

        if(Input.GetButtonDown("NextHand"))
        {
            activeModel++;
            if (activeModel >= manager.GetHandPoolSize())
                activeModel = 0;
            UpdateHands();
        }


        if (Input.GetButtonDown("PrevHand"))
        {
            activeModel--;
            if (activeModel < 0)
                activeModel = manager.GetHandPoolSize() - 1;
            UpdateHands();
        }
    }

    private void UpdateHands ()
    {
        for(int i = 0; i < manager.GetHandPoolSize(); i++)
        {
            if (i == activeModel)
                manager.EnableGroup(manager.GetHandModelName(i));
            else
                manager.DisableGroup(manager.GetHandModelName(i));
        }
    }
}
