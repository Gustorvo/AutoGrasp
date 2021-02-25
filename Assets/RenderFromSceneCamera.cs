using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RenderFromSceneCamera : MonoBehaviour
{
    // get scene camera
    public Camera cam = null;
    public RenderTexture myRenderTexture;
    //Camera.current 

    private void RenderSceneCamera()
    {
        if (cam == null)
            cam = SceneView.lastActiveSceneView.camera;        
        cam.targetTexture = myRenderTexture;
       // cam.giz
    }
    private void Start()
    {
        RenderSceneCamera();
    }

    private void LateUpdate()
    {
        if (cam != null)
            cam.Render();
    }
    
}
