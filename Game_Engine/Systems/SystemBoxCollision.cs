using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game_Engine.Objects;
using Game_Engine.Components;
using OpenTK;

namespace Game_Engine.Systems
{
    public class SystemBoxCollision : ISystem
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
                        ignoreEntity = false;

                        //Checks that the entity isn't trying to collide with itself
                        if (entity.Name != collidedEntity.Name)
                        {
                            //Checks that entity isn't trying to collide with ignored entities
                            foreach (string name in ignoreCollisions)
                            {
                                if (collidedEntity.Name.Contains(name))
                                {
                                    ignoreEntity = true;
                                }
                            }
                        }

                        //Does a Box->Box collision check if the collidable entity has a box collider
                        if ((collidedEntity.Mask & BOXMASK) == BOXMASK && ignoreEntity != true)
                        {
                            bool collided = BoxBoxCollisionCheck(entity, collidedEntity, boxCollider);

                            //If entity has collided with this collidable entity, sets the entities position to its old position and adds the collidable entity to the collidedWith list
                            if (collided == true)
                            {
                                oldPositions.TryGetValue(entity.Name, out oldPosition);
                                entity.GetTransform().Translation = oldPosition;
                                boxCollider.CollidedWith.Add(collidedEntity.Name);
                                collidedEntity.GetCollidedWith().Add(entity.Name);
                            }
                        }

                        //Does a Box->Sphere collision check if the collidable entity has a sphere collider
                        if ((collidedEntity.Mask & SPHEREMASK) == SPHEREMASK && ignoreEntity != true)
                        {
                            bool collided = BoxSphereCollisionCheck(entity, collidedEntity, boxCollider);

                            //If entity has collided with this collidable entity, sets the entities position to its old position and adds the collidable entity to the collidedWith list
                            if (collided == true)
                            {
                                oldPositions.TryGetValue(entity.Name, out oldPosition);
                                entity.GetTransform().Translation = oldPosition;
                                boxCollider.CollidedWith.Add(collidedEntity.Name);
                                collidedEntity.GetCollidedWith().Add(entity.Name);
                            }
                        }
                    }
                }
                //Keeps stored old positions up to date every frame
                oldPosition = entity.GetTransform().Translation;
                oldPositions.Remove(entity.Name);
                oldPositions.Add(entity.Name, oldPosition);
            }
        }

        /// <summary>
        /// Executes a box->box collision check between two entities
        /// </summary>
        /// <param name="entity">Current entity to check collisions for</param>
        /// <param name="collidedEntity">Potentially collided with entity</param>
        /// <param name="boxCollider">The box collider of the current entity</param>
        /// <returns></returns>
        private bool BoxBoxCollisionCheck(Entity entity, Entity collidedEntity, ComponentBoxCollider boxCollider)
        {
            //Retrieves box collider for potentially collided entity
            IComponent collidedEntityCollider = collidedEntity.Components.Find(delegate (IComponent component)
            {
                return component.ComponentType == ComponentTypes.COMPONENT_BOX_COLLIDER;
            });
            ComponentBoxCollider collidedBoxCollider = ((ComponentBoxCollider)collidedEntityCollider);

            //Position of entity
            Vector3 position = entity.GetTransform().Translation;

            //Position of collided entity
            Vector3 collidedEntityPosition = collidedEntity.GetTransform().Translation;

            //Compares the two box colliders postions modified by their width, height and depth to see if there is any intersection
            return (position.X < collidedEntityPosition.X + collidedBoxCollider.Width &&
                position.X + boxCollider.Width > collidedEntityPosition.X &&
                position.Y < collidedEntityPosition.Y + collidedBoxCollider.Height &&
                position.Y + boxCollider.Height > collidedEntityPosition.Y &&
                position.Z < collidedEntityPosition.Z + collidedBoxCollider.Depth &&
                position.Z + boxCollider.Depth > collidedEntityPosition.Z);
        }

        /// <summary>
        /// Executes a box->sphere collision check between two entities
        /// </summary>
        /// <param name="entity">Current entity to check collisions for</param>
        /// <param name="collidedEntity">Potentially collided with entity</param>
        /// <param name="boxCollider">The box collider of the current entity</param>
        /// <returns></returns>
        private bool BoxSphereCollisionCheck(Entity entity, Entity collidedEntity, ComponentBoxCollider boxCollider)
        {
            //Retrieves sphere collider for potentially collided entity
            IComponent collidedEntityCollider = collidedEntity.Components.Find(delegate (IComponent component)
            {
                return component.ComponentType == ComponentTypes.COMPONENT_SPHERE_COLLIDER;
            });
            ComponentSphereCollider collidedsphereCollider = ((ComponentSphereCollider)collidedEntityCollider);

            //Radius squared of sphere collider component for entity
            float radiusSquared = collidedsphereCollider.Radius * collidedsphereCollider.Radius;

            //Position of entity
            Vector3 position = entity.GetTransform().Translation;

            //Position of collided entity
            Vector3 collidedEntityPosition = collidedEntity.GetTransform().Translation;

            //Width height and depth of box collider component for potentially collided entity
            Vector3 boxMax = new Vector3(position.X + boxCollider.Width, position.Y + boxCollider.Height, position.Z + boxCollider.Depth);
            Vector3 boxMin = new Vector3(position.X - boxCollider.Width, position.Y - boxCollider.Height, position.Z - boxCollider.Depth);

            //Calculates distance between the sphere and the closest point on the bounding box
            float distance = 0;
            for (int i = 0; i < 3; i++)
            {
                if (collidedEntityPosition[i] < boxMin[i])
                {
                    distance += ((collidedEntityPosition[i] - boxMin[i]) * (collidedEntityPosition[i] - boxMin[i]));
                }
                else if (collidedEntityPosition[i] > boxMax[i])
                {
                    distance += ((collidedEntityPosition[i] - boxMax[i]) * (collidedEntityPosition[i] - boxMax[i]));
                }
            }

            //If distance between sphere and point is less than or equal to radius, returns true, else false
            return distance <= radiusSquared;
        }
    }
}
