using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace OpenGL_Game
{
    static class Maze
    {
        public static int columns = 25;
        public static int rows = 25;
        public static int roomSize = 6; // must be even
        private static System.Random rnd = new System.Random();
        public static Vector2 start;

        //enum used to store each tiles contents
        public enum tile
        {
            CORRIDOR,
            WALL,
            ROOM
        }

        //enum used to select directions for carving
        private enum directions
        {
            NORTH,
            SOUTH,
            EAST,
            WEST
        }

        private static tile[,] grid = new tile[columns, rows];
        private static bool[,] visited = new bool[columns, rows];

        //generate a maze
        public static tile[,] mazeGenerator()
        {
            //reset arrays
            InitialiseGrid();
            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    visited[x, y] = false;
                }

            }

            //set all edges to visited
            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    if ((x == 0) || (x == columns - 1) || (y == 0) || (y == rows - 1))
                    {
                        visited[x, y] = true;
                    }
                }
            }

            addRoom();

            start = getRandomPos();

            // if youve been to this square already (its a boundary) then find a new square
            while ((visited[(int)start.X, (int)start.Y] != false) && (grid[(int)start.X, (int)start.Y] == tile.ROOM))
            {
                start = getRandomPos();
            }

            Vector2 currentCell = start;
            visited[(int)start.X, (int)start.Y] = true;
            grid[(int)currentCell.X, (int)currentCell.Y] = tile.CORRIDOR;
            carveWall(currentCell);
            cleanUpMaze();
            loopDeadEnds();
            addDoors();

            return grid;
        }

        /// <summary>
        /// Gets a random number and ensures it is odd
        /// </summary>
        /// <returns> a random position in the maze as a vector2 </returns>
        private static Vector2 getRandomPos()
        {
            Vector2 position;
            position.X = rnd.Next(1, columns - 2);
            position.Y = rnd.Next(1, rows - 2);

            if (position.X % 2 == 0)
            {
                position.X += 1;
            }
            if (position.Y % 2 == 0)
            {
                position.Y += 1;
            }

            return position;
        }

        /// <summary>
        /// Sets the whole grid to walls
        /// </summary>
        private static void InitialiseGrid()
        {
            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    grid[x, y] = tile.WALL;
                }
            }

        }

        /// <summary>
        /// Adds the centre room
        /// </summary>
        private static void addRoom()
        {
            for (int i = 0; i <= roomSize; i++)
            {
                for (int j = 0; j <= roomSize; j++)
                {
                    grid[(columns - roomSize) / 2 + i, (rows - roomSize) / 2 + j] = tile.ROOM;
                    visited[(columns - roomSize) / 2 + i, (rows - roomSize) / 2 + j] = true;
                }
            }
        }

        /// <summary>
        /// Randomly adds doors to the room in the centre of the maze
        /// </summary>
        private static void addDoors()
        {
            int doorChance = 1;
            int connections = 0;

            //loops until there are at least 2 doorways (more may occur)
            while (connections <= 2)
            {
                for (int x = 2; x < columns - 2; x++)
                {
                    for (int y = 2; y < rows - 2; y++)
                    {
                        if (grid[x, y] == tile.ROOM)
                        {
                            //random chance to create a door
                            if (rnd.Next(0, 100) < doorChance)
                            {
                                //create a door in a direction that leads to a corridor
                                if ((grid[x + 1, y] == tile.WALL) && (grid[x + 2, y] == tile.CORRIDOR))
                                {
                                    grid[x + 1, y] = tile.CORRIDOR;
                                    connections++;
                                }
                                else if ((grid[x, y + 1] == tile.WALL) && (grid[x, y + 2] == tile.CORRIDOR))
                                {
                                    grid[x, y + 1] = tile.CORRIDOR;
                                    connections++;
                                }
                                else if ((grid[x - 1, y] == tile.WALL) && (grid[x - 2, y] == tile.CORRIDOR))
                                {
                                    grid[x - 1, y] = tile.CORRIDOR;
                                    connections++;
                                }
                                else if ((grid[x, y - 1] == tile.WALL) && (grid[x, y - 2] == tile.CORRIDOR))
                                {
                                    grid[x, y - 1] = tile.CORRIDOR;
                                    connections++;
                                }
                                else if ((grid[x + 1, y - 1] == tile.WALL) && (grid[x + 2, y - 2] == tile.CORRIDOR))
                                {
                                    grid[x + 1, y - 1] = tile.CORRIDOR;
                                    connections++;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Find spaces in the maze and fills them in
        /// </summary>
        private static void cleanUpMaze()
        {
            for (int x = 1; x < columns - 1; x++)
            {
                for (int y = 1; y < rows - 1; y++)
                {
                    if ((grid[x + 1, y] == tile.WALL)
                        && (grid[x + 1, y + 1] == tile.WALL)
                        && (grid[x, y + 1] == tile.WALL)
                        && (grid[x - 1, y + 1] == tile.WALL)
                        && (grid[x - 1, y] == tile.WALL)
                        && (grid[x - 1, y - 1] == tile.WALL)
                        && (grid[x, y - 1] == tile.WALL)
                        && (grid[x + 1, y - 1] == tile.WALL))
                    {
                        carveWall(new Vector2(x, y));
                    }
                }
            }
        }

        /// <summary>
        /// Carve recursively
        /// </summary>
        /// <param name="currentCell"></param>
        private static void carveWall(Vector2 currentCell)
        {

            //list of available directions
            List<directions> neighbours = new List<directions>() { directions.NORTH, directions.EAST, directions.SOUTH, directions.WEST };

            int n = neighbours.Count;

            //randomise the list order
            while (n > 1)
            {
                n--;
                int k = rnd.Next(n + 1);
                directions temp = neighbours[k];
                neighbours[k] = neighbours[n];
                neighbours[n] = temp;
            }

            Vector2 nextCell = currentCell;

            //go through each direction
            while (neighbours.Count > 0)
            {
                directions nextDir = neighbours[0];
                neighbours.RemoveAt(0);
                nextCell = currentCell;

                switch (nextDir)
                {
                    case (directions.NORTH):
                        {
                            nextCell.Y += 2;
                            break;
                        }
                    case (directions.EAST):
                        {
                            nextCell.X += 2;
                            break;
                        }
                    case (directions.SOUTH):
                        {
                            nextCell.Y -= 2;
                            break;
                        }
                    case (directions.WEST):
                        {
                            nextCell.X -= 2;
                            break;
                        }
                    default:
                        {
                            return;
                        }
                }

                //if the next cell is a valid one
                if ((nextCell.X > 0) && (nextCell.X < columns - 1) && (nextCell.Y > 0) && (nextCell.Y < rows - 1))
                {
                    //if the next cell is unvisited and isnt a room
                    if ((visited[(int)nextCell.X, (int)nextCell.Y] == false) && (grid[(int)nextCell.X, (int)nextCell.Y] != tile.ROOM))
                    {
                        //carve and recurse
                        grid[(int)nextCell.X, (int)nextCell.Y] = tile.CORRIDOR;
                        visited[(int)nextCell.X, (int)nextCell.Y] = true;
                        Vector2 wallToBeBroken = currentCell + ((nextCell - currentCell) / 2);
                        grid[(int)wallToBeBroken.X, (int)wallToBeBroken.Y] = tile.CORRIDOR;
                        visited[(int)wallToBeBroken.X, (int)wallToBeBroken.Y] = true;
                        carveWall(nextCell);
                    }
                }
            }
        }

        /// <summary>
        /// Carves dead ends to make the maze have loops
        /// </summary>
        private static void loopDeadEnds()
        {
            //list of available directions
            List<directions> neighbours = new List<directions>();

            //go through each cell in the maze, and inspect it if it is a corridor
            for (int x = 0; x < columns - 1; x++)
            {
                for (int y = 0; y < rows - 1; y++)
                {
                    if (grid[x, y] == tile.CORRIDOR)
                    {
                        //Clear list of neighbours andfill it with neighbours that are walls
                        neighbours.Clear();
                        int wallNeighbours = 0;
                        {
                            if (grid[x + 1, y] == tile.WALL)
                            {
                                wallNeighbours += 1;
                                neighbours.Add(directions.EAST);
                            }
                            if (grid[x, y + 1] == tile.WALL)
                            {
                                wallNeighbours += 1;
                                neighbours.Add(directions.NORTH);
                            }
                            if (grid[x - 1, y] == tile.WALL)
                            {
                                wallNeighbours += 1;
                                neighbours.Add(directions.WEST);
                            }
                            if (grid[x, y - 1] == tile.WALL)
                            {
                                wallNeighbours += 1;
                                neighbours.Add(directions.SOUTH);
                            }

                            //if there are 3 walls as neighbours then this is a dead end
                            if (wallNeighbours == 3)
                            {

                                int n = neighbours.Count;

                                //randomise the neighbour list order
                                while (n > 1)
                                {
                                    n--;
                                    int k = rnd.Next(n + 1);
                                    directions temp = neighbours[k];
                                    neighbours[k] = neighbours[n];
                                    neighbours[n] = temp;
                                }

                                Vector2 currentCell = new Vector2(x, y);
                                Vector2 nextCell = currentCell;

                                //while nothing has been carved yet - loop
                                bool carved = false;
                                while (!carved)
                                {
                                    //take the first direction of the list and then remove it
                                    directions nextDir = neighbours[0];
                                    neighbours.RemoveAt(0);
                                    nextCell = currentCell;

                                    //find the cell in that direction
                                    switch (nextDir)
                                    {
                                        case (directions.NORTH):
                                            {
                                                nextCell.Y += 2;
                                                break;
                                            }
                                        case (directions.EAST):
                                            {
                                                nextCell.X += 2;
                                                break;
                                            }
                                        case (directions.SOUTH):
                                            {
                                                nextCell.Y -= 2;
                                                break;
                                            }
                                        case (directions.WEST):
                                            {
                                                nextCell.X -= 2;
                                                break;
                                            }
                                        default:
                                            {
                                                return;
                                            }
                                    }
                                    //if the cell is a valid cell in the grid and is a corridor
                                    if ((nextCell.X > 0) && (nextCell.X < columns - 1) && (nextCell.Y > 0) && (nextCell.Y < rows - 1))
                                    {
                                        if (grid[(int)nextCell.X, (int)nextCell.Y] == tile.CORRIDOR)
                                        {
                                            //break the cell through to the corridor to remove the dead end
                                            Vector2 wallToBeBroken = currentCell + ((nextCell - currentCell) / 2);
                                            grid[(int)wallToBeBroken.X, (int)wallToBeBroken.Y] = tile.CORRIDOR;
                                            carved = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
