using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Game_Engine.Components
{
    public class ComponentVelocity : IComponent
    {
        float velocity;

        public ComponentVelocity(float vel)
        {
            velocity = vel;
        }

        public float Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }

        public ComponentTypes ComponentType
        {
            get { return ComponentTypes.COMPONENT_VELOCITY; }
        }
    }
}
