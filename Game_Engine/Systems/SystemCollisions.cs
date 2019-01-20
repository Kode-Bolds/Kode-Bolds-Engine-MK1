//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Game_Engine.Objects;
//using Game_Engine.Components;
//using OpenTK;

//namespace Game_Engine.Systems
//{
//    public class SystemCollisions : ISystem
//    {
//        const ComponentTypes SPHEREMASK = (ComponentTypes.COMPONENT_SPHERE_COLLIDER);
//        const ComponentTypes BOXMASK = (ComponentTypes.COMPONENT_BOX_COLLIDER);
//        const ComponentTypes MOVINGMASK = (ComponentTypes.COMPONENT_VELOCITY);

//        private List<Entity> entityList;
//        private List<Entity> collidableEntities;

//        private Dictionary<string, Vector3> oldPositions = new Dictionary<string, Vector3>();

//        private bool ignoreEntity;

//        public SystemCollisions()
//        {
//            entityList = new List<Entity>();
//            collidableEntities = new List<Entity>();
//        }

//        public string Name
//        {
//            get { return "SystemCollisions"; }
//        }

//        public void AssignEntity(Entity entity)
//        {
//            if ((entity.Mask & BOXMASK) == BOXMASK || (entity.Mask & SPHEREMASK) == SPHEREMASK)
//            {
//                collidableEntities.Add(entity);

//                if ((entity.Mask & MOVINGMASK) == MOVINGMASK)
//                {
//                    entityList.Add(entity);
//                }
//            }
//        }

//        public void DestroyEntity(Entity entity)
//        {
//            entityList.Remove(entity);
//            collidableEntities.Remove(entity);
//        }

//        public void OnAction()
//        {
//            foreach (Entity entity in entityList)
//            {
//                List<IComponent> components = entity.Components;

//                IComponent transformComponent = components.Find(delegate (IComponent component)
//                {
//                    return component.ComponentType == ComponentTypes.COMPONENT_TRANSFORM;
//                });

//                IComponent boxColliderComponent = components.Find(delegate (IComponent component)
//                {
//                    return component.ComponentType == ComponentTypes.COMPONENT_BOX_COLLIDER;
//                });
//                ComponentBoxCollider boxCollider = ((ComponentBoxCollider)boxColliderComponent);

//                IComponent sphereColliderComponent = components.Find(delegate (IComponent component)
//                {
//                    return component.ComponentType == ComponentTypes.COMPONENT_SPHERE_COLLIDER;
//                });
//                ComponentSphereCollider sphereCollider = ((ComponentSphereCollider)sphereColliderComponent);
//                List<string> ignoreCollisions = ((ComponentSphereCollider)sphereColliderComponent).IgnoreCollisionsWith;

//                IComponent aiComponent = components.Find(delegate (IComponent component)
//                {
//                    return component.ComponentType == ComponentTypes.COMPONENT_AI;
//                });


//                //Stores the old positions of all the moving entities for collision detection
//                Vector3 oldPosition;
//                if (!oldPositions.TryGetValue(entity.Name, out oldPosition))
//                {
//                    oldPosition = entity.GetTransform().Translation;
//                    oldPositions.Add(entity.Name, oldPosition);
//                }

//                //Checks if entity has box collider and executes box collision checks if it has
//                if (boxColliderComponent != null)
//                {
//                    //Checks if collisions are disabled
//                    if (boxCollider.Disabled != true)
//                    {
//                        foreach (Entity collidedEntity in collidableEntities)
//                        {
//                            if ((collidedEntity.Mask & BOXMASK) == BOXMASK)
//                            {

//                            }

//                            if ((collidedEntity.Mask & SPHEREMASK) == SPHEREMASK)
//                            {

//                            }
//                        }
//                    }
//                }

//                //Checks if entity has sphere collider and executes sphere collision checks if it has 
//                if (sphereColliderComponent != null)
//                {
//                    //Clears collided with list
//                    sphereCollider.CollidedWith.Clear();

//                    //Checks if collisions are disabled
//                    if(sphereCollider.Disabled != true)
//                    {
//                        //Checks if entity has ai component and if player is within fov then sets bool to true, and will be set back to false if a collision is dectected between the entity and the player
//                        if (aiComponent != null && ((ComponentAI)aiComponent).PlayerInFov == true)
//                        {
//                            ((ComponentAI)aiComponent).CanSeePlayer = true;
//                        }

