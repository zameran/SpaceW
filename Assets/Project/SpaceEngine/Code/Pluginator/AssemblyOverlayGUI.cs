using UnityEngine;

public class AssemblyOverlayGUI : MonoBehaviour
{
    public AssemblyLoader loader = null;

    private void Start()
    {
        if (loader == null) loader = AssemblyLoader.Instance;
    }

    private void OnGUI()
    {
        AssembiesList();
    }

    private void AssembiesList()
    {
        if (loader == null || loader.ExternalAssemblies.Count == 0 || loader.ExternalAssemblies == null) return;

        for (int i = 0; i < loader.ExternalAssemblies.Count; i++)
        {
            AssemblyExternal assembly = loader.ExternalAssemblies[i];

            GUILayoutExtensions.Horizontal(() => 
            {
                GUILayout.Label(assembly.Name);
                GUILayout.FlexibleSpace();
                GUILayout.Label(assembly.Version);
            });
        }
    }
}