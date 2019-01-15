using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game_Engine.Managers;
using Game_Engine.Objects;

namespace Game_Engine.Components
{
    public class ComponentGeometry : IComponent
    {
        Geometry geometry;

        public ComponentGeometry(string geometryName)
        {
            geometry = ResourceManager.LoadGeometry(geometryName);
        }

        public ComponentTypes ComponentType
        {
            get { return ComponentTypes.COMPONENT_GEOMETRY; }
        }

        public Geometry Geometry()
        {
            return geometry;
        }
    }
}
