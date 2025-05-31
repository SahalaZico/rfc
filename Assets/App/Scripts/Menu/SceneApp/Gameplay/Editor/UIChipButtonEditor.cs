using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(UIChipButton))]
public class UIChipButtonEditor : ButtonEditor
{
    SerializedProperty selectedChip;
    SerializedProperty amountChip;
    SerializedProperty gameChip;
    SerializedProperty skinChip;
    SerializedProperty textChip;

    protected override void OnEnable()
    {
        base.OnEnable();
        selectedChip = serializedObject.FindProperty("selectedChip");
        amountChip = serializedObject.FindProperty("amountChip");
        gameChip = serializedObject.FindProperty("gameChip");
        skinChip = serializedObject.FindProperty("skinChip");
        textChip = serializedObject.FindProperty("textChip");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Chip Fields", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(amountChip);
        EditorGUILayout.PropertyField(gameChip);
        EditorGUILayout.PropertyField(selectedChip);
        EditorGUILayout.PropertyField(skinChip);
        EditorGUILayout.PropertyField(textChip);
        serializedObject.ApplyModifiedProperties();
    }
}
