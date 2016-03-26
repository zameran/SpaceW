using UnityEngine;
using UnityEngine.SceneManagement;

public class UserInterface : MonoBehaviour
{
    public GameObject Controllable;

    #region API

    public void FreezeTime()
    {
        Time.timeScale = 0.0000001f;
    }

    public void UnFreezeTime()
    {
        Time.timeScale = 1.0f;
    }

    public void LoadGameScene()
    {
        LoadScene(2);
    }

    public void LoadMenuScene()
    {
        LoadScene(1);
    }

    public void LoadScene(int id)
    {
        SceneManager.LoadScene(id);
    }

    public void Quit()
    {
        Application.Quit();
    }

    #endregion

    private void OnLevelWasLoaded(int level)
    {
        UnFreezeTime();
    }
}