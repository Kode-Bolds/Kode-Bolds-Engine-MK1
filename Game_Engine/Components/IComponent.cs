using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Engine.Components
{
    [FlagsAttribute]
    public enum ComponentTypes {
        COMPONENT_NONE = 0,
	    COMPONENT_TRANSFORM = 1 << 0,
        COMPONENT_GEOMETRY = 1 << 1,
        COMPONENT_TEXTURE  = 1 << 2,
        COMPONENT_AUDIO = 1 << 3,
        COMPONENT_VELOCITY = 1 << 4,
        COMPONENT_SHADER = 1 << 5,
        COMPONENT_AI = 1 << 6,
        COMPONENT_BOX_COLLIDER = 1 << 7,
        COMPONENT_SPHERE_COLLIDER = 1 << 8,
        COMPONENT_HEALTH = 1 << 9,
        COMPONENT_LIGHT = 1 << 10
    }

    public interface IComponent
    {
        ComponentTypes ComponentType
        {
            get;
        }
    }
}
