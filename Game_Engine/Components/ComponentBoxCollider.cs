using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Game_Engine.Components
{
    public class ComponentBoxCollider : IComponent
    {
        private float width;
        private float height;
        private float depth;

        private bool collided;
        private bool disabled;

        private List<string> ignoreCollisionsWith;

        private List<string> collidedWith;

        public ComponentBoxCollider(float widthIn, float heightIn, float depthIn, List<string> ignoreCollisionsWithIn)
        {
            width = widthIn;
            height = heightIn;
            depth = depthIn;
            ignoreCollisionsWith = ignoreCollisionsWithIn;
            collided = false;
            disabled = false;
            collidedWith = new List<string>();
        }

        public float Width
        {
            get { return width; }
            set { width = value; }
        }

        public float Height
        {
            get { return height; }
            set { height = value; }
        }

        public float Depth
        {
            get { return depth; }
            set { depth = value; }
        }

        public bool Collided
        {
            get { return collided; }
        }

        public List<string> IgnoreCollisionsWith
        {
            get { return ignoreCollisionsWith; }
        }

        public List<string> CollidedWith
        {
            get { return collidedWith; }
            set { collidedWith = value; }
        }

        public bool Disabled
        {
            get { return disabled; }
            set { disabled = value; }
        }

        public ComponentTypes ComponentType
        {
            get { return ComponentTypes.COMPONENT_BOX_COLLIDER; }
        }
    }
}
