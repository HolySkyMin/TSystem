using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSystem
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class Multiliner : MonoBehaviour
    {
        public IngameBasis Game { get { return IngameBasis.Now; } }

        [HideInInspector] public Note leftNote;
        [HideInInspector] public Note rightNote;

        public float halfWidthCoeff;
        public Material material;
        [Range(2, 25)] public int joints = 2;

        protected int[] tris;
        protected Vector3[] joint, columns;
        protected Vector2[] uvs;
        protected Mesh mesh;

        int lineSet;
        float halfWidth;

        public void Set(Note l, Note r)
        {
            leftNote = l;
            rightNote = r;

            // Assume that left / right note have same line set
            lineSet = leftNote.lineSet;
            halfWidth = Mathf.Min(leftNote.halfTailWidth, rightNote.halfTailWidth) * halfWidthCoeff;

            InitializeVertexArrays();
        }

        public void InitializeVertexArrays()
        {
            joint = new Vector3[joints];
            columns = new Vector3[joints * 2];
            uvs = new Vector2[joints * 2];
            tris = new int[(joints - 1) * 6];

            for (int i = 0, j = 0; i < (joints - 1) * 2; i++, j += 3)
            {
                tris[j] = i;
                tris[j + 1] = i + 1;
                tris[j + 2] = i + 2;
            }
        }

        private void Start()
        {
            mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = mesh;
            GetComponent<MeshFilter>().mesh.Clear();
            GetComponent<MeshRenderer>().material = material;
            GetComponent<MeshRenderer>().sortingLayerName = "Meshes";
        }

        private void LateUpdate()
        {
            if (leftNote.isHit || rightNote.isHit)
                gameObject.SetActive(false);

            // Assume that left and right note has same progress.
            var progress = leftNote.Progress;
            var leftPath = new Vector2(leftNote.StartLine, leftNote.EndLine);
            var rightPath = new Vector2(rightNote.StartLine, rightNote.EndLine);

            for(int i = 0; i < joints; i++)
            {
                var localPath = Vector2.Lerp(leftPath, rightPath, i / (float)(joints - 1));
                joint[i] = Game.Mode.GetCurrentPos(lineSet, progress, localPath.x, localPath.y);

                var tilt = Game.Mode.GetCurrentTiltDegree(lineSet, progress, localPath.x, localPath.y);
                var armDir = new Vector2(-Mathf.Sin(tilt * Mathf.Deg2Rad), Mathf.Cos(tilt * Mathf.Deg2Rad));
                var magnitude = Game.Mode.GetScale(progress) * halfWidth;
                columns[2 * i] = joint[i] + (Vector3)(armDir * magnitude);
                columns[2 * i + 1] = joint[i] - (Vector3)(armDir * magnitude);
                uvs[2 * i] = new Vector2(0, 1 - i / (float)(joints - 1));
                uvs[2 * i + 1] = new Vector2(1, 1 - i / (float)(joints - 1));
            }

            mesh.Clear();
            mesh.vertices = columns;
            mesh.uv = uvs;
            mesh.triangles = tris;
        }
    }
}