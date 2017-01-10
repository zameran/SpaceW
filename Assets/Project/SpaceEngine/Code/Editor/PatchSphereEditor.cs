using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PatchSphere))]
class PatchSphereEditor : Editor
{
    private void OnSceneGUI()
    {
        PatchSphere p = (PatchSphere)target;

        p.CallUpdate();
    }

    public override void OnInspectorGUI()
    {
        PatchSphere p = (PatchSphere)target;

        DrawDefaultInspector();

        if (GUI.changed)
        {
            p.Rebuild();
        }
    }
}