//                        foreach (Entity collidedEntity in collidableEntities)
//                        {
//                            ignoreEntity = false;
//                            //Checks that entity isn't trying to collide with itself
//                            if (entity.Name != collidedEntity.Name)
//                            {
//                                //Checks that entity isn't trying to collide with ignored entities
//                                foreach (string name in ignoreCollisions)
//                                {
//                                    if (collidedEntity.Name.Contains(name))
//                                    {
//                                        ignoreEntity = true;
//                                    }
//                                }


//                                //If current collidable entity has a box collider then executes Sphere on Box collision checks
//                                if ((collidedEntity.Mask & BOXMASK) == BOXMASK && ignoreEntity != true)
//                                {
//                                    //Current position of current entity
//                                    Vector2 position = entity.GetTransform().Translation.Xz;

//                                    //Old position of current entity
//                                    Vector3 oldPositionV3;
//                                    oldPositions.TryGetValue(entity.Name, out oldPositionV3);
//                                    Vector2 oldPositionV2 = oldPositionV3.Xz;

//                                    //Collision check
//                                    bool collided = SphereBoxCollisionCheck(collidedEntity, sphereColliderComponent, position, oldPositionV2);


//                                    //If entity has ai component does a raycast check to see if there is an obstacle between the entity and the target entity
//                                    if (aiComponent != null && ((ComponentAI)aiComponent).PlayerInFov == true && ((ComponentAI)aiComponent).CanSeePlayer == true)
//                                    {
//                                        if (SphereBoxCollisionCheck(collidedEntity, sphereColliderComponent, ((ComponentAI)aiComponent).PlayerPosition.Xz, position))
//                                        {
//                                            ((ComponentAI)aiComponent).CanSeePlayer = false;
//                                        }
//                                    }

//                                    //If entity has collided with this collidable entity, sets the entities position to its old position and adds the collidable entity to the collidedWith list
//                                    if (collided == true)
//                                    {
//                                        oldPositions.TryGetValue(entity.Name, out oldPosition);
//                                        entity.GetTransform().Translation = oldPosition;
//                                        sphereCollider.CollidedWith.Add(collidedEntity.Name);
//                                        collidedEntity.GetCollidedWith().Add(entity.Name);

//                                    }
//                                }


//                                //If current collidable entity has a sphere collider then executes Sphere on Sphere collision checks
//                                if ((collidedEntity.Mask & SPHEREMASK) == SPHEREMASK && ignoreEntity != true)
//                                {
//                                    //Collision check
//                                    bool collided = SphereSphereCollisionCheck(entity, collidedEntity, sphereColliderComponent);

//                                    //If entity has collided with this collidable entity, sets the entities position to its old position and adds the collidable entity to the collidedWith list
//                                    if (collided == true)
//                                    {
//                                        oldPositions.TryGetValue(entity.Name, out oldPosition);
//                                        entity.GetTransform().Translation = oldPosition;
//                                        sphereCollider.CollidedWith.Add(collidedEntity.Name);
//                                        collidedEntity.GetCollidedWith().Add(entity.Name);
//                                    }
//                                }
//                            }
//                        }
//                    }
//                }
//                //Keeps stored old positions up to date every frame
//                oldPosition = entity.GetTransform().Translation;
//                oldPositions.Remove(entity.Name);
//                oldPositions.Add(entity.Name, oldPosition);
//            }
//        }

//        private bool SphereBoxCollisionCheck(Entity collidedEntity, IComponent sphereColliderComponent, Vector2 position, Vector2 oldPosition)
//        {
//            bool collided = false;

//            //Radius of current entity
//            float radius = ((ComponentSphereCollider)sphereColliderComponent).Radius;

//            //Movement vector and normal for current entities movement
//            Vector2 movement = position - oldPosition;
//            Vector2 movementNormal = movement.PerpendicularLeft;

//            //Retrieves box collider for potentially collided entity
//            IComponent collidedEntityCollider = collidedEntity.Components.Find(delegate (IComponent component)
//            {
//                return component.ComponentType == ComponentTypes.COMPONENT_BOX_COLLIDER;
//            });
//            ComponentBoxCollider collidedBoxCollider = ((ComponentBoxCollider)collidedEntityCollider);

