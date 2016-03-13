using UnityEngine;

public class DebugGUISwitcher : MonoBehaviour
{
    public DebugGUI[] GUIs;

    public int state = 0;

    private void Start()
    {
        ToogleAll(GUIs, false);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.F5))
        {
            if (state == GUIs.Length)
            {
                state = 0;
                ToogleAll(GUIs, false);
                return;
            }

            ToogleAll(GUIs, false);
            state++;
            ToogleAt(GUIs, true, state);
        }
    }

    public void Toogle(DebugGUI GUI, bool state)
    {
        GUI.enabled = state;
    }

    public void ToogleAt(DebugGUI[] GUIs, bool state, int index)
    {
        GUIs[index - 1].enabled = state;
    }

    public void ToogleAll(DebugGUI[] GUIs, bool state)
    {
        for (int i = 0; i < GUIs.Length; i++)
        {
            GUIs[i].enabled = false;
        }
    }
}