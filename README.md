# AutoGrasp
![Hands L](https://user-images.githubusercontent.com/31797378/151725702-30392f89-8727-4d6c-9990-55b01f53c1f5.gif)
VR hands with realistic physics behaviors using Unity's PhysX SDK 4 and new TGS solver. Hand joints are driven by new [articulation joint system](https://docs.unity3d.com/2020.1/Documentation/ScriptReference/ArticulationBody.html).
Those are much more difficult to control, since articulation bodies can only be moved by forces and torques, so a properly tuned PID/PD controller is requaired.
Currently only Oculus hand tracking is supported (via Oculus SDK integration package). Future updates will include Leap Motion (Ultraleap) support as well.
Articulation body component is still in beta, but continuesly improving with new versions of unity. [This thread](https://forum.unity.com/threads/featherstones-solver-for-articulations.792294/) might be useful to start working with articulations.

