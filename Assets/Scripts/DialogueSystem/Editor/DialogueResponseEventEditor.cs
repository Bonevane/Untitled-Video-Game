using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(DialogueResponseEvent))]
public class DialogueResponseEventEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DialogueResponseEvent responseEvents = (DialogueResponseEvent)target;

        if(GUILayout.Button("Refresh"))
        {
            responseEvents.OnValidate();
        }
    }
}
