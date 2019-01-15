using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Game_Engine.Components
{
    public class ComponentSphereCollider : IComponent
    {
        float radius;

        private bool collided;
        private bool disabled;

        private List<string> ignoreCollisionsWith;

        private List<string> collidedWith;

        public ComponentSphereCollider(float radiusIn, List<string> ignoreCollisionsWithIn)
        {
            radius = radiusIn;
            ignoreCollisionsWith = ignoreCollisionsWithIn;
            collided = false;
            disabled = false;
            collidedWith = new List<string>();
        }

        public float Radius
        {
            get { return radius; }
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
            get { return ComponentTypes.COMPONENT_SPHERE_COLLIDER; }
        }
    }
}
