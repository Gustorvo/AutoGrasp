namespace SoftHandInternal
{
    using SoftHand;
    using UnityEngine;
    using static OVRSkeleton;
    using static SoftHandInternal.OVRToSoftHandTrackingData;

    public static class CopyFromOVRDataExtensions
    {
        /**
         * Copies the data from an internal tracking message into a frame.
         *
         * @param trackingMsg The internal tracking message with the data to be copied into this frame.
         */
        public static Frame CopyFrom(this Frame frame, ref OVR_TRACKING_EVENT trackingData)
        {
            frame.Id = (long)trackingData.frame_id;
            frame.Timestamp = (long)trackingData.timestamp;
            frame.CurrentFramesPerSecond = trackingData.framerate;

            frame.ResizeHandList((int)trackingData.nHands);

            for (int i = frame.Hands.Count; i-- != 0;)
            {
                OVRHand hand = new OVRHand(); // get data from VRP hand provider
               // StructMarshal<LEAP_HAND>.ArrayElementToStruct(trackingData.pHands, i, out hand);
                frame.Hands[i].CopyFrom(ref hand, frame.Id);
            }

            return frame;
        }

        /**
         * Copies the data from an internal hand definition into a hand.
         *
         * @param leapHand The internal hand definition to be copied into this hand.
         * @param frameId The frame id of the frame this hand belongs to.
         */
        public static ArticulatedHand CopyFrom(this ArticulatedHand hand, ref OVRHand ovrHand, long frameId)
        {
           // hand.FrameId = frameId;
           // hand.Id = (int)ovrHand.id;

            //hand.Arm.CopyFrom(leapHand.arm, Bone.BoneType.TYPE_INVALID);

           // hand.Confidence = ovrHand.confidence;
           // hand.GrabStrength = ovrHand.grab_strength;
           // hand.GrabAngle = ovrHand.grab_angle;
           // hand.PinchStrength = ovrHand.pinch_strength;
           // hand.PinchDistance = ovrHand.pinch_distance;
           // hand.PalmWidth = ovrHand.palm.width;
           //// hand.IsLeft = leapHand.type == eLeapHandType.eLeapHandType_Left;
           // hand.TimeVisible = (float)(ovrHand.visible_time * 1e-6);
           // hand.PalmPosition = ovrHand.palm.position.ToLeapVector();
           // hand.StabilizedPalmPosition = ovrHand.palm.stabilized_position.ToLeapVector();
           // hand.PalmVelocity = ovrHand.palm.velocity.ToLeapVector();
           // hand.PalmNormal = ovrHand.palm.normal.ToLeapVector();
           // hand.Rotation = ovrHand.palm.orientation.ToLeapQuaternion();
           // hand.Direction = ovrHand.palm.direction.ToLeapVector();
           // //hand.WristPosition = hand.Arm.NextJoint;

            //hand.Fingers[0].CopyFrom(ovrHand.thumb, Leap.Finger.FingerType.TYPE_THUMB, hand.Id, hand.TimeVisible);
            //hand.Fingers[1].CopyFrom(ovrHand.index, Leap.Finger.FingerType.TYPE_INDEX, hand.Id, hand.TimeVisible);
            //hand.Fingers[2].CopyFrom(ovrHand.middle, Leap.Finger.FingerType.TYPE_MIDDLE, hand.Id, hand.TimeVisible);
            //hand.Fingers[3].CopyFrom(ovrHand.ring, Leap.Finger.FingerType.TYPE_RING, hand.Id, hand.TimeVisible);
            //hand.Fingers[4].CopyFrom(ovrHand.pinky, Leap.Finger.FingerType.TYPE_PINKY, hand.Id, hand.TimeVisible);

            return hand;
        }

        /**
         * Copies the data from an internal finger definition into a finger.
         *
         * @param leapBone The internal finger definition to be copied into this finger.
         * @param type The finger type of this finger.
         * @param frameId The frame id of the frame this finger belongs to.
         * @param handId The hand id of the hand this finger belongs to.
         * @param timeVisible The time in seconds that this finger has been visible.
         */
        public static ArticulatedFinger CopyFrom(this ArticulatedFinger finger, OVRBone OvrBone, ArticulatedFinger.FingerType type, int handId, float timeVisible)
        {
            //finger.Id = (handId * 10) + OvrBone.finger_id;
            finger.HandId = handId;
            finger.TimeVisible = timeVisible;

            ArticulatedBone metacarpal = finger.bones[0];
            ArticulatedBone proximal = finger.bones[1];
            ArticulatedBone intermediate = finger.bones[2];
            ArticulatedBone distal = finger.bones[3];

           // metacarpal.CopyFrom(OvrBone.metacarpal, Leap.Bone.BoneType.TYPE_METACARPAL);
           // proximal.CopyFrom(OvrBone.proximal, Leap.Bone.BoneType.TYPE_PROXIMAL);
           // intermediate.CopyFrom(OvrBone.intermediate, Leap.Bone.BoneType.TYPE_INTERMEDIATE);
           // distal.CopyFrom(OvrBone.distal, Leap.Bone.BoneType.TYPE_DISTAL);

            finger.TipPosition = distal.NextJoint;
            finger.Direction = intermediate.Direction;
            finger.Width = intermediate.Width;
            //finger.Length = (OvrBone.finger_id == 0 ? 0.0f : 0.5f * proximal.Length) + intermediate.Length + 0.77f * distal.Length; //The values 0.5 for proximal and 0.77 for distal are used in platform code for this calculation
            //finger.IsExtended = OvrBone.is_extended != 0;
            finger.Type = type;

            return finger;
        }

        /**
         * Copies the data from an internal bone definition into a bone.
         *
         * @param leapBone The internal bone definition to be copied into this bone.
         * @param type The bone type of this bone.
         */
        public static ArticulatedBone CopyFrom(this ArticulatedBone bone, OVRBone OvrBone, ArticulatedBone.BoneType type)
        {
            bone.Type = type;
           // bone.PrevJoint = leapBone.prev_joint.ToLeapVector();
           // bone.NextJoint = leapBone.next_joint.ToLeapVector();
            bone.Direction = (bone.NextJoint - bone.PrevJoint);
            //bone.Length = bone.Direction.Magnitude;

            if (bone.Length < float.Epsilon)
            {
                bone.Direction = Vector3.zero;
            }
            else
            {
                bone.Direction /= bone.Length;
            }

            bone.Center = (bone.PrevJoint + bone.NextJoint) / 2.0f;
            //bone.Rotation = OvrBone.rotation.ToLeapQuaternion();
            //bone.Width = OvrBone.width;

            return bone;
        }
    }

}