using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using SoftHand;
using static SoftHand.Enums;

[CustomEditor(typeof(SkeletonMapping))]
public class SkeletonMappingEditor : Editor
{
    public new void OnInspectorGUI()
    {
        DrawPropertiesExcluding(serializedObject, new string[] { "_customBones" });
        serializedObject.ApplyModifiedProperties();

        SkeletonMapping skeleton = (SkeletonMapping)target;
        Handedness skeletonType = skeleton.SkeletonType;

        if (skeletonType == Handedness.None)
        {
            EditorGUILayout.HelpBox("Please select a SkeletonType.", MessageType.Warning);
        }
        else
        {
            if (GUILayout.Button("Auto Map Bones"))
            {
                skeleton.TryAutoMapBonesByName();
                EditorUtility.SetDirty(skeleton);
                EditorSceneManager.MarkSceneDirty(skeleton.gameObject.scene);
            }
            if (skeleton.IsInitialized)
            {
                if (GUILayout.Button("Add/Reset capsules"))
                {
                    skeleton.InitializeCapsules();
                    EditorUtility.SetDirty(skeleton);
                    EditorSceneManager.MarkSceneDirty(skeleton.gameObject.scene);
                }
                if (GUILayout.Button("Add/Reset Articulation bodies"))
                {
                    skeleton.InitializeArticulationBodies();
                    EditorUtility.SetDirty(skeleton);
                    EditorSceneManager.MarkSceneDirty(skeleton.gameObject.scene);
                }
            }

            EditorGUILayout.LabelField("Bones", EditorStyles.boldLabel);
            BoneId start = skeleton.CurrentStartBoneId;
            BoneId end = skeleton.CurrentEndBoneId;
            if (start != BoneId.Invalid && end != BoneId.Invalid)
            {
                for (int i = (int)start; i < (int)end; ++i)
                {
                    string boneName = BoneLabelFromBoneId(skeletonType, (BoneId)i);
                    skeleton.CustomBones[i] = (Transform)EditorGUILayout.ObjectField(boneName, skeleton.CustomBones[i], typeof(Transform), true);
                }
            }
        }
    }

    // force aliased enum values to the more appropriate value
    private static string BoneLabelFromBoneId(Handedness skeletonType, BoneId boneId)
    {
        if (skeletonType != Handedness.None)
        {
            switch (boneId)
            {
                case BoneId.Hand_WristRoot:
                    return "Hand_WristRoot";
                case BoneId.Hand_ForearmStub:
                    return "Hand_ForearmStub";
                case BoneId.Hand_Thumb0:
                    return "Hand_Thumb0";
                case BoneId.Hand_Thumb1:
                    return "Hand_Thumb1";
                case BoneId.Hand_Thumb2:
                    return "Hand_Thumb2";
                case BoneId.Hand_Thumb3:
                    return "Hand_Thumb3";
                case BoneId.Hand_Index1:
                    return "Hand_Index1";
                case BoneId.Hand_Index2:
                    return "Hand_Index2";
                case BoneId.Hand_Index3:
                    return "Hand_Index3";
                case BoneId.Hand_Middle1:
                    return "Hand_Middle1";
                case BoneId.Hand_Middle2:
                    return "Hand_Middle2";
                case BoneId.Hand_Middle3:
                    return "Hand_Middle3";
                case BoneId.Hand_Ring1:
                    return "Hand_Ring1";
                case BoneId.Hand_Ring2:
                    return "Hand_Ring2";
                case BoneId.Hand_Ring3:
                    return "Hand_Ring3";
                case BoneId.Hand_Pinky0:
                    return "Hand_Pinky0";
                case BoneId.Hand_Pinky1:
                    return "Hand_Pinky1";
                case BoneId.Hand_Pinky2:
                    return "Hand_Pinky2";
                case BoneId.Hand_Pinky3:
                    return "Hand_Pinky3";
                case BoneId.Hand_ThumbTip:
                    return "Hand_ThumbTip";
                case BoneId.Hand_IndexTip:
                    return "Hand_IndexTip";
                case BoneId.Hand_MiddleTip:
                    return "Hand_MiddleTip";
                case BoneId.Hand_RingTip:
                    return "Hand_RingTip";
                case BoneId.Hand_PinkyTip:
                    return "Hand_PinkyTip";
                default:
                    return "Hand_Unknown";
            }
        }
        else
        {
            return "Skeleton_Unknown";
        }
    }
}

