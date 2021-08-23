using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panel : MonoBehaviour
{
    private Canvas canvas = null;
    private Menu menu = null;


    private void Awake()
    {
        canvas = GetComponent<Canvas>();
    }
    public void Setup(Menu menu)
    {
        this.menu = menu;
        hide();
    }
    public void show()
    {
        canvas.enabled = true;
    }
    public void hide()
    {
        canvas.enabled = false;
    }
}
