using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Game_Engine.Components
{
    public class ComponentTransform : IComponent
    {
        Matrix4 transform;

        Vector3 forward;
        Vector3 right;
        Vector3 up;
        Vector3 translation;
        Vector3 rotation;
        Vector3 scale;
        bool transformSet;

        public ComponentTransform(Vector3 t, Vector3 r, Vector3 s)
        {
            translation = t;
            rotation = r;
            scale = s;
            transformSet = false;
        }

        public bool SetTransform
        {
            get { return transformSet; }
            set { transformSet = value; }
        }

        public Vector3 Translation
        {
            get { return translation; }
            set { translation = value; }
        }

        public Vector3 Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        public Vector3 Scale
        {
            get { return scale; }
            set { scale = value; }
        }

        public Matrix4 Transform
        {
            get { return transform; }
            set { transform = value; }
        }

        public Vector3 Forward
        {
            get { return forward; }
            set { forward = value; }
        }

        public Vector3 Right
        {
            get { return right; }
            set { right = value; }
        }

        public Vector3 Up
        {
            get { return up; }
            set { up = value; }
        }

        public ComponentTypes ComponentType
        {
            get { return ComponentTypes.COMPONENT_TRANSFORM; }
        }
    }
}
