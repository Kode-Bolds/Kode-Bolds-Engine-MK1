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
        Vector4 position;
        Vector4 colour;

        public ComponentLight(Vector4 lightPosition, Vector4 lightColour)
        {
            position = lightPosition;
            colour = lightColour;
        }

        public Vector4 Position
        {
            get { return position; }
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
