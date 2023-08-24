using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
[CustomEditor(typeof(Level))]

public class SavedLevelViewer : Editor
{
    public VisualTreeAsset levelUXML;
    public override VisualElement CreateInspectorGUI()
    {
        // Create a new VisualElement to be the root of our inspector UI
        VisualElement myInspector = new VisualElement();

        // Load from default reference
        levelUXML.CloneTree(myInspector);

        // Return the finished inspector UI
        return myInspector;
    }
}
