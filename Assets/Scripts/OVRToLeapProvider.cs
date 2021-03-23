using Leap;
using Leap.Unity;
using Leap.Unity.Encoding;
using LeapExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class OVRToLeapProvider : LeapProvider


{
    #region OVR variables
    [SerializeField]
    private OVRCustomSkeleton _leftOvrSkeleton, _rightOvrSkeleton;

    private OVRPlugin.HandState _handState = new OVRPlugin.HandState();
    #endregion

    #region Leap variables
    private Frame _updateFrame = new Frame();
    private Frame _beforeRenderFrame = new Frame();
    private Frame _fixedUpdateFrame = new Frame();

    private VectorHand _leftVHand = new VectorHand();
    private VectorHand _rightVHand = new VectorHand();

    private Hand _leftHand = new Hand();
    private Hand _rightHand = new Hand();
    private List<Hand> _hands = new List<Hand>();
    //private HandJointLocation[] handJointLocations = new HandJointLocation[HandTracker.JointCount];
    #endregion


    public bool IsDataValid { get; private set; }

    public override Frame CurrentFrame
    {
        get
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                _editTimeFrame.Hands.Clear();
                _untransformedEditTimeFrame.Hands.Clear();
                _untransformedEditTimeFrame.Hands.Add(_editTimeLeftHand);
                _untransformedEditTimeFrame.Hands.Add(_editTimeRightHand);
                transformFrame(_untransformedEditTimeFrame, _editTimeFrame);
                return _editTimeFrame;
            }
#endif
            return _updateFrame;
        }
    }

    public override Frame CurrentFixedFrame
    {
        get
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                _editTimeFrame.Hands.Clear();
                _untransformedEditTimeFrame.Hands.Clear();
                _untransformedEditTimeFrame.Hands.Add(_editTimeLeftHand);
                _untransformedEditTimeFrame.Hands.Add(_editTimeRightHand);
                transformFrame(_untransformedEditTimeFrame, _editTimeFrame);
                return _editTimeFrame;
            }
