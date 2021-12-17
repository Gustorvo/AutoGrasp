using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomItemSerializer
{
    //public void SaveWindows()
    //{
    //    //serialize room windows:
    //    string windowsPositionData = "";
    //    string windowsScaleData = "";
    //    for (int i = 0; i < Windows.Count; i++)
    //    {
    //        Vector3 windowPosition = Windows[i].transform.position;
    //        Vector3 windowScale = Windows[i].transform.localScale;
    //        windowsPositionData += $"{windowPosition.x},{windowPosition.y},{windowPosition.z}";
    //        windowsScaleData += $"{windowScale.x},{windowScale.y},{windowScale.z}";
    //        if (i < Windows.Count - 1)
    //        {
    //            windowsPositionData += "|";
    //            windowsScaleData += "|";
    //        }
    //    }
    //    if (!string.IsNullOrEmpty(windowsPositionData))
    //    {
    //        PlayerPrefs.SetString("WindowsPositions", windowsPositionData);
    //        PlayerPrefs.SetString("WindowsScales", windowsScaleData);
    //    }
    //}

    //public void LoadWindows()
    //{
    //    if (!PlayerPrefs.HasKey("WindowsPositions"))
    //        return;
    //    //deserialize room windows:
    //    List<Vector3> windowsPositions = deserialize("WindowsPositions");
    //    List<Vector3> windowsScales = deserialize("WindowsScales");
    //    WindowsPoses = new Vector3[windowsPositions.Count, 2];
    //    for (int i = 0; i < windowsPositions.Count; i++)
    //    {
    //        WindowsPoses[i, 0] = windowsPositions[i];
    //        WindowsPoses[i, 1] = windowsScales[i];
    //    }

    //    // local function
    //    List<Vector3> deserialize(string key)
    //    {
    //        List<Vector3> unwrapped = new List<Vector3>();
    //        try
    //        {
    //            string input = PlayerPrefs.GetString(key, "");
    //            string[] inputs = input.Split('|');
    //            foreach (var item in inputs)
    //            {
    //                string[] vector3 = item.Split(',');
    //                unwrapped.Add(new Vector3(float.Parse(vector3[0]), float.Parse(vector3[1]), float.Parse(vector3[2])));
    //            }
    //        }
    //        catch (Exception e)
    //        {
    //            PlayerPrefs.DeleteAll();
    //            throw e;
    //        }
    //        return unwrapped;
    //    }
    //}
}
