#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using Game_Engine.Managers;
using OpenGL_Game.Scenes;
#endregion

namespace OpenGL_Game
{
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class MainEntry
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (var sceneManager = new SceneManager())
            {
                sceneManager.LoadScene(new MainMenuScene(sceneManager));
                sceneManager.Run(60.0, 0.0);
            }
        }
    }
#endif
}
