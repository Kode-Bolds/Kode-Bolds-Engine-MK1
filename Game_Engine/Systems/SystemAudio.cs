using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game_Engine.Objects;
using OpenTK;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using System.IO;
using Game_Engine.Components;

namespace Game_Engine.Systems
{
    public class SystemAudio : ISystem
    {
        const ComponentTypes MASK = (ComponentTypes.COMPONENT_AUDIO | ComponentTypes.COMPONENT_TRANSFORM);

        private Vector3 listenerPosition;
        private Vector3 listenerDirection;
        private Vector3 listenerUp;
        private int mySource;
        private int newSource;
        
        Camera camera;

        List<Entity> entityList;

        public SystemAudio(Camera inCamera)
        {
            entityList = new List<Entity>();
            camera = inCamera;
            listenerPosition = camera.Position;
            listenerDirection = camera.Direction;
            listenerUp = camera.UpDirection;
        }

        public string Name
        {
            get { return "SystemAudio"; }
        }

        public void AssignEntity(Entity entity)
        {
            if ((entity.Mask & MASK) == MASK)
            {
                entityList.Add(entity);
            }
        }

        public void DestroyEntity(Entity entity)
        {
            if(entityList.Contains(entity))
            {
                DeleteSource(entity);

                entityList.Remove(entity);
            }
        }

        public void OnAction()
        {
            UpdateListener();
            foreach (Entity entity in entityList)
            {
                List<IComponent> components = entity.Components;

                IComponent positionComponent = components.Find(delegate (IComponent component)
                {
                    return component.ComponentType == ComponentTypes.COMPONENT_TRANSFORM;
                });
                Vector3 translation = ((ComponentTransform)positionComponent).Translation;

                IComponent audioComponent = components.Find(delegate (IComponent component)
                {
                    return component.ComponentType == ComponentTypes.COMPONENT_AUDIO;
                });
                AudioBuffer audio = ((ComponentAudio)audioComponent).AudioBuffer();
                bool isLooping = ((ComponentAudio)audioComponent).IsLooping();
                bool isPlaying = ((ComponentAudio)audioComponent).IsPlaying;

                if(((ComponentAudio)audioComponent).AudioSource == 0)
                {
                    AL.GenSources(1, out newSource);
                    ((ComponentAudio)audioComponent).AudioSource = newSource;
                }

                if(((ComponentAudio)audioComponent).PlayOnAwake == true)
                {
                    mySource = ((ComponentAudio)audioComponent).AudioSource;

                    PlayAudio(isLooping, isPlaying, mySource, audio, translation);

                    ((ComponentAudio)audioComponent).IsPlaying = true;
                }
            }
        }

        public void PlayAudio(bool isLooping, bool isPlaying, int source, AudioBuffer audio, Vector3 position)
        {
            audio.Play(isLooping, isPlaying, source, position);
        }

        private void UpdateListener()
        {
            listenerPosition = camera.Position;
            listenerDirection = camera.Direction;
            listenerUp = camera.UpDirection;
            AL.Listener(ALListener3f.Position, ref listenerPosition);
            AL.Listener(ALListenerfv.Orientation, ref listenerDirection, ref listenerUp);
            AL.DistanceModel(ALDistanceModel.LinearDistance);
        }

        private void DeleteSource(Entity entity)
        {
            List<IComponent> components = entity.Components;

            IComponent audioComponent = components.Find(delegate (IComponent component)
            {
                return component.ComponentType == ComponentTypes.COMPONENT_AUDIO;
            });

            AL.DeleteSource(((ComponentAudio)audioComponent).AudioSource);
        }
    }
}
