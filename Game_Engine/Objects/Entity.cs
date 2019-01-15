using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Game_Engine.Components;

namespace Game_Engine.Objects
{
    public class Entity
    {
        string name;
        List<IComponent> componentList = new List<IComponent>();
        ComponentTypes mask;

        public Entity(string name)
        {
            this.name = name;
        }

        public void AddComponent(IComponent component)
        {
            Debug.Assert(component != null, "Component cannot be null");

            componentList.Add(component);
            mask |= component.ComponentType;
        }

        public String Name
        {
            get { return name; }
        }

        public ComponentTypes Mask
        {
            get { return mask; }
        }

        public List<IComponent> Components
        {
            get { return componentList; }
        }

        public ComponentTransform GetTransform()
        {
            return (ComponentTransform)componentList.Find(delegate (IComponent e)
            {
                return e.ComponentType == ComponentTypes.COMPONENT_TRANSFORM;
            });
        }

        public ComponentVelocity GetVelocity()
        {
            return (ComponentVelocity)componentList.Find(delegate (IComponent e)
             {
                 return e.ComponentType == ComponentTypes.COMPONENT_VELOCITY;
             });
        }

        public List<string> GetCollidedWith()
        {
            List<string> collidedWith = new List<string>();

            //Checks if entity has a sphere collider or a box collider
            if((mask & ComponentTypes.COMPONENT_SPHERE_COLLIDER) == ComponentTypes.COMPONENT_SPHERE_COLLIDER)
            {
                //Gets sphere collider component from entity
                ComponentSphereCollider sphereCollider = (ComponentSphereCollider)componentList.Find(delegate (IComponent e)
                {
                    return e.ComponentType == ComponentTypes.COMPONENT_SPHERE_COLLIDER;
                });

                //Gets the collidedWith list from the sphere collider component
                collidedWith = sphereCollider.CollidedWith;
            }
            else
            {
                if ((mask & ComponentTypes.COMPONENT_BOX_COLLIDER) == ComponentTypes.COMPONENT_BOX_COLLIDER)
                {
                    //Gets sphere collider component from entity
                    ComponentBoxCollider boxCollider = (ComponentBoxCollider)componentList.Find(delegate (IComponent e)
                    {
                        return e.ComponentType == ComponentTypes.COMPONENT_BOX_COLLIDER;
                    });

                    //Gets the collidedWith list from the sphere collider component
                    collidedWith = boxCollider.CollidedWith;
                }
            }

            return collidedWith;
        }

        public ComponentBoxCollider GetComponentBoxCollider()
        {
            //Checks if entity has a box collider
            if ((mask & ComponentTypes.COMPONENT_BOX_COLLIDER) == ComponentTypes.COMPONENT_BOX_COLLIDER)
            {
                //Gets sphere collider component from entity
                ComponentBoxCollider boxCollider = (ComponentBoxCollider)componentList.Find(delegate (IComponent e)
                {
                    return e.ComponentType == ComponentTypes.COMPONENT_BOX_COLLIDER;
                });
                return boxCollider;
            }
            return null;
        }

        public ComponentSphereCollider GetComponentSphereCollider()
        {
            //Checks if entity has a sphere collider
            if ((mask & ComponentTypes.COMPONENT_SPHERE_COLLIDER) == ComponentTypes.COMPONENT_SPHERE_COLLIDER)
            {
                //Gets sphere collider component from entity
                ComponentSphereCollider sphereCollider = (ComponentSphereCollider)componentList.Find(delegate (IComponent e)
                {
                    return e.ComponentType == ComponentTypes.COMPONENT_SPHERE_COLLIDER;
                });
                return sphereCollider;
            }
            return null;
        }

        public ComponentAI GetComponentAI()
        {
            //Checks if entity has an AI component
            if((mask & ComponentTypes.COMPONENT_AI) == ComponentTypes.COMPONENT_AI)
            {
                //Gets AiComponent from entity
                ComponentAI aiComponent = (ComponentAI)componentList.Find(delegate (IComponent e)
                {
                    return e.ComponentType == ComponentTypes.COMPONENT_AI;
                });
                return aiComponent;
            }
            return null;
        }

        public ComponentHealth GetHealth()
        {
            return (ComponentHealth)componentList.Find(delegate (IComponent e)
            {
                return e.ComponentType == ComponentTypes.COMPONENT_HEALTH;
            });
        }

        public ComponentAudio GetAudio()
        {
            return (ComponentAudio)componentList.Find(delegate (IComponent e)
            {
                return e.ComponentType == ComponentTypes.COMPONENT_AUDIO;
            });
        }
    }
}
