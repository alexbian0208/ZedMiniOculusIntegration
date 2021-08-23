using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    public Panel currentPanel = null;
    private List<Panel> PanelHistory = new List<Panel>();

    private void Start()
    {
        SetupPanel();
    }

    private void SetupPanel()
    {
        Panel[] panels = GetComponentsInChildren<Panel>();

        for (int i = 0; i < panels.Length; i++)
        {
            Panel panel = panels[i];
            panel.Setup(this);
        }

        currentPanel.show();
    }

    public void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.One))
            GoToPrevious();
    }

    public void GoToPrevious()
    {
        if(PanelHistory.Count == 0)
        {
            return;
        }
        int lastIdx = PanelHistory.Count - 1;
        SetCurrent(PanelHistory[lastIdx]);
        PanelHistory.RemoveAt(lastIdx);
    }
    public void SetCurrentWithHistory(Panel newpanel)
    {
        PanelHistory.Add(currentPanel);
        SetCurrent(newpanel);
    }
    private void SetCurrent(Panel newpanel)
    {
        currentPanel.hide();
        currentPanel = newpanel;
        currentPanel.show();
    }
}
