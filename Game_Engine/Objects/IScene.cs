using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Game_Engine.Objects
{
   public interface IScene
    {
        void Render(FrameEventArgs e);
        void Update(FrameEventArgs e);
    }
}