#endif
            return _updateFrame;
        }
    }

    protected virtual void transformFrame(Frame source, Frame dest)
    {
        dest.CopyFrom(source).Transform(transform.GetLeapMatrix());
    }


    private void OnEnable()
    {
        Application.onBeforeRender += Application_onBeforeRender;
    }

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {

    }

    private void OnDisable()
    {
        Application.onBeforeRender -= Application_onBeforeRender;
    }

    private void Application_onBeforeRender()
    {
        // Dispatch the frame event just brefore rendering to reduce jitter
        FillLeapFrame(OVRPlugin.Step.Render, ref _beforeRenderFrame);
        DispatchUpdateFrameEvent(_beforeRenderFrame);
    }

    private void FixedUpdate()
    {
        FillLeapFrame(OVRPlugin.Step.Physics, ref _fixedUpdateFrame);
        DispatchFixedFrameEvent(_fixedUpdateFrame);
    }
    /// <Summary>
    /// Popuates the given Leap Frame with the most recent hand data from OVR
    /// </Summary>
    public void FillLeapFrame(OVRPlugin.Step step, ref Frame leapFrame)
    {
        _hands.Clear();
        if (FillLeapHandFromExtension(_leftOvrSkeleton, true, step, ref _leftVHand))
        {
            _leftVHand.Decode(_leftHand);
            //_leftVHand.Decode(_leftHand, 3);
            _hands.Add(_leftHand);
        }
        if (FillLeapHandFromExtension(_rightOvrSkeleton, false, step, ref _rightVHand))
        {
            _rightVHand.Decode(_rightHand);
            _hands.Add(_rightHand);
        }

        leapFrame.Hands = _hands;
    }

    /// <Summary>
    /// Read the most recent hand data from the OVR plugin and populate a given VectorHand with joint rotations
    /// Since OVR plugin doesnt give us joint locations, we need to find some way to manually calcutate it 
    /// further down the stack in order to fill its data into VectorHand
    /// </Summary>
    private bool FillLeapHandFromExtension(OVRCustomSkeleton skeleton, bool isLeft, OVRPlugin.Step step, ref VectorHand vHand)
    {
        if (skeleton && skeleton.IsDataValid) //(handState.TryLocateHandJoints(frameTime, handJointLocations))
        {
            vHand.isLeft = isLeft;

            //Fill the vHand with joint data            
            vHand.palmPos = skeleton.CustomBones[1].position;// + Vector3.up * 0.1f;// - skeleton.Bones[0].Transform.position).normalized * 10f + skeleton.Bones[1].Transform.position;
            vHand.palmRot = skeleton.CustomBones[1].rotation;// handState.RootPose.Orientation.FromFlippedXQuatf(); // or use FromFlippedZQuatf ?

            Vector3 localJoint = VectorHand.ToLocal(skeleton.CustomBones[0].position, vHand.palmPos, vHand.palmRot); //VectorHand.ToLocal(handJointLocations[2].Position, vHand.palmPos, vHand.palmRot);
            vHand.jointPositions[0] = localJoint;

            for (int j = 1; j < vHand.jointPositions.Length; j++)
            {                
                Vector3 worldBoneV = Vector3.zero;
                int index = GetLeapBoneIndex(j);
                //if ((j + 1) % 5 == 0) // distal (4|9|14|19|24)
                //{
                //    worldBoneV = (skeleton.Bones[index].Transform.position - skeleton.Bones[index - 1].Transform.position).normalized * 0.05f + skeleton.Bones[index].Transform.position;
                //}
                //else
                    worldBoneV = skeleton.CustomBones[index].position;
                localJoint = VectorHand.ToLocal(worldBoneV, vHand.palmPos, vHand.palmRot);
                vHand.jointPositions[j] = localJoint; //skeleton.Bones[GetLeapBoneIndex(j)].Transform.localPosition;//localJoint;
            }

            //3.Move Hands from relative to the provider
            vHand.palmPos = transform.position + vHand.palmPos;

            return true;
        }
        else
        {
            return false;
        }
    }
    private int GetLeapBoneIndex(int fromIndex)
    {
        switch (fromIndex)
        {
            case 0: return 0; // palm
            case 1: return 0; // wrist
            // thumb
            case 2: return 3;
            case 3: return 4;
            case 4: return 5;
            case 5: return 19;
            // index
            case 6: return 6;
            case 7: return 7;
            case 8: return 8;
            case 9: return 20;
            case 10: return 20;
            // middle
            case 11: return 9;
            case 12: return 10;
            case 13: return 11;
            case 14: return 21;
            case 15: return 21;
            // ring
            case 16: return 12;
            case 17: return 13;
            case 18: return 14;
            case 19: return 22;
            case 20: return 22;
            // little
            case 21: return 16;
            case 22: return 17;
            case 23: return 18;
            case 24: return 23;
            case 25: return 23;


            default: return 0;
        }
    }

    //private int GetLeapBoneIndex(int fromIndex)
    //{        

    //    switch (fromIndex)
    //    {
    //        // since oculus doen't track metacarpals of index (6), middle (11) and ring (16) fingers,
    //        // we'll simply return palm...
    //        case 6: case 11: case 16:
    //            return 1;
    //        case int n when (n % 5 == 0): // checking for finger tips, which have 5, 10, 15, 20, 25 as indexes
    //            if (fromIndex == 5)       // thumb
    //                return 19;
    //            else if (fromIndex == 10) // index
    //                return 20;
    //            else if (fromIndex == 15) // middle
    //                return 21;
    //            else if (fromIndex == 20) // ring
    //                return 22;
    //            else if (fromIndex == 25) // little
    //                return 23;
    //            else return 3;            // palm


    //            case int n when (n >= 21): // little finger joints
    //            return fromIndex - 6;

    //        case int n when (n >= 17): // ring finger joints
    //            return fromIndex - 5;

    //        case int n when (n >= 12): // middle finger joints
    //            return fromIndex - 3;

    //        case int n when (n >= 7): // index finger joints
    //            return fromIndex - 1;


    //        default: return fromIndex + 1; // thumb finger joints            
    //    }
    //}

    #region Editor Pose Implementation

#if UNITY_EDITOR
    private Frame _backingUntransformedEditTimeFrame = null;
    private Frame _untransformedEditTimeFrame
    {
        get
        {
            if (_backingUntransformedEditTimeFrame == null)
            {
                _backingUntransformedEditTimeFrame = new Frame();
            }
            return _backingUntransformedEditTimeFrame;
        }
    }
    private Frame _backingEditTimeFrame = null;
    private Frame _editTimeFrame
    {
        get
        {
            if (_backingEditTimeFrame == null)
            {
                _backingEditTimeFrame = new Frame();
            }
            return _backingEditTimeFrame;
        }
    }

    private Dictionary<TestHandFactory.TestHandPose, Hand> _cachedLeftHands
      = new Dictionary<TestHandFactory.TestHandPose, Hand>();
    private Hand _editTimeLeftHand
    {
        get
        {
            if (_cachedLeftHands.TryGetValue(editTimePose, out Hand cachedHand))
            {
                return cachedHand;
            }
            else
            {
                cachedHand = TestHandFactory.MakeTestHand(isLeft: true, pose: editTimePose);
                _cachedLeftHands[editTimePose] = cachedHand;
                return cachedHand;
            }
        }
    }

    private Dictionary<TestHandFactory.TestHandPose, Hand> _cachedRightHands
      = new Dictionary<TestHandFactory.TestHandPose, Hand>();
    private Hand _editTimeRightHand
    {
        get
        {
            if (_cachedRightHands.TryGetValue(editTimePose, out Hand cachedHand))
            {
                return cachedHand;
            }
            else
            {
                cachedHand = TestHandFactory.MakeTestHand(isLeft: false, pose: editTimePose);
                _cachedRightHands[editTimePose] = cachedHand;
                return cachedHand;
            }
        }
    }

#endif
    #endregion
}

//public enum Chirality { Left, Right };
