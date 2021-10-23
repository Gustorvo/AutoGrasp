using System.Collections.Generic;
using SoftHand;
using UnityEditor;
using UnityEngine;

namespace SoftHand.Experimental
{
    // TODO: Make the window to update its values continuesly (in play mode)
    // TODO: Add bone names and propper spacings

    public class ArtBodyTargetControllerEditor : EditorWindow
    {
        ArticulatedHand _articulatedHand = null;
        public GameObject artHandGO;



        [MenuItem("Articulated Hand/Drive target visualizer")]
        static void Init()
        {
            EditorWindow window = GetWindow(typeof(ArtBodyTargetControllerEditor));
            window.Show();
        }

        void OnGUI()
        {
            // if (EditorApplication.isPlayingOrWillChangePlaymode)
            //    return;
            EditorGUILayout.BeginHorizontal();
            artHandGO = (GameObject)EditorGUILayout.ObjectField(artHandGO, typeof(GameObject), true);
            if (artHandGO == null)
                return;
            EditorGUILayout.EndHorizontal();
            if (artHandGO.TryGetComponent(out ArticulatedHand hand))
                _articulatedHand = hand;
            else EditorGUILayout.HelpBox("GameObject has no ArtBodyTargetController attached!", MessageType.Warning);

            if (_articulatedHand.JointBodies == null || _articulatedHand.JointBodies.Length == 0)
            {
                EditorGUILayout.HelpBox("Articulation drives not available in editor mode! It needs to be initialized in play mode", MessageType.Warning);
            }
            else
            {
                Fetch();
            }
        }

        private void Fetch()
        {
            for (int i = 0; i < _articulatedHand.JointBodies.Length; ++i)
            {
                ArticulationBody body = _articulatedHand.JointBodies[i];
                int dofs = body.dofCount;
                if (body.twistLock == ArticulationDofLock.LimitedMotion)
                    MakeDriveSlider(i, "x", body.xDrive.target, body.xDrive.upperLimit, body.xDrive.lowerLimit);
                if (body.swingYLock == ArticulationDofLock.LimitedMotion)
                    MakeDriveSlider(i, "y", body.yDrive.target, body.yDrive.upperLimit, body.yDrive.lowerLimit);
                if (body.swingZLock == ArticulationDofLock.LimitedMotion)
                    MakeDriveSlider(i, "z", body.zDrive.target, body.zDrive.upperLimit, body.zDrive.lowerLimit);
            }
            // EditorUtility.SetDirty(_articulatedHand);
            // EditorSceneManager.MarkSceneDirty(_articulatedHand.gameObject.scene);

        }

        private void MakeDriveSlider(int id, string axis, float degrees, float maxDgr, float minDgr)
        {
            GUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(id.ToString(), axis);
            EditorGUILayout.Slider(degrees, minDgr, maxDgr);

            GUILayout.EndHorizontal();
        }
    }
}