using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game_Engine.Objects;
using Game_Engine.Components;
using OpenTK;

namespace Game_Engine.Systems
{
    class SystemBoxCollision : ISystem
    {
        const ComponentTypes SPHEREMASK = (ComponentTypes.COMPONENT_SPHERE_COLLIDER);
        const ComponentTypes BOXMASK = (ComponentTypes.COMPONENT_BOX_COLLIDER);
        const ComponentTypes MOVINGMASK = (ComponentTypes.COMPONENT_VELOCITY);

        private List<Entity> entityList;
        private List<Entity> collidableEntities;

        private Dictionary<string, Vector3> oldPositions = new Dictionary<string, Vector3>();

        private bool ignoreEntity;

        public SystemBoxCollision()
        {
            entityList = new List<Entity>();
            collidableEntities = new List<Entity>();
        }

        public string Name
        {
            get { return "SystemBoxCollision"; }
        }

        public void AssignEntity(Entity entity)
        {
            if ((entity.Mask & BOXMASK) == BOXMASK)
            {
                collidableEntities.Add(entity);

                if ((entity.Mask & MOVINGMASK) == MOVINGMASK)
                {
                    entityList.Add(entity);
                }
            }
        }

        public void DestroyEntity(Entity entity)
        {
            entityList.Remove(entity);
            collidableEntities.Remove(entity);
        }

        public void OnAction()
        {
            foreach (Entity entity in entityList)
            {
                //Retrieves entities component list
                List<IComponent> components = entity.Components;

                //Retrieves entities box collider component
                IComponent boxColliderComponent = components.Find(delegate (IComponent component)
                {
                    return component.ComponentType == ComponentTypes.COMPONENT_BOX_COLLIDER;
                });
                ComponentBoxCollider boxCollider = ((ComponentBoxCollider)boxColliderComponent);

                //Retrives list of entities to ignore collisions with
                List<string> ignoreCollisions = boxCollider.IgnoreCollisionsWith;

                //Stores/retrieves the old positions of all the moving entities for collision detection
                Vector3 oldPosition;
                if (!oldPositions.TryGetValue(entity.Name, out oldPosition))
                {
                    oldPosition = entity.GetTransform().Translation;
                    oldPositions.Add(entity.Name, oldPosition);
                }

                //Checks if collisions are disabled
                if (boxCollider.Disabled != true)
                {
                    //Clears collided with list
                    boxCollider.CollidedWith.Clear();

                    //Does collision checks for each collidable entity
                    foreach (Entity collidedEntity in collidableEntities)
                    {
                        //Does a Box->Box collision check if the collidable entity has a box collider
                        if ((collidedEntity.Mask & BOXMASK) == BOXMASK)
                        {

                        }

                        //Does a Box->Sphere collision check if the collidable entity has a sphere collider
                        if ((collidedEntity.Mask & SPHEREMASK) == SPHEREMASK)
                        {

                        }
                    }
                }
            }
        }


    }
}
