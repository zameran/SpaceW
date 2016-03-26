using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class SpaceAddonMonoBehaviour : Attribute
{
    public EntryPoint @EntryPoint;

    public bool StartupOnce;

    public SpaceAddonMonoBehaviour(EntryPoint @EntryPoint, bool StartupOnce)
    {
        this.EntryPoint = EntryPoint;

        this.StartupOnce = StartupOnce;
    }
}