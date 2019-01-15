using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using Game_Engine.Managers;
using Game_Engine.Objects;

namespace OpenGL_Game.Scenes
{
    class VictoryScene : Scene, IScene
    {
        InputManager inputManager;
        List<string> keyboardInput;
        List<string> mouseInput;
        Vector2 mousePos;

        public VictoryScene(SceneManager sceneManager) : base(sceneManager)
        {
            //Set window title
            sceneManager.Title = "DOOMED - Victory";

            //Set the render and update delegates
            sceneManager.renderer = Render;
            sceneManager.updater = Update;

            //Creates Input Manager
            inputManager = new InputManager(sceneManager);

            //Gets reference from input manager of the list of buttons pressed
            keyboardInput = inputManager.Keyboard();
            mouseInput = inputManager.MouseInput();

            //Sets cursor visibility state
            inputManager.CursorVisible(true);
        }

        public void Render(FrameEventArgs e)
        {
            GL.Viewport(0, 0, sceneManager.Width, sceneManager.Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, sceneManager.Width, 0, sceneManager.Height, -1, 1);

            float width = sceneManager.Width, height = sceneManager.Height, fontSize = Math.Min(width, height) / 10f;
            GUI.DrawText(new Rectangle(0, (int)(fontSize / 2f), (int)width, (int)(fontSize * 2f)), "DOOMED", (int)(fontSize * 1.5f), StringAlignment.Center, Color.White);
            GUI.DrawText(new Rectangle(0, (int)(fontSize / 2f + 200), (int)width, (int)(fontSize * 2f)), "VICTORY!", (int)fontSize, StringAlignment.Center, Color.White);
            GUI.DrawText(new Rectangle(0, (int)(fontSize / 2f + 400), (int)width, (int)(fontSize * 2f)), "Press R To Retry", (int)(fontSize / 1.5f), StringAlignment.Center, Color.White);
            GUI.DrawText(new Rectangle(0, (int)(fontSize / 2f + 600), (int)width, (int)(fontSize * 2f)), "Press M For Menu", (int)(fontSize / 1.5f), StringAlignment.Center, Color.White);

            GUI.Render(Color.Green);
        }

        public void Update(FrameEventArgs e)
        {
            //Gets mouse position from input manager every frame
            mousePos = inputManager.MousePosition();

            //Reloads game if R is pressed
            if (keyboardInput.Contains("R"))
            {
                sceneManager.LoadScene(new MyGame(sceneManager));
            }

            //Loads main menu if M is pressed
            if (keyboardInput.Contains("M"))
            {
                sceneManager.LoadScene(new MainMenuScene(sceneManager));
            }
        }
    }
}
