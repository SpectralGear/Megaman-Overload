using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Reflection;

[CustomEditor(typeof(SolarBulletBehaviour))]
public class VFXConditionControllerEditor : Editor {
    public override void OnInspectorGUI() {
        serializedObject.Update();

        SolarBulletBehaviour controller = (SolarBulletBehaviour)target;
        SerializedProperty listProp = serializedObject.FindProperty("conditionalVFX");

        if (GUILayout.Button("Add Condition"))
            listProp.arraySize++;

        for (int i = 0; i < listProp.arraySize; i++) {
            SerializedProperty item = listProp.GetArrayElementAtIndex(i);
            SerializedProperty vfx = item.FindPropertyRelative("targetVFX");
            SerializedProperty fieldName = item.FindPropertyRelative("boolFieldName");
            SerializedProperty reverse = item.FindPropertyRelative("reverseCondition");

            EditorGUILayout.BeginVertical(GUI.skin.box);

            EditorGUILayout.PropertyField(vfx, new GUIContent("Target VFX"));

            string[] boolFieldNames = controller.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(f => f.FieldType == typeof(bool))
                .Select(f => f.Name)
                .ToArray();

            int selected = Mathf.Max(0, System.Array.IndexOf(boolFieldNames, fieldName.stringValue));
            int newSelected = EditorGUILayout.Popup("Bool Field", selected, boolFieldNames);
            fieldName.stringValue = boolFieldNames.Length > 0 ? boolFieldNames[newSelected] : "";

            reverse.boolValue = EditorGUILayout.Toggle("Reverse Condition", reverse.boolValue);

            if (GUILayout.Button("Remove"))
                listProp.DeleteArrayElementAtIndex(i);

            EditorGUILayout.EndVertical();
        }

        serializedObject.ApplyModifiedProperties();
    }
}