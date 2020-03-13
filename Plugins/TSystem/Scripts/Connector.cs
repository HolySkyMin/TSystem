using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSystem
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class Connector : MonoBehaviour
    {
        public IngameBasis Game { get { return IngameBasis.Now; } }

        public Material material;
        public float thicknessCoefficient = 0.5f;

        [HideInInspector] public Note headNote;
        [HideInInspector] public Note tailNote;

        float halfWidth;
        Mesh mesh;
        int[] tris;
        Vector2[] uvs;
        Vector3[] columns;

        public void Set(Note h, Note t)
        {
            headNote = h;
            tailNote = t;

            halfWidth = Mathf.Min(headNote.halfTailWidth, tailNote.halfTailWidth) * thicknessCoefficient;
            tris = new[] { 0, 1, 2, 1, 2, 3 };
            columns = new Vector3[4];
            uvs = new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) };

            if(TSystemConfig.Now.colorNote)
                GetComponent<MeshRenderer>().material.color = headNote.ColorKey;
        }

        void Start()
        {
            mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = mesh;
            GetComponent<MeshFilter>().mesh.Clear();
            GetComponent<MeshRenderer>().material = material;
            GetComponent<MeshRenderer>().sortingLayerName = "Meshes";
        }

        void LateUpdate()
        {
            var headTilt = Game.Mode.GetCurrentTiltDegree(headNote.lineSet, headNote.Progress, headNote.StartLine, headNote.EndLine);
            var tailTilt = Game.Mode.GetCurrentTiltDegree(tailNote.lineSet, tailNote.Progress, tailNote.StartLine, tailNote.EndLine);
            var headScale = Game.Mode.GetScale(headNote.Progress) * halfWidth;
            var tailScale = Game.Mode.GetScale(tailNote.Progress) * halfWidth;

            var dV_h = new Vector2(-Mathf.Sin(headTilt * Mathf.Deg2Rad), Mathf.Cos(headTilt * Mathf.Deg2Rad));
            var dV_t = new Vector2(-Mathf.Sin(tailTilt * Mathf.Deg2Rad), Mathf.Cos(tailTilt * Mathf.Deg2Rad));

            columns[0] = headNote.Position + dV_h * headScale;
            columns[1] = headNote.Position - dV_h * headScale;
            columns[2] = tailNote.Position + dV_t * tailScale;
            columns[3] = tailNote.Position - dV_t * tailScale;

            mesh.Clear();
            mesh.vertices = columns;
            mesh.uv = uvs;
            mesh.triangles = tris;
        }
    }
}