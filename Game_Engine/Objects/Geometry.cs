using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Game_Engine.Objects
{
    public class Geometry
    {
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> textures = new List<Vector2>();
        List<Vector3> normals = new List<Vector3>();
        List<float> indices = new List<float>();
        int numberOfTriangles;

        // Graphics
        private int vao_Handle;
        private int vbo_verts;

        public Geometry()
        {
        }

        public void LoadObject(string filename)
        {
            string line;

            try
            {
                if (filename != null)
                {
                    FileStream fin = File.OpenRead(filename);
                    StreamReader sr = new StreamReader(fin);

                    GL.GenVertexArrays(1, out vao_Handle);
                    GL.BindVertexArray(vao_Handle);
                    GL.GenBuffers(1, out vbo_verts);

                    List<Vector3> vert = new List<Vector3>();
                    List<Vector2> tex = new List<Vector2>();
                    List<Vector3> norm = new List<Vector3>();
                    List<int> vertInd = new List<int>();
                    List<int> texInd = new List<int>();
                    List<int> normInd = new List<int>();

                    while (!sr.EndOfStream)
                    {
                        line = sr.ReadLine();
                        string[] values = line.Split(' ');

                        if (values[0] == "v")
                        {
                            vert.Add(new Vector3(float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3])));
                        }
                        else if (values[0] == "vt")
                        {
                            tex.Add(new Vector2(float.Parse(values[1]), float.Parse(values[2])));
                        }
                        else if (values[0] == "vn")
                        {
                            norm.Add(new Vector3(float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3])));
                        }
                        else if (values[0] == "f")
                        {
                            string[] faceValues = values[1].Split('/');
                            vertInd.Add(int.Parse(faceValues[0]));
                            texInd.Add(int.Parse(faceValues[1]));
                            normInd.Add(int.Parse(faceValues[2]));

                            faceValues = values[2].Split('/');
                            vertInd.Add(int.Parse(faceValues[0]));
                            texInd.Add(int.Parse(faceValues[1]));
                            normInd.Add(int.Parse(faceValues[2]));

                            faceValues = values[3].Split('/');
                            vertInd.Add(int.Parse(faceValues[0]));
                            texInd.Add(int.Parse(faceValues[1]));
                            normInd.Add(int.Parse(faceValues[2]));

                            numberOfTriangles++;
                        }
                    }

                    for (int i = 0; i < vertInd.Count; i++)
                    {
                        vertices.Add(vert[vertInd[i] - 1]);
                    }
                    for (int i = 0; i < texInd.Count; i++)
                    {
                        textures.Add(tex[texInd[i] - 1]);
                    }
                    for (int i = 0; i < normInd.Count; i++)
                    {
                        normals.Add(norm[normInd[i] - 1]);
                    }

                    for (int i = 0; i < vertices.Count; i++)
                    {
                        indices.Add(vertices[i].X);
                        indices.Add(vertices[i].Y);
                        indices.Add(vertices[i].Z);

                        indices.Add(textures[i].X);
                        indices.Add(textures[i].Y);

                        indices.Add(normals[i].X);
                        indices.Add(normals[i].Y);
                        indices.Add(normals[i].Z);
                    }

                    GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_verts);
                    GL.BufferData<float>(BufferTarget.ArrayBuffer, (IntPtr)(indices.Count * sizeof(float)), indices.ToArray<float>(), BufferUsageHint.StaticDraw);

                    // Positions
                    GL.EnableVertexAttribArray(0);
                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);

                    // Tex Coords
                    GL.EnableVertexAttribArray(1);
                    GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, true, 8 * sizeof(float), 3 * sizeof(float));

                    // Normals
                    GL.EnableVertexAttribArray(2);
                    GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, true, 8 * sizeof(float), 5 * sizeof(float));

                    GL.BindVertexArray(0);
                }
                else
                {
                    throw new FileNotFoundException();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void Render()
        {
            GL.BindVertexArray(vao_Handle);
            GL.DrawArrays(PrimitiveType.Triangles, 0, numberOfTriangles * 3);
        }
    }
}
