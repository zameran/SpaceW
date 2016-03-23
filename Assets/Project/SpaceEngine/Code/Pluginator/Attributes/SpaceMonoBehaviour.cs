using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class SpaceMonoBehaviour : Attribute
{
    public StartupEnum Startup;

    public bool StartupOnce;

    public SpaceMonoBehaviour(StartupEnum Startup, bool StartupOnce)
    {
        this.Startup = Startup;

        this.StartupOnce = StartupOnce;
    }

    public enum StartupEnum : int
    {
        Init = 0,
        MainMenu = 1,
    }
}