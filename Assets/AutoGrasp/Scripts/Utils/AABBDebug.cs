using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AABBDebug : MonoBehaviour
{

    public AABBDebugSetting setting;


    public Vector3 lastRecordedPosisiton;
    public List<Vector3> recordedPositions = new List<Vector3>();

    private void LateUpdate()
    {

        if (float.IsNaN(transform.position.x))
        {
            UnityEngine.Debug.LogWarning("pos is nan " + GetComponent<ArticulationBody>().velocity, gameObject);
            if (setting.pauseGame) Time.timeScale = 0f;
            if (setting.autoRecover)
            {
                GetComponent<ArticulationBody>().velocity = Vector3.zero;
                GetComponent<ArticulationBody>().angularVelocity = Vector3.zero;
                transform.localPosition = lastRecordedPosisiton;
                if (TryGetComponent<Renderer>(out Renderer re))
                    re.material.color = setting.colorAfterRecover;
            }

            if (setting.pauseEditor) UnityEngine.Debug.Break();
            return;
        }
        else
        {
            lastRecordedPosisiton = transform.localPosition;
        }
        if (!setting.recordPastFrames) return;
        if (recordedPositions.Count == 0)
        {
            recordedPositions.Add(transform.localPosition);
            lastRecordedPosisiton = transform.localPosition;
            return;
        }
        if (
            transform.localPosition.x != recordedPositions[recordedPositions.Count - 1].x
            ||
            transform.localPosition.y != recordedPositions[recordedPositions.Count - 1].y
            )
        {
            recordedPositions.Add(transform.localPosition);

        }

        if (recordedPositions.Count > 200) recordedPositions.RemoveAt(0);

    }
}