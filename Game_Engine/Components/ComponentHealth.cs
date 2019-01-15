using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Engine.Components
{
    public class ComponentHealth : IComponent
    {
        private int health;

        public ComponentHealth(int healthIn)
        {
            health = healthIn;
        }

        public int Health
        {
            get { return health; }
            set { health = value; }
        }

        public ComponentTypes ComponentType
        {
            get { return ComponentTypes.COMPONENT_HEALTH; }
        }
    }
}
