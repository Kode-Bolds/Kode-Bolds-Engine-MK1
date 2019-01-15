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
    class MainMenuScene : Scene, IScene
    {
        InputManager inputManager;
        List<string> keyboardInput;
        List<string> mouseInput;
        Vector2 mousePos;

        public MainMenuScene(SceneManager sceneManager) : base(sceneManager)
        {
            //Set window title
            sceneManager.Title = "DOOMED - Main Menu";

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
            GUI.DrawText(new Rectangle(0, (int)(fontSize / 2f + 200), (int)width, (int)(fontSize * 2f)), "Main Menu", (int)fontSize, StringAlignment.Center, Color.White);
            GUI.DrawText(new Rectangle(0, (int)(fontSize / 2f + 400), (int)width, (int)(fontSize * 2f)), "Press Space For New Game", (int)(fontSize / 1.5f), StringAlignment.Center, Color.White);
            GUI.DrawText(new Rectangle(0, (int)(fontSize / 2f + 600), (int)width, (int)(fontSize * 2f)), "Press Esc To Quit", (int)(fontSize / 1.5f), StringAlignment.Center, Color.White);

            GUI.Render(Color.Blue);
        }

        public void Update(FrameEventArgs e)
        {
            //Gets mouse position from input manager every frame
            mousePos = inputManager.MousePosition();

            //Starts game if space is pressed
            if(keyboardInput.Contains("Space"))
            {
                sceneManager.LoadScene(new MyGame(sceneManager));
            }

            //Exits program if escape is pressed
            if(keyboardInput.Contains("Escape"))
            {
                sceneManager.Exit();
            }
        }
    }
}
