using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Game_Engine.Components
{
    public class ComponentAI : IComponent
    {
        private List<Vector2> path;
        private Vector2 currentNode;
        private Vector3 currentNodeLocation;
        private Vector2 targetNode;
        private Vector3 targetNodeLocation;
        private Vector2 nextNode;
        private Vector3 nextNodeLocation;
        private int pathNodesTraversed;
        private bool targetSet;
        private bool nextSet;
        private int fov;
        private bool playerInFov;
        private Vector3 playerPosition;
        private bool canSeePlayer;
        private bool disabled;
        private bool reFindPath;
        private Vector3 currentPosition;
        private bool wasInFov;

        public ComponentAI(Vector2 currentNodeIn, Vector3 currentNodeLocationIn, int fovIn)
        {
            currentNode = currentNodeIn;
            currentNodeLocation = currentNodeLocationIn;
            path = new List<Vector2>();
            pathNodesTraversed = 0;
            targetSet = false;
            nextSet = false;
            playerInFov = false;
            canSeePlayer = true;
            fov = fovIn;
            disabled = false;
            reFindPath = false;
            currentPosition = currentNodeLocation;
        }

        public bool WasInFov
        {
            get { return wasInFov; }
            set { wasInFov = value; }
        }

        public List<Vector2> Path
        {
            get { return path; }
            set { path = value; }
        }

        public Vector2 CurrentNode
        {
            get { return currentNode; }
            set { currentNode = value; }
        }

        public Vector3 CurrentNodeLocation
        {
            get { return currentNodeLocation; }
            set { currentNodeLocation = value; }
        }

        public Vector2 TargetNode
        {
            get { return targetNode; }
            set { targetNode = value; }
        }

        public Vector3 TargetNodeLocation
        {
            get { return targetNodeLocation; }
            set { targetNodeLocation = value; }
        }

        public Vector2 NextNode
        {
            get { return nextNode; }
            set { nextNode = value; }
        }

        public Vector3 NextNodeLocation
        {
            get { return nextNodeLocation; }
            set { nextNodeLocation = value; }
        }

        public int PathNodesTraversed
        {
            get { return pathNodesTraversed; }
            set { pathNodesTraversed = value; }
        }

        public bool TargetSet
        {
            get { return targetSet; }
            set { targetSet = value; }
        }

        public bool NextSet
        {
            get { return nextSet; }
            set { nextSet = value; }
        }

        public int FOV
        {
            get { return fov; }
            set { fov = value; }
        }

        public bool PlayerInFov
        {
            get { return playerInFov; }
            set { playerInFov = value; }
        }

        public Vector3 PlayerPosition
        {
            get { return playerPosition; }
            set { playerPosition = value; }
        }

        public bool CanSeePlayer
        {
            get { return canSeePlayer; }
            set { canSeePlayer = value; }
        }

        public bool Disabled
        {
            get { return disabled; }
            set { disabled = value; }
        }

        public bool ReFindPath
        {
            get { return reFindPath; }
            set { reFindPath = value; }
        }

        public Vector3 CurrentPosition
        {
            get { return currentPosition; }
            set { currentPosition = value; }
        }

        public ComponentTypes ComponentType
        {
            get { return ComponentTypes.COMPONENT_AI; }
        }
    }
}
