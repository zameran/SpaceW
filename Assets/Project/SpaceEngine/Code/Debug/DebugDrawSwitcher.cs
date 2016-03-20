using UnityEngine;

public class DebugDrawSwitcher : MonoBehaviour, IDebugSwitcher
{
    public DebugDraw[] GUIs;

    public int state = 0;

    private void Start()
    {
        ToogleAll(GUIs, false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F6))
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

    public void Toogle(DebugDraw GUI, bool state)
    {
        GUI.enabled = state;
    }

    public void ToogleAt(DebugDraw[] GUIs, bool state, int index)
    {
        GUIs[index - 1].enabled = state;
    }

    public void ToogleAll(DebugDraw[] GUIs, bool state)
    {
        for (int i = 0; i < GUIs.Length; i++)
        {
            GUIs[i].enabled = false;
        }
    }
}