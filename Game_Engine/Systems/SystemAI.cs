using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game_Engine.Objects;
using Game_Engine.Components;
using OpenTK;

namespace Game_Engine.Systems
{
    public class SystemAI : ISystem
    {
        const ComponentTypes MASK = (ComponentTypes.COMPONENT_AI | ComponentTypes.COMPONENT_TRANSFORM);

        private List<Entity> entityList;
        private Node[,] map;
        private int rows;
        private int columns;
        private Random rnd = new Random();
        private Entity player;

        public SystemAI(Node[,] mapIn, int rowsIn, int columnsIn, Entity playerIn)
        {
            entityList = new List<Entity>();
            map = mapIn;
            rows = rowsIn;
            columns = columnsIn;
            player = playerIn;
        }

        public string Name
        {
            get { return "SystemAI"; }
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

                IComponent aiComponent = components.Find(delegate (IComponent component)
                {
                    return component.ComponentType == ComponentTypes.COMPONENT_AI;
                });

                IComponent transformComponent = components.Find(delegate (IComponent component)
                {
                    return component.ComponentType == ComponentTypes.COMPONENT_TRANSFORM;
                });
                Matrix4 transform = ((ComponentTransform)transformComponent).Transform;

                if (((ComponentAI)aiComponent).PlayerInFov == true && ((ComponentAI)aiComponent).CanSeePlayer == true && !PlayerInFOV((ComponentAI)aiComponent, (ComponentTransform)transformComponent))
                {
                    ((ComponentAI)aiComponent).ReFindPath = true;
                }

                //Checks if the entity can see the player and if it can changes to follow mode else keeps following random path
                if (PlayerInFOV((ComponentAI)aiComponent, (ComponentTransform)transformComponent))
                {
                    ((ComponentAI)aiComponent).WasInFov = true;
                    ((ComponentAI)aiComponent).PlayerInFov = true;
                    ((ComponentAI)aiComponent).PlayerPosition = player.GetTransform().Translation;
                }
                else
                {
                    ((ComponentAI)aiComponent).PlayerInFov = false;
                    ((ComponentAI)aiComponent).CanSeePlayer = false;
                    
                }

                if (((ComponentAI)aiComponent).ReFindPath == true)
                {
                    FindNearestNode(((ComponentAI)aiComponent));
                    ((ComponentAI)aiComponent).TargetSet = false;
                    ((ComponentAI)aiComponent).NextSet = false;
                    ((ComponentAI)aiComponent).ReFindPath = false;
                }

                if (((ComponentAI)aiComponent).CanSeePlayer == false)
                {

                    //Checks that entity has a target node and assigns one if there is not one assigned then calculates path to new target
                    if (!((ComponentAI)aiComponent).TargetSet)
                    {
                        SetTargetNode(((ComponentAI)aiComponent));
                        SetRandomPath(((ComponentAI)aiComponent));
                    }

                    //Checks that the entity has a next node and assigns one if there is not one assigned
                    if (!((ComponentAI)aiComponent).NextSet)
                    {
                        SetNextNode(((ComponentAI)aiComponent));
                    }

                    //Checks if the entity has reached its target node and assigns new target if target is met then calculates path to new target
                    if (((ComponentAI)aiComponent).CurrentNode == ((ComponentAI)aiComponent).TargetNode)
                    {
                        SetTargetNode(((ComponentAI)aiComponent));
                        SetRandomPath(((ComponentAI)aiComponent));
                    }
                    else
                    {
                        //Checks if the entity has reached the next node on its path to its target node and assigns new next node if it is reached
                        if (((ComponentAI)aiComponent).CurrentNode == ((ComponentAI)aiComponent).NextNode)
                        {
                            SetNextNode(((ComponentAI)aiComponent));
                            ((ComponentAI)aiComponent).PathNodesTraversed++;
                        }
                    }
                }
            }
        }

