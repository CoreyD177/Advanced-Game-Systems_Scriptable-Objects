using System;
using Variable;
using UnityEngine;
using UnityEditor;

namespace References
{
    #region Basic
    [CustomPropertyDrawer(typeof(Bool))]
    [CustomPropertyDrawer(typeof(Char))]
    [CustomPropertyDrawer(typeof(Double))]
    [CustomPropertyDrawer(typeof(Float))]
    [CustomPropertyDrawer(typeof(Int16))]
    [CustomPropertyDrawer(typeof(Int))]
    [CustomPropertyDrawer(typeof(Int64))]
    [CustomPropertyDrawer(typeof(String))]
    #endregion
    #region  Struct
    [CustomPropertyDrawer(typeof(Quaternion))]
    [CustomPropertyDrawer(typeof(Vector2))]
    [CustomPropertyDrawer(typeof(Vector3))]
    #endregion
    #region References
    [CustomPropertyDrawer(typeof(GameObject))]
    [CustomPropertyDrawer(typeof(Transform))]
    [CustomPropertyDrawer(typeof(AnimationCurve))]
    [CustomPropertyDrawer(typeof(Mesh))]
    [CustomPropertyDrawer(typeof(Collider))]
    [CustomPropertyDrawer(typeof(RigidBody))]
    [CustomPropertyDrawer(typeof(Gradient))]
    [CustomPropertyDrawer(typeof(CharacterController))]
    #endregion
    public class ReferenceDrawer : PropertyDrawer
    {
        private readonly string[] _popupOptions = { "Use Constant", "Use Variable" };
        private GUIStyle _popupStyle;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //base.OnGUI(position, property, label);
            if (_popupStyle == null)
            {
                _popupStyle = new GUIStyle(GUI.skin.GetStyle("PaneOptions"));
                _popupStyle.imagePosition = ImagePosition.ImageOnly;
            }
            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);

            EditorGUI.BeginChangeCheck();
            SerializedProperty useConstant = property.FindPropertyRelative("useConstant");
            SerializedProperty constantValue = property.FindPropertyRelative("_constantValue");
            SerializedProperty variable = property.FindPropertyRelative("_variable");

            Rect buttonRect = new Rect(position);
            buttonRect.yMin += _popupStyle.margin.top;
            buttonRect.width = _popupStyle.fixedWidth + _popupStyle.margin.right;
            position.xMin = buttonRect.xMax;

            int _indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            int _result = EditorGUI.Popup(buttonRect, useConstant.boolValue ? 0 : 1, _popupOptions, _popupStyle);
            useConstant.boolValue = _result == 0;
            EditorGUI.PropertyField(position, useConstant.boolValue ? constantValue : variable, GUIContent.none);
            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
            }
            EditorGUI.indentLevel = _indent;
            EditorGUI.EndProperty();
        }
    }
}
