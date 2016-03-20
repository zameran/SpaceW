using System.Collections.Generic;

using UnityEngine;

public class DebugGUILeaks : DebugGUI
{
    private Vector2 ScrollPosition;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnGUI()
    {
        base.OnGUI();

        GUILayout.BeginArea(debugInfoBounds);

        ScrollPosition = GUILayout.BeginScrollView(ScrollPosition, false, true, GUILayout.Width(250), GUILayout.Height(250));
        {
            Object[] objects = FindObjectsOfType(typeof(UnityEngine.Object));

            Dictionary<string, int> dictionary = new Dictionary<string, int>();

            foreach (Object obj in objects)
            {
                string key = obj.GetType().ToString();

                if (dictionary.ContainsKey(key))
                {
                    dictionary[key]++;
                }
                else
                {
                    dictionary[key] = 1;
                }
            }

            List<KeyValuePair<string, int>> myList = new List<KeyValuePair<string, int>>(dictionary);

            myList.Sort(delegate(KeyValuePair<string, int> firstPair, KeyValuePair<string, int> nextPair)
            {
                return nextPair.Value.CompareTo((firstPair.Value));
            });

            foreach (KeyValuePair<string, int> entry in myList)
            {
                GUILayoutExtensions.LabelWithSpace(entry.Key + " : " + entry.Value, -10);
            }

        }

        GUILayout.EndScrollView();

        GUILayout.EndArea();
    }
}