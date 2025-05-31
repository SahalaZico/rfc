using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(UIButtonCell))]
public class UIButtonCellEditor : ButtonEditor
{
    SerializedProperty idCell;
    SerializedProperty gameChip;
    SerializedProperty imageChip;
    SerializedProperty textChip;
    SerializedProperty imageHighlight;

    protected override void OnEnable()
    {
        base.OnEnable();
        idCell = serializedObject.FindProperty("idCell");
        gameChip = serializedObject.FindProperty("gameChip");
        imageChip = serializedObject.FindProperty("imageChip");
        textChip = serializedObject.FindProperty("textChip");
        imageHighlight = serializedObject.FindProperty("imageHighlight");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Cell Fields", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(idCell);
        EditorGUILayout.PropertyField(gameChip);
        EditorGUILayout.PropertyField(imageChip);
        EditorGUILayout.PropertyField(imageHighlight);
        EditorGUILayout.PropertyField(textChip);
        serializedObject.ApplyModifiedProperties();
    }
}
