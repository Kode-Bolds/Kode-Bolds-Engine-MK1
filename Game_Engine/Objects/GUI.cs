using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging;

namespace Game_Engine.Objects
{
    static public class GUI
    {
        static private Bitmap guiTexture;
        static private int textureLocation;
        static private Graphics GFX;
        static private int width, height;
        static public Vector2 position;
        static public Color clearColour;

        public static void LoadGUI(int widthIn, int heightIn)
        {
            width = widthIn;
            height = heightIn;
            clearColour = Color.Black;
            position = Vector2.Zero;

            //Bitmap
            guiTexture = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);

            //Graphics
            GFX = Graphics.FromImage(guiTexture);
            GFX.Clear(clearColour);

            //Texture loading onto graphics card
            if(textureLocation > 0)
            {
                GL.DeleteTexture(textureLocation);
                textureLocation = 0;
            }
            textureLocation = GL.GenTexture();
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, textureLocation);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, guiTexture.Width, guiTexture.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Disable(EnableCap.Texture2D);
        }

        static public void DrawText(Rectangle rect, string text, int fontSize, StringAlignment stringAligntment, Color colour)
        {
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = stringAligntment;
            stringFormat.LineAlignment = stringAligntment;

            SolidBrush brush = new SolidBrush(colour);

            GFX.DrawString(text, new Font("Impact", fontSize), brush, rect, stringFormat);
        }

        static public void Render(Color clearColourIn)
        {
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            GL.BindTexture(TextureTarget.Texture2D, textureLocation);

            BitmapData data = guiTexture.LockBits(new Rectangle(0, 0, guiTexture.Width, guiTexture.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, (int)guiTexture.Width, (int)guiTexture.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            guiTexture.UnlockBits(data);

            GL.Begin(PrimitiveType.Quads);
            GL.TexCoord2(0f, 1f); GL.Vertex2(position.X, position.Y);
            GL.TexCoord2(1f, 1f); GL.Vertex2(position.X + width, position.Y);
            GL.TexCoord2(1f, 0f); GL.Vertex2(position.X + width, position.Y + height);
            GL.TexCoord2(0f, 0f); GL.Vertex2(position.X, position.Y + height);
            GL.End();

            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.Blend);

            GFX.Clear(clearColourIn);
        }
    }
}
