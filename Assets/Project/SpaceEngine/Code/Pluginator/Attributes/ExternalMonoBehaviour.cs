using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class ExternalMonoBehaviour : Attribute
{
    public StartupEnum Startup;

    public bool StartupOnce;

    public ExternalMonoBehaviour(StartupEnum Startup, bool StartupOnce)
    {
        this.Startup = Startup;

        this.StartupOnce = StartupOnce;
    }

    public enum StartupEnum
    {
        MainMenu = 0,
        MainScene = 1
    }
}