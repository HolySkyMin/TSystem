using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSystem
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class Tail : MonoBehaviour
    {
        public static bool FixTailPosAtZero { get; set; }

        public IngameBasis Game { get { return IngameBasis.Now; } }

        [HideInInspector] public Color32 headColor;
        [HideInInspector] public Color32 tailColor;
        [HideInInspector] public Note headNote;
        [HideInInspector] public Note tailNote;

        // MeshRenderer, MeshFilter는 GetComponent 로 해결
        public Material material;
        [Range(15, 50)] public int joints = 20;

        // 메쉬를 구성하는 필수 꼭짓점 데이터들
        protected int[] tris;
        protected Vector3[] joint, columns;
        protected Vector2[] uvs;
        protected Mesh mesh;

        // 데이터들
        float headTime = 0, tailTime = 0, halfWidth;
        bool allowFlexibleTilt;

        public void Set(Note h, Note t, bool flexible)
        {
            headNote = h;
            tailNote = t;
            allowFlexibleTilt = flexible;

            halfWidth = Mathf.Min(headNote.halfTailWidth, tailNote.halfTailWidth);

            InitializeVertexArrays();
        }

        /// <summary>
        /// Initializes 'joint, columns, uvs, tris' array based on Joints value.
        /// </summary>
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
            headTime = Mathf.Clamp01(headNote.Progress);
            tailTime = Mathf.Clamp01(tailNote.Progress);

            var progressBetweenAppear = Mathf.Clamp01(Mathf.InverseLerp(headNote.appearTime, tailNote.appearTime, Game.Time));
            var progressBetweenReach = Mathf.Clamp01(Mathf.InverseLerp(Mathf.Min(headNote.hitTime, headNote.ReachTime), tailNote.ReachTime, Game.Time));
            var headPath = new Vector2(headNote.StartLine, Mathf.Lerp(headNote.EndLine, tailNote.EndLine, progressBetweenReach));
            var tailPath = new Vector2(FixTailPosAtZero ? tailNote.StartLine : Mathf.Lerp(headNote.StartLine, tailNote.StartLine, progressBetweenAppear), tailNote.EndLine);

            for (int i = 0; i < joints; i++)
            {
                var localProgress = Mathf.Lerp(tailTime, headTime, i / (float)(joints - 1));
                var localPathPos = Vector2.Lerp(tailPath, headPath, i / (float)(joints - 1));
                var localLineSet = Mathf.Lerp(tailNote.lineSet, headNote.lineSet, i / (float)(joints - 1));
                joint[i] = Game.Mode.GetCurrentPos(localLineSet, localProgress, localPathPos.x, localPathPos.y);
                //Debug.Log(gameObject.name + " [" + i + "] : joint = " + joint[i]);

                var localTiltAngle = Game.Mode.GetCurrentTiltDegree(localLineSet, localProgress, localPathPos.x, localPathPos.y);
                var localScale = Game.Mode.GetScale(localProgress);
                var armDir = new Vector2(Mathf.Cos(localTiltAngle * Mathf.Deg2Rad), Mathf.Sin(localTiltAngle * Mathf.Deg2Rad));
                if (allowFlexibleTilt)
                {
                    var tiltAngle = ((Vector2)(joint[joints - 1] - joint[joints - 2])).normalized; // Joint goes as tail -> head; so, this means the tilt of head. (dir to head)
                    armDir = Vector2.Perpendicular(tiltAngle);
                }
                var magnitude = halfWidth * Mathf.Sqrt(
                    Mathf.Pow(localScale.x * Mathf.Cos(Vector2.SignedAngle(Vector2.right, armDir)), 2) +
                    Mathf.Pow(localScale.y * Mathf.Sin(Vector2.SignedAngle(Vector2.right, armDir)), 2));
                //Debug.Log(gameObject.name + " [" + i + "] : dir = " + armDir + ", mag = " + magnitude);
                columns[2 * i] = joint[i] + (Vector3)(armDir * magnitude);
                columns[2 * i + 1] = joint[i] - (Vector3)(armDir * magnitude);
                uvs[2 * i] = new Vector2(0, 1 - i / (float)(joints - 1));
                uvs[2 * i + 1] = new Vector2(1, 1 - i / (float)(joints - 1));
                //Debug.Log(gameObject.name + " [" + i + "] : columns = [ " + columns[2 * i] + ", " + columns[2 * i + 1] + " ]");
            }

            mesh.Clear();
            mesh.vertices = columns;
            mesh.uv = uvs;
            mesh.triangles = tris;
        }
    }
}