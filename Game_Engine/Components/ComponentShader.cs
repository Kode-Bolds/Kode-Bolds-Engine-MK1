using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game_Engine.Managers;

namespace Game_Engine.Components
{
    public class ComponentShader : IComponent
    {
        int pgmID;

        public ComponentShader(string vShaderName, string fShaderName)
        {
            pgmID = ResourceManager.LoadShaderProgram(vShaderName, fShaderName);
        }
        public ComponentShader()
        {
            pgmID = ResourceManager.LoadShaderProgram("Shaders/vs.glsl", "Shaders/fs.glsl");
        }

        public int PgmID
        {
            get { return pgmID; }
            set { pgmID = value; }
        }

        public ComponentTypes ComponentType
        {
            get { return ComponentTypes.COMPONENT_SHADER; }
        }
    }
}
