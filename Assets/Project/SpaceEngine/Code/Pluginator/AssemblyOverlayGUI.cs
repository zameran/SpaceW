using System;
using UnityEngine;

public sealed class AssemblyOverlayGUI : MonoBehaviour
{
    public AssemblyLoader loader = null;

    private string addonList;

    private Rect position = new Rect(0.0f, 0.0f, 384.0f, 0.0f);
    private Vector2 scrollPosition;

    private bool showAddons;
    private bool useScrollView;

    private GUIStyle windowStyle;
    private GUIStyle labelGreen;
    private GUIStyle labelYellow;

    public Texture2D backgroundTex;

    private void Awake()
    {
        if (loader == null) loader = AssemblyLoader.Instance as AssemblyLoader;
    }

    private void OnGUI()
    {
        if (loader == null) return;

        this.position = GUILayout.Window(this.GetInstanceID(), this.position, this.Window, String.Empty, this.windowStyle);
        this.CheckScrollViewUsage();
    }

    private void Start()
    {
        this.InitialiseStyles();
    }

    private void Update()
    {

    }

    private void CheckScrollViewUsage()
    {
        if (this.position.height < Screen.height * 0.5f || this.useScrollView)
        {
            return;
        }

        this.useScrollView = true;
        this.position.height = Screen.height * 0.5f;
    }

    private void DrawAddonBoxEnd()
    {
        if (this.useScrollView)
        {
            GUILayout.EndScrollView();
        }
        else
        {
            GUILayout.EndVertical();
        }
    }

    private void DrawAddonBoxStart()
    {
        if (this.useScrollView)
        {
            this.scrollPosition = GUILayout.BeginScrollView(this.scrollPosition, GUILayout.Height(Screen.height * 0.5f));
        }
        else
        {
            GUILayout.BeginVertical(GUI.skin.scrollView);
        }
    }

    private void DrawAddonList()
    {
        this.DrawAddonBoxStart();
        this.DrawAddons();
        this.DrawAddonBoxEnd();
    }

    private void DrawAddons()
    {
        this.addonList = String.Empty;

        foreach (var addon in loader.ExternalAssemblies)
        {
            var nameStyle = this.labelGreen;
            var versionStyle = this.labelYellow;

            this.addonList += Environment.NewLine + addon.Name + " - " + addon.Version;

            GUILayout.BeginHorizontal();
            GUILayout.Label(addon.Name, nameStyle);

            GUILayout.FlexibleSpace();

            GUILayout.Label(addon.Version, versionStyle);
            GUILayout.EndHorizontal();
        }
    }

    private void InitialiseStyles()
    {
        this.windowStyle = new GUIStyle
        {
            normal =
            {
                background = backgroundTex
            },
            border = new RectOffset(3, 3, 20, 3),
            padding = new RectOffset(10, 10, 1, 5)
        };

        this.labelGreen = new GUIStyle
        {
            normal =
            {
                textColor = Color.green
            }
        };

        this.labelYellow = new GUIStyle
        {
            normal =
            {
                textColor = Color.yellow
            }
        };
    }

    private void Window(int windowId)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Loaded Add-ons: " + loader.TotalDetected);
        GUILayout.FlexibleSpace();

        if (GUILayout.Toggle(this.showAddons, "Show Add-ons") != this.showAddons)
        {
            this.showAddons = !this.showAddons;
            this.position.height = 0.0f;
        }

        GUILayout.EndHorizontal();

        if (this.showAddons)
        {
            this.DrawAddonList();
        }
    }
}