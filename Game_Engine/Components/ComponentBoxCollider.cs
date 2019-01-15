using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Game_Engine.Components
{
    public class ComponentBoxCollider : IComponent
    {
        private Vector2 topLeft;
        private Vector2 topRight;
        private Vector2 bottomRight;
        private Vector2 bottomLeft;

        private bool collided;
        private bool disabled;

        private List<string> ignoreCollisionsWith;

        private List<string> collidedWith;

        public ComponentBoxCollider(Vector2 topLeftIn, Vector2 topRightIn, Vector2 bottomRightIn, Vector2 bottomLeftIn, List<string> ignoreCollisionsWithIn)
        {
            topLeft = topLeftIn;
            topRight = topRightIn;
            bottomRight = bottomRightIn;
            bottomLeft = bottomLeftIn;
            ignoreCollisionsWith = ignoreCollisionsWithIn;
            collided = false;
            disabled = false;
            collidedWith = new List<string>();
        }

        public Vector2 TopLeft
        {
            get { return topLeft; }
        }

        public Vector2 TopRight
        {
            get { return topRight; }
        }

        public Vector2 BottomRight
        {
            get { return bottomRight; }
        }

        public Vector2 BottomLeft
        {
            get { return bottomLeft; }
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
