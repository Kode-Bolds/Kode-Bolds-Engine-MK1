using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game_Engine.Objects;
using OpenTK.Input;
using OpenTK;
using System.Diagnostics;

namespace Game_Engine.Managers
{
    public class InputManager
    {
        List<string> keyboardInput;
        List<string> mouseButtonInput;
        private Vector2 mousePosition;
        SceneManager sceneManager;

        public InputManager(SceneManager sceneManagerIn)
        {
            sceneManager = sceneManagerIn;
            keyboardInput = new List<string>();
            mouseButtonInput = new List<string>();
            sceneManager.KeyDown += GameKeyDown;
            sceneManager.KeyUp += GameKeyUp;
            sceneManager.MouseMove += MouseMovement;
            sceneManager.MouseDown += MouseClickDown;
            sceneManager.MouseUp += MouseClickUp;
        }

        public void CenterCursor()
        {
            Mouse.SetPosition(sceneManager.Bounds.Left + sceneManager.Bounds.Width / 2, sceneManager.Bounds.Top + sceneManager.Bounds.Height / 2);
        }

        public void CursorVisible(bool hideCursor)
        {
            if (hideCursor == true)
            {
                sceneManager.CursorVisible = true;
            }
            if(hideCursor == false)
            {
                sceneManager.CursorVisible = false;
            }
        }

        public List<string> Keyboard()
        {
            return keyboardInput;
        }

        public List<string> MouseInput()
        {
            return mouseButtonInput;
        }

        public Vector2 MousePosition()
        {
            return mousePosition;
        }

        void GameKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if (!keyboardInput.Contains(e.Key.ToString()))
            {
                keyboardInput.Add(e.Key.ToString());
            }
        }

        void GameKeyUp(object sender, KeyboardKeyEventArgs e)
        {
            keyboardInput.Remove(e.Key.ToString());
        }

        void MouseClickDown(object sender, MouseButtonEventArgs e)
        {
            if(!mouseButtonInput.Contains(e.Button.ToString()))
            {
                mouseButtonInput.Add(e.Button.ToString());
            }
        }

        void MouseClickUp(object sender, MouseButtonEventArgs e)
        {
            mouseButtonInput.Remove(e.Button.ToString());
        }

        void MouseMovement(object sender, MouseMoveEventArgs e)
        {
            mousePosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
        }
    }
}

