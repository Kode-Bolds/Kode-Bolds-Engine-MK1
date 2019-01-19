using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using Game_Engine.Objects;
using OpenTK.Audio;

namespace Game_Engine.Managers
{
    public class SceneManager : GameWindow
    {
        Scene scene;
        static int windowWidth;
        static int windowHeight;
        static AudioContext audioContext;

        public delegate void SceneDelegate(FrameEventArgs e);
        public SceneDelegate renderer;
        public SceneDelegate updater;

        public float dt;
        public static float time;

        public SceneManager() : base(
            1280,
            720,
            GraphicsMode.Default,
            "DOOMED",
            GameWindowFlags.FixedWindow,
            DisplayDevice.Default,
            3,
            3,
            GraphicsContextFlags.ForwardCompatible
            )
        {
            audioContext = new AudioContext();           
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //Sets window width and height
            windowWidth = Width;
            windowHeight = Height;

            //Load GUI
            GUI.LoadGUI(windowWidth, windowHeight);

            //Load main menu on load
            //scene = new MainMenuScene(this);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            updater(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            renderer(e);

            GL.Flush();
            SwapBuffers();
        }

        //public void GameScene()
        //{
        //    scene = new MyGame(this);
        //}

        //public void MainMenuScene()
        //{
        //    scene = new MainMenuScene(this);
        //}

        //public void GameOverScene()
        //{
        //    scene = new GameOverScene(this);
        //}

        //public void VictoryScene()
        //{
        //    scene = new VictoryScene(this);
        //}

        public void LoadScene(Scene sceneIn)
        {
            scene = sceneIn;
        }
    }
}