//            //Direction Vectors for each face of box collider
//            Vector2 top = new Vector2(collidedBoxCollider.TopRight.X - collidedBoxCollider.TopLeft.X, collidedBoxCollider.TopRight.Y - collidedBoxCollider.TopLeft.Y);
//            Vector2 right = new Vector2(collidedBoxCollider.BottomRight.X - collidedBoxCollider.TopRight.X, collidedBoxCollider.BottomRight.Y - collidedBoxCollider.TopRight.Y);
//            Vector2 bottom = new Vector2(collidedBoxCollider.BottomLeft.X - collidedBoxCollider.BottomRight.X, collidedBoxCollider.BottomLeft.Y - collidedBoxCollider.BottomRight.Y);
//            Vector2 left = new Vector2(collidedBoxCollider.TopLeft.X - collidedBoxCollider.BottomLeft.X, collidedBoxCollider.TopLeft.Y - collidedBoxCollider.BottomLeft.Y);

//            //Normal (Perpendicular) Vectors for each face of box collider
//            Vector2 topNormal = top.PerpendicularLeft;
//            Vector2 rightNormal = right.PerpendicularLeft;
//            Vector2 bottomNormal = bottom.PerpendicularLeft;
//            Vector2 leftNormal = left.PerpendicularLeft;



//            //Top face of box collider collision checks
//            float positionDotProduct = Vector2.Dot(new Vector2(position.X - collidedBoxCollider.TopLeft.X, (position.Y + radius) - collidedBoxCollider.TopLeft.Y), topNormal);
//            float oldPositionDotProduct = Vector2.Dot(new Vector2(oldPosition.X - collidedBoxCollider.TopLeft.X, (oldPosition.Y + radius) - collidedBoxCollider.TopLeft.Y), topNormal);

//            float wallPoint1DotProduct;
//            float wallPoint2DotProduct;

//            //Checks if entity has crossed the wall (unlimited line), if true then potential collision has occured
//            if (positionDotProduct * oldPositionDotProduct < 0)
//            {

//                wallPoint1DotProduct = Vector2.Dot(collidedBoxCollider.TopLeft - new Vector2(oldPosition.X + radius, oldPosition.Y + radius), movementNormal);
//                wallPoint2DotProduct = Vector2.Dot(collidedBoxCollider.TopRight - new Vector2(oldPosition.X - radius, oldPosition.Y + radius), movementNormal);

//                //Checks if the entity crossed the line within the bounds of the wall segment, if true then collision has occured with this wall segment
//                if (wallPoint1DotProduct * wallPoint2DotProduct < 0)
//                {
//                    //Console.WriteLine("Collision");
//                    collided = true;
//                    return true;
//                }
//            }

//            //Right face of box collider collision checks
//            positionDotProduct = Vector2.Dot(new Vector2((position.X - radius) - collidedBoxCollider.TopRight.X, position.Y - collidedBoxCollider.TopRight.Y), rightNormal);
//            oldPositionDotProduct = Vector2.Dot(new Vector2((oldPosition.X - radius) - collidedBoxCollider.TopRight.X, oldPosition.Y - collidedBoxCollider.TopRight.Y), rightNormal);

//            //Checks if entity has crossed the wall (unlimited line), if true then potential collision has occured
//            if (positionDotProduct * oldPositionDotProduct < 0)
//            {

//                wallPoint1DotProduct = Vector2.Dot(collidedBoxCollider.TopRight - new Vector2(oldPosition.X - radius, oldPosition.Y + radius), movementNormal);
//                wallPoint2DotProduct = Vector2.Dot(collidedBoxCollider.BottomRight - new Vector2(oldPosition.X - radius, oldPosition.Y - radius), movementNormal);

//                //Checks if the entity crossed the line within the bounds of the wall segment, if true then collision has occured with this wall segment
//                if (wallPoint1DotProduct * wallPoint2DotProduct < 0)
//                {
//                    //Console.WriteLine("Collision");
//                    collided = true;
//                    return true;
//                }
//            }

