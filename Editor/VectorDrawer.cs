using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UnityExtensions
{

    [CustomPropertyDrawer(typeof(VectorAttribute))]
    public sealed class VectorDrawer : PropertyDrawer
    {
        private int elementCount;

        private float elementLabelWidth;

        public override bool CanCacheInspectorGUI(
            SerializedProperty property)
        {
            return true;
        }

        public override float GetPropertyHeight(
            SerializedProperty property,
            GUIContent label)
        {
            if (elementLabelWidth == 0)
                ResolveElementMeasurements(property);

            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            var spacing = EditorGUIUtility.standardVerticalSpacing;

            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);
            position.xMin -= 1;

            using (IndentLevelScope())
            using (LabelWidthScope(elementLabelWidth))
            {

                var elementWidth =
                    (position.width - (elementCount - 1) * spacing)
                    / elementCount;

                position.width = elementWidth;

                EditorGUI.indentLevel = 0;

                foreach (var element in EnumerateChildProperties(property))
                {
                    EditorGUI.PropertyField(position, element);
                    position.x += elementWidth + spacing;
                }
            }

            EditorGUI.EndProperty();
        }

        //----------------------------------------------------------------------

        private void ResolveElementMeasurements(
            SerializedProperty property)
        {
            elementLabelWidth = 13;

            var label = new GUIContent();
            var labelStyle = EditorStyles.label;

            foreach (var element in EnumerateChildProperties(property))
            {
                label.text = element.displayName;
                var labelWidth = labelStyle.CalcSize(label).x;
                elementCount += 1;
                elementLabelWidth = Mathf.Max(elementLabelWidth, labelWidth);
            }
        }


        //----------------------------------------------------------------------

        private static IEnumerable<SerializedProperty>
        EnumerateChildProperties(SerializedProperty parentProperty)
        {
            var parentPropertyPath = parentProperty.propertyPath;
            var iterator = parentProperty.Copy();
            var end = iterator.GetEndProperty();
            if (iterator.NextVisible(enterChildren: true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(iterator, end))
                        yield break;

                    yield return iterator;
                }
                while (iterator.NextVisible(enterChildren: false));
            }
        }

        //----------------------------------------------------------------------

        private struct Deferred : IDisposable
        {
            private readonly Action _onDispose;

            public Deferred(Action onDispose)
            {
                _onDispose = onDispose;
            }

            public void Dispose()
            {
                if (_onDispose != null)
                    _onDispose();
            }
        }

        private static Deferred IndentLevelScope()
        {
            var oldIndentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            return new Deferred(() =>
                EditorGUI.indentLevel = 0
            );
        }

        private static Deferred LabelWidthScope(float labelWidth)
        {
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = labelWidth;
            return new Deferred(() =>
                EditorGUIUtility.labelWidth = oldLabelWidth
            );
        }

    }

}