using System.Collections.Generic;
using UnityEngine;

namespace VRoom
{
    public class PolygonRoomItem : RoomItem
    {      
        [SerializeField] GameObject _polygonGO;         

        public override void ConstructFromParameters(Parameters parameters)
        {
            //SetWindingDirection(parameters);
            
            _polygonGO.transform.SetPositionAndRotation(parameters.Position, parameters.Rotation);
            List<Vector2> verts2D = new List<Vector2>();
            List<Vector3> verts3D = new List<Vector3>();
            for (int i = 0; i < parameters.VertsPositions.Length; i++)
            {
                Vector3 pos = parameters.VertsPositions[i];
                verts3D.Add(_polygonGO.transform.InverseTransformPoint(pos));
                verts2D.Add(new Vector2(pos.x, pos.z));
            }
            Mesh mesh = new Mesh();
            // set verts
            mesh.vertices = verts3D.ToArray();
            // tris
            int[] triangles = new Triangulator(verts2D.ToArray()).Triangulate();
               
            mesh.triangles = triangles;
            //uvs:
            Vector2[] uvs = new Vector2[verts2D.Count];
            for (int i = 0; i < uvs.Length; i++)
            {
                uvs[i] = new Vector2(verts2D[i].x, verts2D[i].y);
            }
            mesh.uv = uvs;

            //rendering components:
            _polygonGO.GetComponent<MeshFilter>().mesh = mesh;          
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            _polygonGO.AddComponent<BoxCollider>();
        }

        private void SetWindingDirection(Parameters par)
        {
            //int last = par.VertsPositions.Length -2;

            ////discover winding direction:
            //Vector3 centerToFirst = Vector3.Normalize(transform.TransformPoint(par.VertsPositions[0]) - par.Position);
            //Vector3 centerToLast = Vector3.Normalize(transform.TransformPoint(par.VertsPositions[last]) - par.Position);
            //float windingAngle = Vector3.SignedAngle(centerToLast, centerToFirst, Vector3.up);

            ////1 = clockwise, -1 = counterclockwise
            //_windingDirection = Mathf.Sign(windingAngle);
        }
    }
}
