using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;

namespace Game_Engine.Components
{
    public class ComponentLight : IComponent
    {
        Vector4 colour;

        public ComponentLight(Vector4 lightColour)
        {
            colour = lightColour;
        }

        public Vector4 Colour
        {
            get { return colour; }
        }

        public ComponentTypes ComponentType
        {
            get { return ComponentTypes.COMPONENT_LIGHT; }
        }
    }
}
