namespace SoftHand
{
    using System;
    using System.Threading;
    using SoftHandInternal;
    using UnityEngine;
    using static SoftHandInternal.OVRToSoftHandTrackingData;

    /// <summary>
    /// The Controller class is your main interface to the Leap Motion Controller.
    /// 
    /// Create an instance of this Controller class to access frames of tracking
    /// data and configuration information.Frame data can be polled at any time
    /// using the Controller.Frame() function.Call frame() or frame(0) to get the
    /// most recent frame.Set the history parameter to a positive integer to access
    /// previous frames.A controller stores up to 60 frames in its frame history.
    /// 
    /// 
    /// Polling is an appropriate strategy for applications which already have an
    /// intrinsic update loop, such as a game. You can also subscribe to the FrameReady
    /// event to get tracking frames through an event delegate.
    /// 
    /// If the current thread implements a SynchronizationContext that contains a message
    /// loop, events are posted to that threads message loop. Otherwise, events are called
    /// on an independent thread and applications must perform any needed synchronization
    /// or marshalling of data between threads. Note that Unity3D does not create an
    /// appropriate SynchronizationContext object. Typically, event handlers cannot access
    /// any Unity objects.    /// 

    /// </summary>
    public class Controller

    {
        bool _disposed = false;
        private bool _hasInitialized = false;
        private bool _hasConnected = false;

        public Controller() { }

        /// <summary>
        /// In most cases you should get Frame objects using the LeapProvider.CurrentFrame
        /// property. The data in Frame objects taken directly from a Leap.Controller instance
        /// is still in the Leap Motion frame of reference and will not match the hands
        /// displayed in a Unity scene.
        /// Returns a frame of tracking data from the Leap Motion software. Use the optional
        /// history parameter to specify which frame to retrieve. Call frame() or
        /// frame(0) to access the most recent frame; call frame(1) to access the
        /// previous frame, and so on. If you use a history value greater than the
        /// number of stored frames, then the controller returns an empty frame.
        /// 
        /// @param history The age of the frame to return, counting backwards from
        /// the most recent frame (0) into the past and up to the maximum age (59).
        /// @returns The specified frame; or, if no history parameter is specified,
        /// the newest frame. If a frame is not available at the specified history
        /// position, an invalid Frame is returned.

        /// </summary>
        public Frame Frame(int history = 0)
        {
            Frame frame = new Frame();
            Frame(frame, history);
            return frame;
        }

        /// <summary>
        /// Identical to Frame(history) but instead of constructing a new frame and returning
        /// it, the user provides a frame object to be filled with data instead.
        /// </summary>
        public void Frame(Frame toFill, int history = 0)
        {
            throw new NotImplementedException();
            //OVR_TRACKING_EVENT trackingEvent;
            //_connection.Frames.Get(out trackingEvent, history);
            //toFill.CopyFrom(ref trackingEvent);
        }

        /// <summary>
        /// Returns the timestamp of a recent tracking frame.  Use the
        /// optional history parameter to specify how many frames in the past
        /// to retrieve the timestamp.  Leave the history parameter as
        /// it's default value to return the timestamp of the most recent
        /// tracked frame.
        /// </summary>
        public long FrameTimestamp(int history = 0)
        {
            OVR_TRACKING_EVENT trackingEvent;
            throw new NotImplementedException();
            // _connection.Frames.Get(out trackingEvent, history);
            // return trackingEvent.info.timestamp;
        }

        /// <summary>
        /// Returns the frame object with all hands transformed by the specified
        /// transform matrix.
        /// </summary>
        public Frame GetTransformedFrame(Transform trs, int history = 0)
        {
            throw new NotImplementedException();
            //return new Frame().CopyFrom(Frame(history)).Transform(trs);
        }

        /// <summary>
        /// Returns the Frame at the specified time, interpolating the data between existing frames, if necessary.
        /// </summary>
        public Frame GetInterpolatedFrame(Int64 time)
        {
            throw new NotImplementedException();
            //return _connection.GetInterpolatedFrame(time);
        }

        /// <summary>
        /// Fills the Frame with data taken at the specified time, interpolating the data between existing frames, if necessary.
        /// </summary>
        public void GetInterpolatedFrame(Frame toFill, Int64 time)
        {
            //_connection.GetInterpolatedFrame(toFill, time);
        }

        /// <summary>
        /// Returns the Head pose at the specified time, interpolating the data between existing frames, if necessary.
        /// </summary>
        public OVR_HEAD_POSE_EVENT GetInterpolatedHeadPose(Int64 time)
        {
            throw new NotImplementedException();
            //return _connection.GetInterpolatedHeadPose(time);
        }

        public void GetInterpolatedHeadPose(ref OVR_HEAD_POSE_EVENT toFill, Int64 time)
        {
            throw new NotImplementedException();
            //_connection.GetInterpolatedHeadPose(ref toFill, time);
        }

        public UInt64 TelemetryGetNow()
        {
            throw new NotImplementedException();
            // return LeapC.TelemetryGetNow();
        }


        /// <summary>
        /// This is a special variant of GetInterpolatedFrameFromTime, for use with special
        /// features that only require the position and orientation of the palm positions, and do
        /// not care about pose data or any other data.
        /// 
        /// You must specify the id of the hand that you wish to get a transform for.  If you specify
        /// an id that is not present in the interpolated frame, the output transform will be the
        /// identity transform.
        /// </summary>
        public void GetInterpolatedLeftRightTransform(Int64 time,
                                                      Int64 sourceTime,
                                                      int leftId,
                                                      int rightId,
                                                  out Transform leftTransform,
                                                  out Transform rightTransform)
        {
            throw new NotImplementedException();
            //_connection.GetInterpolatedLeftRightTransform(time, sourceTime, leftId, rightId, out leftTransform, out rightTransform);
        }

        public void GetInterpolatedFrameFromTime(Frame toFill, Int64 time, Int64 sourceTime)
        {
            throw new NotImplementedException();
            // _connection.GetInterpolatedFrameFromTime(toFill, time, sourceTime);
        }

        /// <summary>
        /// Returns a timestamp value as close as possible to the current time.
        /// Values are in microseconds, as with all the other timestamp values.
        /// 
        /// @since 2.2.7
        /// </summary>
        public long Now()
        {
            throw new NotImplementedException();
            // return LeapC.GetNow();
        }
    }
}
