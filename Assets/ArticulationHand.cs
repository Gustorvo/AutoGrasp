using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static OVRSkeleton;

[RequireComponent(typeof(OVRSkeleton))]
[RequireComponent(typeof(OVRHand))]
public class ArticulationHand : MonoBehaviour
    
{
	[SerializeField]
	protected SkeletonType _skeletonType = SkeletonType.None;
	protected OVRPlugin.Skeleton2 _skeleton = new OVRPlugin.Skeleton2();
	private OVRHand _hand = null;   
    private OVRSkeleton.IOVRSkeletonDataProvider _dataProvider;
   

    public bool IsInitialized { get; private set; }
    public bool IsDataValid { get; private set; }
    public bool IsDataHighConfidence { get; private set; }

	private void Initialize()
	{
		if (OVRPlugin.GetSkeleton2((OVRPlugin.SkeletonType)_skeletonType, ref _skeleton))
		{
			//InitializeBones();
			//InitializeBindPose();
			//InitializeCapsules();

			IsInitialized = true;
		}
	}

	//protected virtual void InitializeBones()
	//{
	//	bool flipX = (_skeletonType == SkeletonType.HandLeft || _skeletonType == SkeletonType.HandRight);

	//	if (!_bonesGO)
	//	{
	//		_bonesGO = new GameObject("Bones");
	//		_bonesGO.transform.SetParent(transform, false);
	//		_bonesGO.transform.localPosition = Vector3.zero;
	//		_bonesGO.transform.localRotation = Quaternion.identity;
	//	}

	//	if (_bones == null || _bones.Count != _skeleton.NumBones)
	//	{
	//		_bones = new List<OVRBone>(new OVRBone[_skeleton.NumBones]);
	//		Bones = _bones.AsReadOnly();
	//	}

	//	// pre-populate bones list before attempting to apply bone hierarchy
	//	for (int i = 0; i < _bones.Count; ++i)
	//	{
	//		OVRBone bone = _bones[i] ?? (_bones[i] = new OVRBone());
	//		bone.Id = (OVRSkeleton.BoneId)_skeleton.Bones[i].Id;
	//		bone.ParentBoneIndex = _skeleton.Bones[i].ParentBoneIndex;

	//		Transform trans = bone.Transform ?? (bone.Transform = new GameObject(bone.Id.ToString()).transform);
	//		trans.localPosition = flipX ? _skeleton.Bones[i].Pose.Position.FromFlippedXVector3f() : _skeleton.Bones[i].Pose.Position.FromFlippedZVector3f();
	//		trans.localRotation = flipX ? _skeleton.Bones[i].Pose.Orientation.FromFlippedXQuatf() : _skeleton.Bones[i].Pose.Orientation.FromFlippedZQuatf();
	//	}

	//	for (int i = 0; i < _bones.Count; ++i)
	//	{
	//		if ((BoneId)_bones[i].ParentBoneIndex == BoneId.Invalid)
	//		{
	//			_bones[i].Transform.SetParent(_bonesGO.transform, false);
	//		}
	//		else
	//		{
	//			_bones[i].Transform.SetParent(_bones[_bones[i].ParentBoneIndex].Transform, false);
	//		}
	//	}
	//}

	//private void InitializeBindPose()
	//{
	//	if (!_bindPosesGO)
	//	{
	//		_bindPosesGO = new GameObject("BindPoses");
	//		_bindPosesGO.transform.SetParent(transform, false);
	//		_bindPosesGO.transform.localPosition = Vector3.zero;
	//		_bindPosesGO.transform.localRotation = Quaternion.identity;
	//	}

	//	if (_bindPoses == null || _bindPoses.Count != _bones.Count)
	//	{
	//		_bindPoses = new List<OVRBone>(new OVRBone[_bones.Count]);
	//		BindPoses = _bindPoses.AsReadOnly();
	//	}

	//	// pre-populate bones list before attempting to apply bone hierarchy
	//	for (int i = 0; i < _bindPoses.Count; ++i)
	//	{
	//		OVRBone bone = _bones[i];
	//		OVRBone bindPoseBone = _bindPoses[i] ?? (_bindPoses[i] = new OVRBone());
	//		bindPoseBone.Id = bone.Id;
	//		bindPoseBone.ParentBoneIndex = bone.ParentBoneIndex;

	//		Transform trans = bindPoseBone.Transform ?? (bindPoseBone.Transform = new GameObject(bindPoseBone.Id.ToString()).transform);
	//		trans.localPosition = bone.Transform.localPosition;
	//		trans.localRotation = bone.Transform.localRotation;
	//	}

