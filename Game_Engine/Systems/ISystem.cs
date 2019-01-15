using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game_Engine;
using Game_Engine.Objects;

namespace Game_Engine.Systems
{
    public interface ISystem
    {
        void OnAction();
        void AssignEntity(Entity entity);
        void DestroyEntity(Entity entity);

        // Property signatures: 
        string Name
        {
            get;
        }
    }
}
