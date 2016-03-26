using UnityEngine;

public class InGamePauseMenuUI : UserInterface, IUserInterface
{
    private bool Paused = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Paused = !Paused;

            if (Paused)
                FreezeTime();
            else
                UnFreezeTime();
        }

        Controllable.SetActive(Paused);
    }
}