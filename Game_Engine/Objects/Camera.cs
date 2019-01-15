using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game_Engine.Components;
using Game_Engine.Systems;
using Game_Engine.Managers;
using Game_Engine.Objects;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace Game_Engine.Objects
{
    public class Camera
    {
        private Matrix4 view, projection;
        private Vector3 position, viewDirection, upDirection;

        public Camera(Vector3 inPosition, Vector3 inViewDirection, Vector3 inUpDirection, int fov, float aspectRatio, float near, float far)
        {
            GL.ClearColor(0.0f, 1.0f, 0.0f, 1.0f);

            GL.CullFace(CullFaceMode.Front);
            GL.Enable(EnableCap.DepthTest);

            position = inPosition;
            viewDirection = inViewDirection;
            upDirection = inUpDirection;

            view = Matrix4.LookAt(position, position + viewDirection, upDirection);
            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(fov), aspectRatio, near, far);
        }

        public Camera(Vector3 inPosition, Vector3 inViewDirection, Vector3 inUpDirection, float width, float height, float near, float far)
        {
            GL.ClearColor(0.0f, 1.0f, 0.0f, 1.0f);

            GL.CullFace(CullFaceMode.Front);
            GL.Enable(EnableCap.DepthTest);

            position = inPosition;
            viewDirection = inViewDirection;
            upDirection = inUpDirection;

            view = Matrix4.LookAt(position, viewDirection, upDirection);
            projection = Matrix4.CreateOrthographic(width, height, near, far);
        }

        public Matrix4 View
        {
            get { return view; }
            set { view = value; }
        }

        public Matrix4 Projection
        {
            get { return projection; }
            set { projection = value; }
        }

        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        public Vector3 Direction
        {
            get { return viewDirection; }
            set { viewDirection = value; }
        }

        public Vector3 UpDirection
        {
            get { return upDirection; }
            set { upDirection = value; }
        }

        public void MoveCamera(Vector3 translation)
        {
            position = translation;
            view = Matrix4.LookAt(position, position + viewDirection, upDirection);
        }
        public void RotateCamera(Vector3 rotation)
        {
            viewDirection = new Vector3 (0, 0, -1) * Matrix3.CreateRotationX(rotation.X) * Matrix3.CreateRotationY(rotation.Y);
            view = Matrix4.LookAt(position, position + viewDirection, upDirection);
        }
    }
}