//            //Bottom face of box collider collision checks
//            positionDotProduct = Vector2.Dot(new Vector2(position.X - collidedBoxCollider.BottomRight.X, (position.Y - radius) - collidedBoxCollider.BottomRight.Y), bottomNormal);
//            oldPositionDotProduct = Vector2.Dot(new Vector2(oldPosition.X - collidedBoxCollider.BottomRight.X, (oldPosition.Y - radius) - collidedBoxCollider.BottomRight.Y), bottomNormal);

//            //Checks if entity has crossed the wall (unlimited line), if true then potential collision has occured
//            if (positionDotProduct * oldPositionDotProduct < 0)
//            {

//                wallPoint1DotProduct = Vector2.Dot(collidedBoxCollider.BottomRight - new Vector2(oldPosition.X - radius, oldPosition.Y - radius), movementNormal);
//                wallPoint2DotProduct = Vector2.Dot(collidedBoxCollider.BottomLeft - new Vector2(oldPosition.X + radius, oldPosition.Y - radius), movementNormal);

//                //Checks if the entity crossed the line within the bounds of the wall segment, if true then collision has occured with this wall segment
//                if (wallPoint1DotProduct * wallPoint2DotProduct < 0)
//                {
//                    //Console.WriteLine("Collision");
//                    collided = true;
//                    return true;
//                }
//            }

//            //Left face of box collider collision checks
//            positionDotProduct = Vector2.Dot(new Vector2((position.X + radius) - collidedBoxCollider.BottomLeft.X, position.Y - collidedBoxCollider.BottomLeft.Y), leftNormal);
//            oldPositionDotProduct = Vector2.Dot(new Vector2((oldPosition.X + radius) - collidedBoxCollider.BottomLeft.X, oldPosition.Y - collidedBoxCollider.BottomLeft.Y), leftNormal);

//            //Checks if entity has crossed the wall (unlimited line), if true then potential collision has occured
//            if (positionDotProduct * oldPositionDotProduct < 0)
//            {

//                wallPoint1DotProduct = Vector2.Dot(collidedBoxCollider.BottomLeft - new Vector2(oldPosition.X + radius, oldPosition.Y - radius), movementNormal);
//                wallPoint2DotProduct = Vector2.Dot(collidedBoxCollider.TopLeft - new Vector2(oldPosition.X + radius, oldPosition.Y + radius), movementNormal);

//                //Checks if the entity crossed the line within the bounds of the wall segment, if true then collision has occured with this wall segment
//                if (wallPoint1DotProduct * wallPoint2DotProduct < 0)
//                {
//                    //Console.WriteLine("Collision");
//                    collided = true;
//                    return true;
//                }
//            }

//            return collided;
//        }

//        private bool SphereSphereCollisionCheck(Entity entity, Entity collidedEntity, IComponent sphereColliderComponent)
//        {
//            bool collided = false;

//            //Retrieves sphere collider for potentially collided entity
//            IComponent collidedEntityCollider = collidedEntity.Components.Find(delegate (IComponent component)
//            {
//                return component.ComponentType == ComponentTypes.COMPONENT_SPHERE_COLLIDER;
//            });
//            ComponentSphereCollider collidedSphereCollider = ((ComponentSphereCollider)collidedEntityCollider);

//            //Retrieves transform for potentially collided entity
//            IComponent collidedEntityTransformComponent = collidedEntity.Components.Find(delegate (IComponent component)
//            {
//                return component.ComponentType == ComponentTypes.COMPONENT_TRANSFORM;
//            });
//            ComponentTransform collidedEntityTransform = ((ComponentTransform)collidedEntityTransformComponent);

//            //Radius of entity
//            float radius1 = ((ComponentSphereCollider)sphereColliderComponent).Radius;

//            //Radius of collided entity
//            float radius2 = collidedSphereCollider.Radius;

//            //Position of entity
//            Vector2 position = entity.GetTransform().Translation.Xz;

//            //Position of collided entity
//            Vector2 collidedEntityPosition = collidedEntityTransform.Translation.Xz;

//            //Checks if the two radii are intersecting, causing a collision if they are
//            if ((radius1 + radius2) > (collidedEntityPosition - position).Length)
//            {
//                collided = true;
//                return true;
//            }
//            return collided;
//        }
//    }
//}
