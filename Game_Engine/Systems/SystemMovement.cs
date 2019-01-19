using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game_Engine.Objects;
using Game_Engine.Components;
using Game_Engine.Managers;
using OpenTK;

namespace Game_Engine.Systems
{
    public class SystemMovement : ISystem
    {
        const ComponentTypes MASK = (ComponentTypes.COMPONENT_TRANSFORM | ComponentTypes.COMPONENT_VELOCITY);

        List<Entity> entityList;
        SceneManager sceneManager;

        public SystemMovement(SceneManager sceneManagerIn)
        {
            sceneManager = sceneManagerIn;
            entityList = new List<Entity>();
        }

        public string Name
        {
            get { return "SystemMovement"; }
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
            entityList.Remove(entity);
        }

        public void OnAction()
        {
            foreach (Entity entity in entityList)
            {
                List<IComponent> components = entity.Components;

                IComponent transformComponent = components.Find(delegate (IComponent component)
                {
                    return component.ComponentType == ComponentTypes.COMPONENT_TRANSFORM;
                });

                IComponent velocityComponent = components.Find(delegate (IComponent component)
                {
                    return component.ComponentType == ComponentTypes.COMPONENT_VELOCITY;
                });

                if (((ComponentTransform)transformComponent).SetTransform == false)
                {
                    UpdateTransform((ComponentTransform)transformComponent);
                    ((ComponentTransform)transformComponent).SetTransform = true;
                }

                UpdateTransform((ComponentTransform)transformComponent);
            }
        }

        public void UpdateTransform(ComponentTransform transform)
        {
            Matrix4 scaleMat = Matrix4.CreateScale(transform.Scale);
            Matrix4 rotateMat = Matrix4.CreateRotationX(transform.Rotation.X) * Matrix4.CreateRotationY(transform.Rotation.Y) * Matrix4.CreateRotationZ(transform.Rotation.Z);
            Matrix4 translateMat = Matrix4.CreateTranslation(transform.Translation);

            transform.Transform = scaleMat * rotateMat * translateMat;

            transform.Up = new Vector3(transform.Transform[1, 0], transform.Transform[1, 1], transform.Transform[1, 2]).Normalized();
            transform.Forward = -new Vector3(transform.Transform[2, 0], transform.Transform[2, 1], transform.Transform[2, 2]).Normalized();
            transform.Right = new Vector3(transform.Transform[0, 0], transform.Transform[0, 1], transform.Transform[0, 2]).Normalized();
        }
    }
}
