using System;
using System.Reflection;

public sealed class AssemblyExternal
{
    public string Path;
    public string Name;
    public string Version;

    public Assembly Assembly;

    public AssemblyExternalTypes Types;

    public AssemblyExternal(string Path, string Name, string Version, Assembly Assembly, AssemblyExternalTypes Types)
    {
        this.Path = Path;
        this.Name = Name;
        this.Version = Version;

        this.Assembly = Assembly;

        this.Types = Types;
    }
}