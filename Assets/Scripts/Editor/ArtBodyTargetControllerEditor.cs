// Editor script that lets you scale the selected GameObject between 1 and 100

using System.Collections.Generic;
using SoftHand;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using static ArtDrive;
using static OVRSkeleton;

public class ArtBodyTargetControllerEditor : EditorWindow
{    
    ArtBodyTargetController _driveController = null;
    List<float> _sliderValues = new List<float>();
    public GameObject artDriveSource;

   


    [MenuItem("Examples/Art Body Target Controller")]
    static void Init()
    {
        EditorWindow window = GetWindow(typeof(ArtBodyTargetControllerEditor));
        window.Show();
    }

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        artDriveSource = (UnityEngine.GameObject)EditorGUILayout.ObjectField(artDriveSource, typeof(GameObject), true);
        if (artDriveSource == null)
            return;
        EditorGUILayout.EndHorizontal();
        if (artDriveSource.TryGetComponent<ArtBodyTargetController>(out ArtBodyTargetController controller))
            _driveController = controller;
        else EditorGUILayout.HelpBox("GameObject has no ArtBodyTargetController attached!", MessageType.Warning);

        if (_driveController.drives.Count == 0)
        {
            EditorGUILayout.HelpBox("Articulation drives not fetched!", MessageType.Warning);
            Fetch();
        }
        else
        {
            Fetch();
            //_sliderValues = new List<float>();
            {
                for (int i = 0; i < _driveController.driveTargets.Count; ++i)
                {
                    MakeDriveSlider(i, _driveController.driveTargets[i]);
                }
            }
        }
    }

    private void Fetch()
    {
        if (GUILayout.Button("Fetch drives"))
        {
            _driveController.Awake();
            EditorUtility.SetDirty(_driveController);
            EditorSceneManager.MarkSceneDirty(_driveController.gameObject.scene);
        }
    }

    private void InitSliderValues()
    {
        if (_sliderValues.Count == 0)
            _driveController.rootBody.GetDriveTargets(_sliderValues);

    }

    private void MakeDriveSlider(int id, ArtDriveTarget artDriveTarget)
    {        
        float val = _driveController.driveTargetValues[id];
        GUILayout.BeginHorizontal();

        EditorGUILayout.LabelField(id.ToString(), artDriveTarget.Name);
        _driveController.driveTargetValues[id] = EditorGUILayout.Slider(val, artDriveTarget.LowerLimit, artDriveTarget.UpperLimit);
       
        GUILayout.EndHorizontal();
    }

    void OnInspectorUpdate()
    {        
        if (_driveController != null && _driveController.driveTargets.Count > 0)
        {
            for (int i = 0; i < _driveController.driveTargets.Count; i++)
            {
                var drive = _driveController.driveTargets[i];
                float sliderValue = _driveController.driveTargetValues[i];
                float driveValue = _driveController.GetDriveValue(drive.DriveType, drive.InstanceId);

                if (Mathf.Abs(sliderValue - driveValue) > float.Epsilon)
                    _driveController.SetDriveValue(drive.DriveType, drive.InstanceId, sliderValue);
            }
        }

    }
}