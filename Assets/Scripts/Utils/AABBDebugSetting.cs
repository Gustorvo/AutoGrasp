using UnityEngine;
using System.Collections;

[System.Serializable]
[CreateAssetMenu(menuName = "Debug/ABBB Setting")]
public class AABBDebugSetting : ScriptableObject
{
    public bool autoRecover = true;
    public Color colorAfterRecover;
    public bool recordPastFrames = false;
    public bool pauseEditor = true;
    public bool pauseGame = false;
}