using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game_Engine.Managers;
using Game_Engine.Objects;

namespace Game_Engine.Components
{
    public class ComponentAudio : IComponent
    {
        AudioBuffer audioBuffer;
        private int audioSource;
        bool isLooping;
        bool isPlaying;
        bool playOnAwake;

        public ComponentAudio(string audioName, bool looping, bool playOnAwakeIn)
        {
            audioBuffer = ResourceManager.LoadWav(audioName);
            isLooping = looping;
            audioSource = 0;
            playOnAwake = playOnAwakeIn;
        }

        public AudioBuffer AudioBuffer()
        {
            return audioBuffer;
        }

        public bool IsLooping()
        {
            return isLooping;
        }

        public bool IsPlaying
        {
            get { return isPlaying; }
            set { isPlaying = value; }
        }

        public int AudioSource
        {
            get { return audioSource; }
            set { audioSource = value; }
        }

        public bool PlayOnAwake
        {
            get { return playOnAwake; }
            set { playOnAwake = value; }
        }

        public ComponentTypes ComponentType
        {
            get { return ComponentTypes.COMPONENT_AUDIO; }
        }
    }
}
