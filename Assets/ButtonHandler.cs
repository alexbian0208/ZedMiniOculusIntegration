using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour
{

    public void SetButton(string text)
    {
        ///if (OVRInput.GetDown(OVRInput.Button.One))
        ///{
            Text txt = transform.Find("Text").GetComponent<Text>();
            txt.text = text;
        ///}


    }
}
