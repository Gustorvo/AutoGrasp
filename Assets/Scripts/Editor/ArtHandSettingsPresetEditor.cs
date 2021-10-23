using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SoftHand
{
    [CustomEditor(typeof(ArticulatedHandSettingsPreset))]
    public class ArtHandSettingsPresetEditor : Editor
    {
        SerializedProperty fingers;

        private void OnEnable()
        {
            fingers = serializedObject.FindProperty("fg");
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var script = (ArticulatedHandSettingsPreset)target;
            if (!script.initialized)
            {
                script.Init();
            } 
            
            if (!script.showExperimentalSettings)
                return;

            if (GUILayout.Button("Create joints", GUILayout.Height(30), GUILayout.MaxWidth(200)))
            {
                script.Init();
                script.CreateJoints();
            }

            bool limitPresetEmpty = script.limitPreset == null;
            GUI.enabled = !limitPresetEmpty;
            if (GUILayout.Button("Set custom joint settings", GUILayout.Height(30), GUILayout.MaxWidth(200)))
            {
                script.SetupCustom();
            }

            if (GUILayout.Button("Fetch joint limits from preset", GUILayout.Height(30), GUILayout.MaxWidth(200)))
            {
                script.SetJointLimits();
            }
            GUI.enabled = true;

            if (GUILayout.Button("Set all drives to 'Spherical & Free Motion'", GUILayout.Height(30), GUILayout.MaxWidth(250)))
            {
                script.SetToFreeMotion();
            }
            if (GUILayout.Button("Set global motor settings", GUILayout.Height(30), GUILayout.MaxWidth(200)))
            {
                script.SetGlobalMotorSettings();
            }
            if (GUILayout.Button("Set all to Revolute", GUILayout.Height(30), GUILayout.MaxWidth(200)))
            {
                script.SetAllDrivesToRevolute();
            }

            if (GUILayout.Button("Set anchor rotation to 0 on Spherical", GUILayout.Height(30), GUILayout.MaxWidth(250)))
            {
                script.ResetAnchorRotaionOnSphericalJoints();
            }


            if (limitPresetEmpty)
                EditorGUILayout.HelpBox("Limit preset is not specified!", MessageType.Warning);


        }

        
    }
}