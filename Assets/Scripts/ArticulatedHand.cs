namespace SoftHand
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;


	
	public class ArticulatedHand : OVRSkeletonBase
	{
		[SerializeField]
		private bool _applyBoneTranslations = true;
		public IList<OVRBone> Bones { get; protected set; }

		[HideInInspector]
		[SerializeField]
		private List<Transform> _customBones_V2 = new List<Transform>(new Transform[(int)OVRSkeleton.BoneId.Max]);

#if UNITY_EDITOR

		private static readonly string[] _fbxHandSidePrefix = { "l_", "r_" };
		private static readonly string _fbxHandBonePrefix = "b_";

		private static readonly string[] _fbxHandBoneNames =
		{
		"wrist",
		"forearm_stub",
		"thumb0",
		"thumb1",
		"thumb2",
		"thumb3",
		"index1",
		"index2",
		"index3",
		"middle1",
		"middle2",
		"middle3",
		"ring1",
		"ring2",
		"ring3",
		"pinky0",
		"pinky1",
		"pinky2",
		"pinky3"
	};

		private static readonly string[] _fbxHandFingerNames =
		{
		"thumb",
		"index",
		"middle",
		"ring",
		"pinky"
	};
#endif

		public List<Transform> CustomBones { get { return _customBones_V2; } }

#if UNITY_EDITOR
		public void TryAutoMapBonesByName()
		{
			OVRSkeleton.BoneId start = GetCurrentStartBoneId();
			OVRSkeleton.BoneId end = GetCurrentEndBoneId();
			OVRSkeleton.SkeletonType skeletonType = GetSkeletonType();
			if (start != OVRSkeleton.BoneId.Invalid && end != OVRSkeleton.BoneId.Invalid)
			{
				for (int bi = (int)start; bi < (int)end; ++bi)
				{
					string fbxBoneName = FbxBoneNameFromBoneId(skeletonType, (OVRSkeleton.BoneId)bi);
					Transform t = transform.FindChildRecursive(fbxBoneName);

					if (t != null)
					{
						_customBones_V2[(int)bi] = t;
					}
				}
			}
		}

		private static string FbxBoneNameFromBoneId(OVRSkeleton.SkeletonType skeletonType, OVRSkeleton.BoneId bi)
		{
			{
				if (bi >= OVRSkeleton.BoneId.Hand_ThumbTip && bi <= OVRSkeleton.BoneId.Hand_PinkyTip)
				{
					return _fbxHandSidePrefix[(int)skeletonType] + _fbxHandFingerNames[(int)bi - (int)OVRSkeleton.BoneId.Hand_ThumbTip] + "_finger_tip_marker";
				}
				else
				{
					return _fbxHandBonePrefix + _fbxHandSidePrefix[(int)skeletonType] + _fbxHandBoneNames[(int)bi];
				}
			}
		}
#endif

		protected override void InitializeBones()
		{
			bool flipX = (_skeletonType == OVRSkeleton.SkeletonType.HandLeft || _skeletonType == OVRSkeleton.SkeletonType.HandRight);

			if (_bones == null || _bones.Count != _skeleton.NumBones)
			{
				_bones = new List<OVRBone>(new OVRBone[_skeleton.NumBones]);
				Bones = _bones.AsReadOnly();
			}

			for (int i = 0; i < _bones.Count; ++i)
			{
				OVRBone bone = _bones[i] ?? (_bones[i] = new OVRBone());
				bone.Id = (OVRSkeleton.BoneId)_skeleton.Bones[i].Id;
				bone.ParentBoneIndex = _skeleton.Bones[i].ParentBoneIndex;
				bone.Transform = _customBones_V2[(int)bone.Id];

				if (_applyBoneTranslations)
				{
					bone.Transform.localPosition = flipX ? _skeleton.Bones[i].Pose.Position.FromFlippedXVector3f() : _skeleton.Bones[i].Pose.Position.FromFlippedZVector3f();
				}

				bone.Transform.localRotation = flipX ? _skeleton.Bones[i].Pose.Orientation.FromFlippedXQuatf() : _skeleton.Bones[i].Pose.Orientation.FromFlippedZQuatf();
			}
		}
	}
}
