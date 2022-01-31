# AutoGrasp
VR hands with realistic physics behaviors using Unity's PhysX SDK 4 and new TGS solver. Hand joints are driven by new [articulation joint system](https://docs.unity3d.com/2020.1/Documentation/ScriptReference/ArticulationBody.html).
Those are much more difficult to control, since articulation bodies can only be moved by forces and torques, so a properly tuned PID/PD controller is requaired.
