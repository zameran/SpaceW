using System;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
public class SpaceAssembly : Attribute
{
    public string Name;
    public string Version;

    public SpaceAssembly(string Name, string Version)
    {
        this.Name = Name;

        this.Version = Version;
    }
}