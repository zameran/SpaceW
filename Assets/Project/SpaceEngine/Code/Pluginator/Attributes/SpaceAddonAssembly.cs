using System;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
public sealed class SpaceAddonAssembly : Attribute
{
    public string Name;
    public string Version;

    public SpaceAddonAssembly(string Name, string Version)
    {
        this.Name = Name;

        this.Version = Version;
    }
}