using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class SpaceAddonMonoBehaviour : Attribute
{
    public StartupEnum Startup;

    public bool StartupOnce;

    public SpaceAddonMonoBehaviour(StartupEnum Startup, bool StartupOnce)
    {
        this.Startup = Startup;

        this.StartupOnce = StartupOnce;
    }
}