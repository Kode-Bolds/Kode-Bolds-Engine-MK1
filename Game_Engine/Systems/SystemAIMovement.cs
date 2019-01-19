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
    public class SystemAIMovement : ISystem
    {
        const ComponentTypes MASK = (ComponentTypes.COMPONENT_TRANSFORM | ComponentTypes.COMPONENT_VELOCITY | ComponentTypes.COMPONENT_AI);

        List<Entity> entityList;
        SceneManager sceneManager;

        public SystemAIMovement(SceneManager sceneManagerIn)
        {
            sceneManager = sceneManagerIn;
            entityList = new List<Entity>();
        }

        public string Name
        {
            get { return "SystemAIMovement"; }
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

                IComponent aiComponent = components.Find(delegate (IComponent component)
                {
                    return component.ComponentType == ComponentTypes.COMPONENT_AI;
                });

                //Checks if the entity has an AI component, then applies movement based on AI path
                ComponentAI ai = ((ComponentAI)aiComponent);
                float velocity = ((ComponentVelocity)velocityComponent).Velocity;
                ComponentTransform transform = ((ComponentTransform)transformComponent);
                ai.CurrentPosition = transform.Translation;
                Vector3 direction;
                float angle;

                if (ai.Disabled != true)
                {
                    if (ai.CanSeePlayer)
                    {
                        //Finds direction towards target
                        direction = new Vector3(ai.PlayerPosition - transform.Translation).Normalized();
                        ai.TargetSet = false;
                        ai.NextSet = false;
                    }
                    else
                    {
                        //Finds the direction towards the target node
                        direction = new Vector3(ai.NextNodeLocation - transform.Translation).Normalized();
                    }

                    //Finds the angle between the target direction and the agent forward vector
                    angle = (float)(Math.Atan2(direction.X, direction.Z) - Math.Atan2(transform.Forward.X, transform.Forward.Z));

                    //if the angle is over 180 in either direction then invert it to be a smaller angle in the opposite direction
                    if (angle > Math.PI)
                    {
                        angle -= (float)(2 * Math.PI);
                    }
                    if (angle < -Math.PI)
                    {
                        angle += (float)(2 * Math.PI);
                    }

                    //Rotates the agent a fraction of the angle found each frame and moves the agent forward
                    transform.Rotation += new Vector3(0, angle / 5, 0);
                    transform.Translation += transform.Forward * velocity * sceneManager.dt;

                    if (!ai.CanSeePlayer)
                    {
                        //Checks if entity has reached the next node then sets current node to the next node causing the AI system to update to a new next node
                        if ((int)(transform.Translation.X + 0.4f) == ai.NextNodeLocation.X && transform.Translation.Y == ai.NextNodeLocation.Y && (int)(transform.Translation.Z + 0.4f) == ai.NextNodeLocation.Z)
                        {
                            ai.CurrentNode = ai.NextNode;
                        }

                        //Checks if entity has reached the target node then sets current node to the target node causing the AI system to update to a new target node
                        if ((int)(transform.Translation.X + 0.4f) == ai.TargetNodeLocation.X && (int)(transform.Translation.Y) == ai.TargetNodeLocation.Y && (int)(transform.Translation.Z + 0.4f) == ai.TargetNodeLocation.Z)
                        {
                            ai.CurrentNode = ai.TargetNode;
                        }
                    }
                }
            }
        }
    }
}
