using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using Sturfee.XRCS.Config;
using System;

namespace Sturfee.XRCS.Editor
{
    public static class Setup
    {
        [InitializeOnLoadMethod]
        public static void InstallLayers()
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");

            Type type = typeof(XrLayers);
            foreach (var layer in type.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public))
            {
                var layerValue = layer.GetValue(null).ToString();
                bool existLayer = false;
                for (int i = 8; i < layers.arraySize; i++)
                {
                    SerializedProperty layerSp = layers.GetArrayElementAtIndex(i);
                    if (layerSp.stringValue == layerValue)
                    {
                        existLayer = true;
                        break;
                    }
                }
                for (int j = 8; j < layers.arraySize; j++)
                {
                    SerializedProperty layerSP = layers.GetArrayElementAtIndex(j);
                    if (layerSP.stringValue == "" && !existLayer)
                    {
                        layerSP.stringValue = layerValue;
                        tagManager.ApplyModifiedProperties();

                        break;
                    }
                }
            }
        }
    }
}