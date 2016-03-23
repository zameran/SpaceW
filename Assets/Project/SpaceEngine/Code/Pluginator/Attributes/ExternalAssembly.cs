using System;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
public class ExternalAssembly : Attribute
{
    public string Name;
    public string Version;

    public ExternalAssembly(string Name, string Version)
    {
        this.Name = Name;

        this.Version = Version;
    }
}