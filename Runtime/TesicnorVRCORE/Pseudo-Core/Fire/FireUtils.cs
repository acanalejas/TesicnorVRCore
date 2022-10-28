using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TesicFire
{
    public interface FireUtils
    {
        public void BeginFire(Vector3 initialPoint);
        public void EndFire();

        public void UpdateFire(Vector3 initialPoint);
        public void ExtinguishFire();

        public void ExtinguishWithRaycast(Ray raycast);

        public void ExtinguishWithCone(Vector3 origin, Vector3 forward);

        public void ExtinguishWithParticles();

        public void Propagate();
        public bool OnFire();

        public bool FireStarted();

        public bool CompleteFire();
        public bool Extinguished();

        public Mesh FireMesh(Vector3 initialFirePoint);

        public Mesh FireMesh(Vector3 initialFirePoint, string assetName);

        public void FireMesh(Vector3 initialFirePoint, string assetName, float radiusMultiplier);

        public Vector2 ParticleSize();
    }

    public static class FireUtilsMethods
    {
        public static string MeshToString(Mesh input)
        {
            List<Vector3> vertices = new List<Vector3>(), normals = new List<Vector3>();
            List<int> triangles = new List<int>();

            input.GetVertices(vertices); input.GetNormals(normals); triangles.AddRange(input.GetTriangles(0));

            string result = Vector3ListToString(vertices) + "%" + IntListToString(triangles) + "%" + Vector3ListToString(normals);

            return result;
        }

        public static Mesh StringToMesh(string input)
        {
            Mesh result = new Mesh();
            string[] lists = input.Split("%");

            if (lists.Length != 3) return result;
            else
            {
                List<Vector3> vertices = StringToVector3List(lists[0]);
                List<int> triangles = StrintToIntList(lists[1]);
                List<Vector3> normals = StringToVector3List(lists[2]);

                result.SetVertices(vertices.ToArray());
                result.SetTriangles(triangles.ToArray(), 0);
                result.SetNormals(normals.ToArray());

                return result;
            }
        }
        public static string Vector3ListToString(List<Vector3> input)
        {
            string result = "";
            foreach(Vector3 v in input)
            {
                result += Vector3ToString(v) + "/";
            }
            return result;
        }

        public static List<Vector3> StringToVector3List(string input)
        {
            List<Vector3> result = new List<Vector3>(); 
            string[] vectors = input.Split("/");

            foreach(var v in vectors)
            {
                Vector3 parse = StringToVector3(v);
                result.Add(parse);
            }

            return result;
        }
        public static string Vector3ToString(Vector3 input)
        {
            float x = input.x;
            float y = input.y;
            float z = input.z;

            string result = x.ToString() + "|" + y.ToString() + "|" + z.ToString();
            return result;
        }

        public static Vector3 StringToVector3(string input)
        {
            string[] components = input.Split("|");

            if (components.Length != 3) return Vector3.zero;
            else
            {
                float x, y, z;

                float.TryParse(components[0], out x);
                float.TryParse(components[1], out y);
                float.TryParse(components[2], out z);

                return new Vector3(x, y, z);
            }
        }

        public static string IntListToString(List<int> input)
        {
            string result = "";
            foreach(int i in input)
            {
                result += i.ToString();
            }
            return result;
        }

        public static string IntArrayToString(int[] input)
        {
            string result = "";
            foreach(int i in input)
            {
                result += i.ToString();
            }
            return result;
        }

        public static List<int> StrintToIntList(string input)
        {
            List<int> result = new List<int>();
            char[] chars = input.ToCharArray();
            foreach(char c in chars)
            {
                int i = 0;
                int.TryParse(c.ToString(), out i);
                result.Add(i);
            }

            return result;
        }

    }
}
