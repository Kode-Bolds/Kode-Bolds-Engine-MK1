using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game_Engine;
using Game_Engine.Managers;

namespace Game_Engine.Objects
{
    public class Scene
    {
        protected SceneManager sceneManager;

        public Scene(SceneManager sceneManager)
        {
            this.sceneManager = sceneManager;
        }
    }
}
