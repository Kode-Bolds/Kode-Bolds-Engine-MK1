using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game_Engine.Objects;
using Game_Engine.Components;
using OpenTK;

namespace Game_Engine.Systems
{
    class SystemSphereCollision : ISystem
    {
        const ComponentTypes SPHEREMASK = (ComponentTypes.COMPONENT_SPHERE_COLLIDER);
        const ComponentTypes BOXMASK = (ComponentTypes.COMPONENT_BOX_COLLIDER);
        const ComponentTypes MOVINGMASK = (ComponentTypes.COMPONENT_VELOCITY);

        private List<Entity> entityList;
        private List<Entity> collidableEntities;

        private Dictionary<string, Vector3> oldPositions = new Dictionary<string, Vector3>();

        private bool ignoreEntity;

        /// <summary>
        /// Constructs a new sphere collision system for handling collisions involving entities with sphere colliders
        /// </summary>
        public SystemSphereCollision()
        {
            entityList = new List<Entity>();
            collidableEntities = new List<Entity>();
        }

        /// <summary>
        /// Returns the name of the system
        /// </summary>
        public string Name
        {
            get { return "SystemSphereCollision"; }
        }

        /// <summary>
        /// Assigns entities to be processed by the system if they match the appropriate mask type
        /// </summary>
        /// <param name="entity">The entity to be checked for assignment validity</param>
        public void AssignEntity(Entity entity)
        {
            if ((entity.Mask & SPHEREMASK) == SPHEREMASK)
            {
                collidableEntities.Add(entity);

                if ((entity.Mask & MOVINGMASK) == MOVINGMASK)
                {
                    entityList.Add(entity);
                }
            }
        }

        /// <summary>
        /// Removes entities from the systems assigned entity lists is the entity is destroyed
        /// </summary>
        /// <param name="entity">The entity to be destroyed</param>
        public void DestroyEntity(Entity entity)
        {
            entityList.Remove(entity);
            collidableEntities.Remove(entity);
        }

        /// <summary>
        /// Loops through every assigned entity applying sphere collision logic
        /// </summary>
        public void OnAction()
        {
            foreach (Entity entity in entityList)
            {
                //Retrieves entities component list
                List<IComponent> components = entity.Components;

                //Retrieves entities sphere collider
                IComponent sphereColliderComponent = components.Find(delegate (IComponent component)
                {
                    return component.ComponentType == ComponentTypes.COMPONENT_SPHERE_COLLIDER;
                });
                ComponentSphereCollider sphereCollider = ((ComponentSphereCollider)sphereColliderComponent);

                //Retrives list of entities to ignore collisions with
                List<string> ignoreCollisions = sphereCollider.IgnoreCollisionsWith;

                //Stores/retrieves the old positions of all the moving entities for collision detection
                Vector3 oldPosition;
                if (!oldPositions.TryGetValue(entity.Name, out oldPosition))
                {
                    oldPosition = entity.GetTransform().Translation;
                    oldPositions.Add(entity.Name, oldPosition);
                }

                //Checks if collisions are disabled
                if (sphereCollider.Disabled != true)
                {
                    //Clears collided with list
                    sphereCollider.CollidedWith.Clear();

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

                            //Does a Sphere->Box collision check if the collidable entity has a box collider
                            if ((collidedEntity.Mask & BOXMASK) == BOXMASK && ignoreEntity != true)
                            {

                            }

                            //Does a Sphere->Sphere collision check if the collidable entity has a sphere collider
                            if ((collidedEntity.Mask & SPHEREMASK) == SPHEREMASK && ignoreEntity != true)
                            {
                                bool collided = SphereSphereCollisionCheck(entity, collidedEntity, sphereCollider);

                                //If entity has collided with this collidable entity, sets the entities position to its old position and adds the collidable entity to the collidedWith list
                                if (collided == true)
                                {
                                    oldPositions.TryGetValue(entity.Name, out oldPosition);
                                    entity.GetTransform().Translation = oldPosition;
                                    sphereCollider.CollidedWith.Add(collidedEntity.Name);
                                    collidedEntity.GetCollidedWith().Add(entity.Name);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Executes a sphere->sphere collision check between two entities
        /// </summary>
        /// <param name="entity">Current entity to check collisions for</param>
        /// <param name="collidedEntity">Potentially collided with entity</param>
        /// <param name="sphereCollider">The sphere collider of the current entity</param>
        /// <returns></returns>
        private bool SphereSphereCollisionCheck(Entity entity, Entity collidedEntity, ComponentSphereCollider sphereCollider)
        {
            bool collided = false;

            //Retrieves sphere collider for potentially collided entity
            IComponent collidedEntityCollider = collidedEntity.Components.Find(delegate (IComponent component)
            {
                return component.ComponentType == ComponentTypes.COMPONENT_SPHERE_COLLIDER;
            });
            ComponentSphereCollider collidedSphereCollider = ((ComponentSphereCollider)collidedEntityCollider);

            //Radius of entity
            float radius1 = sphereCollider.Radius;

            //Radius of collided entity
            float radius2 = collidedSphereCollider.Radius;

            //Position of entity
            Vector3 position = entity.GetTransform().Translation;

            //Position of collided entity
            Vector3 collidedEntityPosition = collidedEntity.GetTransform().Translation;

            //Checks if the two radii are intersecting, causing a collision if they are
            if ((radius1 + radius2) > (collidedEntityPosition - position).Length)
            {
                collided = true;
                return true;
            }
            return collided;
        }
    }
}