	//	for (int i = 0; i < _bindPoses.Count; ++i)
	//	{
	//		if ((BoneId)_bindPoses[i].ParentBoneIndex == BoneId.Invalid)
	//		{
	//			_bindPoses[i].Transform.SetParent(_bindPosesGO.transform, false);
	//		}
	//		else
	//		{
	//			_bindPoses[i].Transform.SetParent(_bindPoses[_bindPoses[i].ParentBoneIndex].Transform, false);
	//		}
	//	}
	//}

	//private void InitializeCapsules()
	//{
	//	bool flipX = (_skeletonType == SkeletonType.HandLeft || _skeletonType == SkeletonType.HandRight);

	//	if (_enablePhysicsCapsules)
	//	{
	//		if (!_capsulesGO)
	//		{
	//			_capsulesGO = new GameObject("Capsules");
	//			_capsulesGO.transform.SetParent(transform, false);
	//			_capsulesGO.transform.localPosition = Vector3.zero;
	//			_capsulesGO.transform.localRotation = Quaternion.identity;
	//		}

	//		if (_capsules == null || _capsules.Count != _skeleton.NumBoneCapsules)
	//		{
	//			_capsules = new List<OVRBoneCapsule>(new OVRBoneCapsule[_skeleton.NumBoneCapsules]);
	//			Capsules = _capsules.AsReadOnly();
	//		}

	//		for (int i = 0; i < _capsules.Count; ++i)
	//		{
	//			OVRBone bone = _bones[_skeleton.BoneCapsules[i].BoneIndex];
	//			OVRBoneCapsule capsule = _capsules[i] ?? (_capsules[i] = new OVRBoneCapsule());
	//			capsule.BoneIndex = _skeleton.BoneCapsules[i].BoneIndex;

	//			if (capsule.CapsuleRigidbody == null)
	//			{
	//				capsule.CapsuleRigidbody = new GameObject((bone.Id).ToString() + "_CapsuleRigidbody").AddComponent<Rigidbody>();
	//				capsule.CapsuleRigidbody.mass = 1.0f;
	//				capsule.CapsuleRigidbody.isKinematic = true;
	//				capsule.CapsuleRigidbody.useGravity = false;
	//				capsule.CapsuleRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
	//			}

	//			GameObject rbGO = capsule.CapsuleRigidbody.gameObject;
	//			rbGO.transform.SetParent(_capsulesGO.transform, false);
	//			rbGO.transform.position = bone.Transform.position;
	//			rbGO.transform.rotation = bone.Transform.rotation;

	//			if (capsule.CapsuleCollider == null)
	//			{
	//				capsule.CapsuleCollider = new GameObject((bone.Id).ToString() + "_CapsuleCollider").AddComponent<CapsuleCollider>();
	//				capsule.CapsuleCollider.isTrigger = false;
	//			}

	//			var p0 = flipX ? _skeleton.BoneCapsules[i].StartPoint.FromFlippedXVector3f() : _skeleton.BoneCapsules[i].StartPoint.FromFlippedZVector3f();
	//			var p1 = flipX ? _skeleton.BoneCapsules[i].EndPoint.FromFlippedXVector3f() : _skeleton.BoneCapsules[i].EndPoint.FromFlippedZVector3f();
	//			var delta = p1 - p0;
	//			var mag = delta.magnitude;
	//			var rot = Quaternion.FromToRotation(Vector3.right, delta);
	//			capsule.CapsuleCollider.radius = _skeleton.BoneCapsules[i].Radius;
	//			capsule.CapsuleCollider.height = mag + _skeleton.BoneCapsules[i].Radius * 2.0f;
	//			capsule.CapsuleCollider.direction = 0;
	//			capsule.CapsuleCollider.center = Vector3.right * mag * 0.5f;

	//			GameObject ccGO = capsule.CapsuleCollider.gameObject;
	//			ccGO.transform.SetParent(rbGO.transform, false);
	//			ccGO.transform.localPosition = p0;
	//			ccGO.transform.localRotation = rot;
	//		}
	//	}
	//}


	private void Awake()
    {
        if (_dataProvider == null)
            _dataProvider = GetComponent<OVRSkeleton.IOVRSkeletonDataProvider>();
        if (_hand == null)
            _hand = GetComponent<OVRHand>();
    } 

    private void FixedUpdate()
    {
        if (!IsInitialized || _dataProvider == null)
        {
            IsDataValid = false;
            IsDataHighConfidence = false;

            return;
        }
        var data = _dataProvider.GetSkeletonPoseData();
        IsDataValid = data.IsDataValid;
        IsDataHighConfidence = data.IsDataHighConfidence;
    }
}

 //public enum Chirality { Left, Right };
