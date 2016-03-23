using System;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
public class ExternalAssembly : Attribute
{
    public string Name;

    public Version Version;

    public ExternalAssembly(string Name, Version Version)
    {
        this.Name = Name;

        this.Version = Version;
    }
}