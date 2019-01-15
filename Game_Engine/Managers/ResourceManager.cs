using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;
using Game_Engine.Objects;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace Game_Engine.Managers
{
    public static class ResourceManager
    {
        static Dictionary<string, Geometry> geometryDictionary = new Dictionary<string, Geometry>();
        static Dictionary<string, int> textureDictionary = new Dictionary<string, int>();
        static Dictionary<string, AudioBuffer> audioDictionary = new Dictionary<string, AudioBuffer>();
        static Dictionary<string, int> shaderDictionary = new Dictionary<string, int>();

        public static Geometry LoadGeometry(string filename)
        {
            Geometry geometry;
            geometryDictionary.TryGetValue(filename, out geometry);
            if (geometry == null)
            {
                geometry = new Geometry();
                geometry.LoadObject(filename);
                geometryDictionary.Add(filename, geometry);
            }

            return geometry;
        }

        public static int LoadTexture(string filename)
        {
            if (String.IsNullOrEmpty(filename))
                throw new ArgumentException(filename);

            int texture;
            textureDictionary.TryGetValue(filename, out texture);
            if (texture == 0)
            {
                texture = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, texture);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

                Bitmap bmp = new Bitmap(filename);
                BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);

                bmp.UnlockBits(bmp_data);
                textureDictionary.Add(filename, texture);
            }

            return texture;
        }

        public static int LoadCubeMap(List<string> filenames)
        {
            int cubeMap = -1;
            cubeMap = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, cubeMap);

            for (int i = 0; i < filenames.Count; i++)
            {
                if (String.IsNullOrEmpty(filenames[i]))
                    throw new ArgumentException(filenames[i]);

                Bitmap bmp = new Bitmap(filenames[i]);
                BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);

                bmp.UnlockBits(bmp_data);
            }

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (float)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (float)TextureWrapMode.ClampToEdge);
            GL.Enable(EnableCap.TextureCubeMapSeamless);
            return cubeMap;
        }

        public static AudioBuffer LoadWav(string filename)
        {
            if (String.IsNullOrEmpty(filename))
                throw new ArgumentException(filename);

            AudioBuffer audio;
            audioDictionary.TryGetValue(filename, out audio);

            if (audio == null)
            {
                audio = new AudioBuffer();
                audio.LoadObject(filename);
                audioDictionary.Add(filename, audio);
            }
            return audio;
        }

        public static void LoadShader(String filename, ShaderType type, int program, out int address)
        {
            address = GL.CreateShader(type);
            using (StreamReader sr = new StreamReader(filename))
            {
                GL.ShaderSource(address, sr.ReadToEnd());
            }
            GL.CompileShader(address);
            GL.AttachShader(program, address);


            int status;
            GL.GetShader(address, ShaderParameter.CompileStatus, out status);
            if (status == 0)
                throw new Exception(
                           String.Format("Error compiling {0} shader: {1}",
            type.ToString(), GL.GetShaderInfoLog(address)));
        }

        public static int LoadShaderProgram(string vShader, string fShader)
        {
            if (String.IsNullOrEmpty(vShader))
                throw new ArgumentException(vShader);

            if (String.IsNullOrEmpty(fShader))
                throw new ArgumentException(fShader);

            string filename = vShader + ',' + fShader;

            int pgmID;
            shaderDictionary.TryGetValue(filename, out pgmID);

            if (pgmID == 0)
            {
                int vsID;
                int fsID;
                pgmID = GL.CreateProgram();
                LoadShader(vShader, ShaderType.VertexShader, pgmID, out vsID);
                LoadShader(fShader, ShaderType.FragmentShader, pgmID, out fsID);
                GL.LinkProgram(pgmID);
                shaderDictionary.Add(filename, pgmID);
            }

            return pgmID;
        }
    }
}