        private void SetTargetNode(ComponentAI ai)
        {
            bool targetSet = false;
            int randomX = 0;
            int randomY = 0;

            //Randomly chooses a target node that is passable on the map grid
            while (!targetSet)
            {
                randomX = rnd.Next(1, rows);
                randomY = rnd.Next(1, columns);

                //Ensures that the chosen target node is passable, then sets new target node
                if (map[randomX, randomY].Passable == true)
                {
                    ai.TargetNode = new Vector2(randomX, randomY);
                    ai.TargetNodeLocation = map[randomX, randomY].Location;
                    targetSet = true;
                    ai.TargetSet = true;
                    ai.PathNodesTraversed = 0;
                }
            }
        }

        private void SetNextNode(ComponentAI ai)
        {
            //Updates current node location then finds next node in the path and sets it as the next node
            ai.CurrentNodeLocation = ai.NextNodeLocation;

            if (ai.Path.Count() != 0)
            {
                ai.NextNode = ai.Path[ai.PathNodesTraversed];
                ai.NextNodeLocation = map[(int)ai.Path[ai.PathNodesTraversed].X, (int)ai.Path[ai.PathNodesTraversed].Y].Location;
            }
            ai.NextSet = true;
        }

        private void SetRandomPath(ComponentAI ai)
        {
            Node currentNode = map[(int)ai.CurrentNode.X, (int)ai.CurrentNode.Y];
            ai.Path.Clear();

            //Calculates total direct distance between nodes and target node
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    if (i != ai.CurrentNode.X && j != ai.CurrentNode.Y)
                    {
                        map[i, j].H = new Vector3(map[i, j].Location - ai.TargetNodeLocation).Length;
                    }
                    map[i, j].ParentNode = null;
                    map[i, j].State = NodeState.Untested;
                }
            }

            //Returns true if a valid path to the target node can be found
            bool pathFound = SearchForPath(ai, currentNode);

            //If path is found, back tracks through the nodes in the path using the parent nodes to retrieve the complete path and then stores in a list from starting node to target node
            if (pathFound)
            {
                Node node = map[(int)ai.TargetNode.X, (int)ai.TargetNode.Y];
                while (node.ParentNode != null)
                {
                    ai.Path.Add(new Vector2(node.mapX, node.mapY));
                    node = node.ParentNode;
                }
                ai.Path.Reverse();
            }

            //Prints out path in console for debugging purposes
            foreach (Vector2 node in ai.Path)
            {
                //Console.WriteLine(node.X + ", " + node.Y);
            }
        }

        private bool SearchForPath(ComponentAI ai, Node currentNode)
        {
            //Gets list of passable neighbours and then sorts them by the estimated distance traveled if taking this node on the path (F) and sets current node to closed
            List<Node> passableNeighbours = GetPassableNeighbours(ai, currentNode);
            passableNeighbours.Sort((node1, node2) => node1.F.CompareTo(node2.F));
            currentNode.State = NodeState.Closed;

            //Recursively searches through each neighbour until the shortest path is found if a valid path is available
            foreach (Node passableNeighbour in passableNeighbours)
            {
                if (passableNeighbour.Location == ai.TargetNodeLocation)
                {
                    return true;
                }
                else
                {
                    if (SearchForPath(ai, passableNeighbour))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if the player is in the FOV of the drone
        /// </summary>
        /// <param name="ai"> the ai component of this entity </param>
        /// <param name="transform"> the transform component of this entity </param>
        /// <returns> true if the player is in the drones FOV </returns>
        private bool PlayerInFOV(ComponentAI ai, ComponentTransform transform)
        {
            //Finds the direction from the drone to the player
            Vector3 direction = (player.GetTransform().Translation - transform.Translation).Normalized();
            //Finds the angle between the target direction and the agent forward vector
            float angle = (float)(Math.Atan2(direction.X, direction.Z) - Math.Atan2(transform.Forward.X, transform.Forward.Z));

            //if the angle is over 180 in either direction then invert it to be a smaller angle in the opposite direction
            if (angle > Math.PI)
            {
                angle -= (float)(2 * Math.PI);
            }
            if (angle < -Math.PI)
            {
                angle += (float)(2 * Math.PI);
            }

            angle = MathHelper.RadiansToDegrees(angle);

            //if the player is in sight return true
            if (angle < ai.FOV && angle > -angle)
            {
                //Console.WriteLine("player spotted");
                return true;
            }
            return false;
        }

        private void FindNearestNode(ComponentAI ai)
        {
            Vector3 distance;
            Vector3 shortestDistance = new Vector3(0, 0, 0);
            Node closestNode = null;
            bool distanceSet = false;

            foreach(Node node in map)
            {
                distance = ai.CurrentPosition - node.Location;

                if(distanceSet)
                {
                    if(distance.Length < shortestDistance.Length)
                    {
                        shortestDistance = distance;
                        closestNode = node;
                    }
                }
                else
                {
                    distanceSet = true;
                    shortestDistance = distance;
                    closestNode = node;
                }
            }

            ai.CurrentNode = new Vector2(closestNode.mapX, closestNode.mapY);
            ai.CurrentNodeLocation = closestNode.Location;
        }

        private List<Node> GetPassableNeighbours(ComponentAI ai, Node currentNode)
        {
            List<Node> passableNeighbours = new List<Node>();

            //Gets all neighbouring nodes of current node
            List<Node> neighbours = GetNeighbours(ai, currentNode);

            //Gets all walkable nodes in the neighbouring nodes that are more efficient nodes to use in the path than the currently chosen node
            foreach (Node neighbour in neighbours)
            {
                //Ignores closed nodes
                if (neighbour.State == NodeState.Closed)
                {
                    continue;
                }

                //Ignores unpassable nodes
                if (!neighbour.Passable)
                {
                    continue;
                }

                //If node has already been tested, tests to see if traversing the neighbouring node will be more efficient from this node than its parents node, then adds it to passable neighbours if it is
                if (neighbour.State == NodeState.Open)
                {
                    float gDistance = new Vector3(neighbour.Location - neighbour.ParentNode.Location).Length;
                    float tempGDistance = currentNode.G + gDistance;
                    if (tempGDistance < neighbour.G)
                    {
                        neighbour.ParentNode = currentNode;
                        passableNeighbours.Add(neighbour);
                    }
                }
                //If node is currently untested, adds it to the passable neighbours list and sets it to Open State
                else
                {
                    neighbour.ParentNode = currentNode;
                    neighbour.State = NodeState.Open;
                    passableNeighbours.Add(neighbour);
                }
            }
            return passableNeighbours;
        }

        private List<Node> GetNeighbours(ComponentAI ai, Node currentNode)
        {
            List<Node> neighbours = new List<Node>();
            //Top
            neighbours.Add(map[(int)currentNode.mapX, (int)currentNode.mapY + 1]);
            //Bottom
            neighbours.Add(map[(int)currentNode.mapX, (int)currentNode.mapY - 1]);
            //Right
            neighbours.Add(map[(int)currentNode.mapX + 1, (int)currentNode.mapY]);
            //Left
            neighbours.Add(map[(int)currentNode.mapX - 1, (int)currentNode.mapY]);
            //Top Right
            //neighbours.Add(map[(int)currentNode.mapX + 1, (int)currentNode.mapY + 1]);
            //Bottom Right
            //neighbours.Add(map[(int)currentNode.mapX + 1, (int)currentNode.mapY - 1]);
            //Top Left
            //neighbours.Add(map[(int)currentNode.mapX - 1, (int)currentNode.mapY + 1]);
            //Bottom Left
            //neighbours.Add(map[(int)currentNode.mapX - 1, (int)currentNode.mapY - 1]);

            return neighbours;
        }
    }

    public class Node
    {
        private Node parentNode;

        public bool Passable { get; set; }
        public Vector3 Location { get; set; }
        public Node ParentNode
        {
            get { return parentNode; }
            set
            {
                parentNode = value;
                if (parentNode != null)
                {
                    G = parentNode.G + new Vector3(Location - parentNode.Location).Length; //Calculates new G value based on the new parent nodes G value
                }
            }
        }
        public NodeState State { get; set; } //Untested means not yet tested, Open means tested and still open for consideration for the path, Closed means tested and eliminated from consideration or already added to a path
        public float G { get; set; } //Total length of path from start node to this node
        public float H { get; set; } //Distance from node to target node
        public float F { get { return G + H; } } //Estimated total distance traveled if taking this node in the path
        public float mapX { get; set; } //X Location on map grid
        public float mapY { get; set; } //Y Location on map grid
    }

    public enum NodeState
    {
        Untested,
        Open,
        Closed
    }
}
