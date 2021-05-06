using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtBodyDebugger : MonoBehaviour
{
    public ArticulationBody _rootArtBody;
    public List<ArticulationBody> bodies = new List<ArticulationBody>();
    public List<int> bodiesStartIndexes = new List<int>();    
    public List<float> fetcheDriveTargetValues = new List<float>();


    private void Awake()
    {
        if (_rootArtBody == null)
            _rootArtBody = GameObject.Find("b_l_wrist/palm").GetComponent<ArticulationBody>();
        if (_rootArtBody == null) // still null, smth is totaly off here;
            Debug.LogError("ArticulationBody root obj is null!");
    }
    private void Start()
    {
        if (_rootArtBody)
        {
            FetchBodies();
            FetchIndexes();          
        }
    }

    private void FixedUpdate()
    {
        if (_rootArtBody)
        {             
            FetchArtDriveTargets();
        }
    }
    public void FetchBodies()
    {
        bodies = new List<ArticulationBody>();
        AddBodiesRecursivelyroot(_rootArtBody.transform);
        // local function
        // adds all art bodies to a list
        void AddBodiesRecursivelyroot(Transform root)
        {
            foreach (Transform child in root)
            {
                if (child.TryGetComponent<ArticulationBody>(out ArticulationBody body))
                {
                    bodies.Add(body);
                    //body.enabled = false;
                   // body.enabled = true;
                    if (child != transform)
                        AddBodiesRecursivelyroot(body.transform);
                }
            }
        }
    }
    public void FetchIndexes()
    {
        bodiesStartIndexes = new List<int>();
        for (int i = 0; i < bodies.Count; i++)
        {
            bodiesStartIndexes.Add(bodies[i].index);
        } 
    }

    private void FetchArtDriveTargets()
    {
        //get all drive targets
        _rootArtBody.GetDriveTargets(fetcheDriveTargetValues);
       // ToDegrees(fetcheDriveTargetValues);
    }

    private void ToDegrees(List<float> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            list[i] *= Mathf.Rad2Deg;
        }
    }
    private void ToRadians(List<float> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            list[i] *= Mathf.Deg2Rad;
        }
    }
}
