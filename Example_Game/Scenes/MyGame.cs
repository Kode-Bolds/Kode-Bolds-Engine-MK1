using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using Game_Engine.Components;
using Game_Engine.Systems;
using Game_Engine.Managers;
using Game_Engine.Objects;
using System.Diagnostics;
using System.Drawing;

namespace OpenGL_Game.Scenes
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class MyGame : Scene, IScene
    {
        List<string> keyboardInput;
        List<string> mouseInput;
        Vector2 mousePos;
        Vector2 oldMousePos;
        Vector2 mouseDelta;
        float mouseSensitivity;
        public static float dt;
        public static float time;
        EntityManager entityManager;
        SystemManager systemManager;
        InputManager inputManager;
        Camera mainCamera;
        Camera minimapCamera;
        List<Camera> cameraList;
        Node[,] map;
        Maze.tile[,] maze;
        int bulletCount;
        float fireRate;
        float nextShot;
        bool noClip;
        Vector4 lightPosition;
        Random rnd = new Random();
        float noClipToggleTimer;
        float disableDroneToggleTimer;
        float disableCollisionToggleTimer;
        bool dronesDestroyed;
        List<Entity> despawnedEntities;
        bool damageBuff;
        bool speedBuff;
        bool disableDroneBuff;
        bool disableDrone;
        float damageBuffTimer;
        float speedBuffTimer;
        float disableDroneTimer;
        bool disableCollisions;
        bool paused;
        float pauseToggleTimer;
        float damageDelay;
        int health;
        int healthMax;
        int droneCount;

        List<string> playerIgnoreCollisionWith;
        List<string> bulletIgnoreCollisionWith;
        List<string> droneIgnoreCollisionWith;
        List<string> powerUpIgnoreCollisionWith;

        Entity player;

        public MyGame(SceneManager sceneManager) : base(sceneManager)
        {
            //Set window title
            sceneManager.Title = "DOOMED";
            //Set the render and update delegates
            sceneManager.renderer = Render;
            sceneManager.updater = Update;

            //Creates managers
            entityManager = new EntityManager();
            systemManager = new SystemManager();
            inputManager = new InputManager(sceneManager);

            cameraList = new List<Camera>();
            //Creates new camera object for the main game camera
            mainCamera = new Camera(new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 1, 0), 60, 1920 / 1080f, 0.01f, 100f);
            cameraList.Add(mainCamera);
            //Creates a new camera for the minimap
            minimapCamera = new Camera(new Vector3(12f, 20, 12f), new Vector3(12f, -1, 12f), new Vector3(0, 0, -1), 25f, 25f, 0.01f, 100f);
            cameraList.Add(minimapCamera);

            playerIgnoreCollisionWith = new List<string>();
            bulletIgnoreCollisionWith = new List<string>();
            droneIgnoreCollisionWith = new List<string>();
            powerUpIgnoreCollisionWith = new List<string>();
            despawnedEntities = new List<Entity>();



            //Sets light position
            lightPosition = new Vector4(0f, 8f, 0f, 1);

            //Creates initial Entities
            CreateEntities();


            //Sets reference to player entity
            player = entityManager.FindEntity("Player");

            //Creates Systems
            CreateSystems();

            //Gets reference from input manager of the list of buttons pressed
            keyboardInput = inputManager.Keyboard();
            mouseInput = inputManager.MouseInput();

            //Sets cursor visibility state
            inputManager.CursorVisible(false);

            //Sets mouse sensitivity
            mouseSensitivity = 0.001f;

            //Sets camera starting position
            mainCamera.Position = player.GetTransform().Translation;

            //Sets base values for shooting variables
            bulletCount = 0;
            fireRate = 0.3f;
            nextShot = 0;

            //Sets no clip to false on load
            noClip = false;

            //Sets buffs to false on load
            damageBuff = false;
            speedBuff = false;
            disableDroneBuff = false;

            //Sets collision and drone disabling to false on load
            disableCollisions = false;
            disableDrone = false;

            //Sets paused to false on load
            paused = false;

            //Sets health
            health = player.GetHealth().Health;
            healthMax = player.GetHealth().Health;

            //Sets drone count
            foreach(Entity entity in entityManager.Entities())
            {
                if(entity.Name.Contains("Drone"))
                {
                    droneCount++;
                }
            }
        }

        private void CreateEntities()
        {
            Entity newEntity;

            //Sets ignored objects for player collisions
            playerIgnoreCollisionWith.Add("Bullet");

            //Sets ignored objects for bullet collisions
            bulletIgnoreCollisionWith.Add("Player");
            bulletIgnoreCollisionWith.Add("Health");
            bulletIgnoreCollisionWith.Add("Speed");
            bulletIgnoreCollisionWith.Add("Damage");
            bulletIgnoreCollisionWith.Add("Disable");

            //Sets ignored objects for drone collisions
            droneIgnoreCollisionWith.Add("Drone");
            droneIgnoreCollisionWith.Add("Health");
            droneIgnoreCollisionWith.Add("Speed");
            droneIgnoreCollisionWith.Add("Damage");
            droneIgnoreCollisionWith.Add("Disable");

            //Sets ignored objects for power up collisions
            powerUpIgnoreCollisionWith.Add("Drone");
            powerUpIgnoreCollisionWith.Add("Bullet");

            #region Maze/map node generation and drone placement

            ////sky box cube mapping
            newEntity = new Entity("SkyBox");
            newEntity.AddComponent(new ComponentTransform(new Vector3(0f, 0f, 0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(10f, 10f, 10f)));
            newEntity.AddComponent(new ComponentGeometry("Geometry/cube.obj"));
            List<string> textures = new List<string>();
            textures.Add("Textures/RIGHT.png");
            textures.Add("Textures/LEFT.png");
            textures.Add("Textures/TOP.png");
            textures.Add("Textures/BOTTOM.png");
            textures.Add("Textures/BACK.png");
            textures.Add("Textures/FRONT.png");
            newEntity.AddComponent(new ComponentTexture(textures));
            newEntity.AddComponent(new ComponentShader("Shaders/vCubeMapping.glsl", "Shaders/fCubeMapping.glsl"));
            entityManager.AddEntity(newEntity);

            //floor plane
            newEntity = new Entity("Floor");
            newEntity.AddComponent(new ComponentTransform(new Vector3(12f, -0.5f, 12f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(12.5f, 0.01f, 12.5f)));
            newEntity.AddComponent(new ComponentGeometry("Geometry/cube.obj"));
            newEntity.AddComponent(new ComponentTexture("Textures/Black.png"));
            newEntity.AddComponent(new ComponentShader("Shaders/vLighting.glsl", "Shaders/fLighting.glsl"));
            entityManager.AddEntity(newEntity);

            //Generates maze
            if(maze == null)
            {
                maze = Maze.mazeGenerator();
            }

            //Sets map array size to same size as maze array
            map = new Node[Maze.rows, Maze.columns];

            int wallCount = 0;
            int droneCount = 0;
            int healthPowerUpCount = 0;
            int speedPowerUpCount = 0;
            int damagePowerUpCount = 0;
            int disableDronePowerUpCount = 0;

            for (int i = 0; i < Maze.rows; i++)
            {
                for (int j = 0; j < Maze.columns; j++)
                {
                    //Creates Nodes for each location in the maze and feeds the location values into each node
                    map[i, j] = new Node();
                    map[i, j].Location = new Vector3(i, -0.25f, j);
                    map[i, j].mapX = i;
                    map[i, j].mapY = j;

                    //Creates wall entities for the walls of the maze
                    if (maze[i, j] == Maze.tile.WALL)
                    {
                        newEntity = new Entity("Wall(" + wallCount + ")");
                        newEntity.AddComponent(new ComponentTransform(new Vector3(i, 0.75f, j), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.5f, 1.25f, 0.5f)));
                        newEntity.AddComponent(new ComponentGeometry("Geometry/cube.obj"));
                        newEntity.AddComponent(new ComponentTexture("Textures/Metal.png"));
                        newEntity.AddComponent(new ComponentShader("Shaders/vLighting.glsl", "Shaders/fLighting.glsl"));
                        newEntity.AddComponent(new ComponentBoxCollider(new Vector2(i - 0.5f, j - 0.5f), new Vector2(i + 0.5f, j - 0.5f), new Vector2(i + 0.5f, j + 0.5f), new Vector2(i - 0.5f, j + 0.5f), new List<string>()));
                        entityManager.AddEntity(newEntity);

                        //Sets passable value of Node to false
                        map[i, j].Passable = false;

                        wallCount++;
                    }
                    else
                    {
                        //Sets passable value of Node to true
                        map[i, j].Passable = true;

                        if (maze[i, j] == Maze.tile.CORRIDOR)
                        {
                            //1 in 15 chance of spawning drone at node
                            int droneRandomSpawnChance = rnd.Next(1, 15);
                            if (droneRandomSpawnChance == 1)
                            {
                                //Creates drone
                                newEntity = new Entity("Drone(" + droneCount + ")");
                                newEntity.AddComponent(new ComponentTransform(map[i, j].Location, new Vector3(0.0f, 1.5708f, 0.0f), new Vector3(0.2f, 0.2f, 0.2f)));
                                newEntity.AddComponent(new ComponentGeometry("Geometry/cone.obj"));
                                newEntity.AddComponent(new ComponentTexture("Textures/Red.png"));
                                //newEntity.AddComponent(new ComponentAudio("Audio/ENEMY DEATH SOUND.wav", false, false));
                                newEntity.AddComponent(new ComponentVelocity(1.5f));
                                newEntity.AddComponent(new ComponentShader("Shaders/vLighting.glsl", "Shaders/fLighting.glsl"));
                                newEntity.AddComponent(new ComponentAI(new Vector2(map[i, j].mapX, map[i, j].mapY), map[i, j].Location, 90));
                                newEntity.AddComponent(new ComponentSphereCollider((0.075f), droneIgnoreCollisionWith));
                                newEntity.AddComponent(new ComponentHealth(4));
                                entityManager.AddEntity(newEntity);

                                droneCount++;
                            }

                            //1 in 20 chance of spawning healthPowerUp at node
                            int healthRandomSpawnChance = rnd.Next(1, 50);
                            if (healthRandomSpawnChance == 1)
                            {
                                //Creates powerup
                                newEntity = new Entity("Health(" + healthPowerUpCount + ")");
                                newEntity.AddComponent(new ComponentTransform(map[i, j].Location, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.1f, 0.1f, 0.1f)));
                                newEntity.AddComponent(new ComponentGeometry("Geometry/cylinder.obj"));
                                newEntity.AddComponent(new ComponentTexture("Textures/Green.png"));
                                newEntity.AddComponent(new ComponentAudio("Audio/buzz.wav", true, true));
                                newEntity.AddComponent(new ComponentShader("Shaders/vLighting.glsl", "Shaders/fLighting.glsl"));
                                newEntity.AddComponent(new ComponentSphereCollider((0.075f), powerUpIgnoreCollisionWith));
                                entityManager.AddEntity(newEntity);

                                healthPowerUpCount++;
                            }

                            //1 in 20 chance of spawning speedPowerUp at node
                            int speedRandomSpawnChance = rnd.Next(1, 40);
                            if (speedRandomSpawnChance == 1)
                            {
                                //Creates powerup
                                newEntity = new Entity("Speed(" + speedPowerUpCount + ")");
                                newEntity.AddComponent(new ComponentTransform(map[i, j].Location, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.1f, 0.1f, 0.1f)));
                                newEntity.AddComponent(new ComponentGeometry("Geometry/cylinder.obj"));
                                newEntity.AddComponent(new ComponentTexture("Textures/Blue.png"));
                                //newEntity.AddComponent(new ComponentAudio("Audio/buzz.wav", true, true));
                                newEntity.AddComponent(new ComponentShader("Shaders/vLighting.glsl", "Shaders/fLighting.glsl"));
                                newEntity.AddComponent(new ComponentSphereCollider((0.075f), powerUpIgnoreCollisionWith));
                                entityManager.AddEntity(newEntity);

                                speedPowerUpCount++;
                            }

                            //1 in 20 chance of spawning damagePowerUp at node
                            int damageRandomSpawnChance = rnd.Next(1, 40);
                            if (damageRandomSpawnChance == 1)
                            {
                                //Creates powerup
                                newEntity = new Entity("Damage(" + damagePowerUpCount + ")");
                                newEntity.AddComponent(new ComponentTransform(map[i, j].Location, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.1f, 0.1f, 0.1f)));
                                newEntity.AddComponent(new ComponentGeometry("Geometry/cylinder.obj"));
                                newEntity.AddComponent(new ComponentTexture("Textures/Red.png"));
                                //newEntity.AddComponent(new ComponentAudio("Audio/buzz.wav", true, true));
                                newEntity.AddComponent(new ComponentShader("Shaders/vLighting.glsl", "Shaders/fLighting.glsl"));
                                newEntity.AddComponent(new ComponentSphereCollider((0.075f), powerUpIgnoreCollisionWith));
                                entityManager.AddEntity(newEntity);

                                damagePowerUpCount++;
                            }

                            //1 in 50 chance of spawning disableDronePowerUp at node
                            int disableRandomSpawnChance = rnd.Next(1, 50);
                            if (disableRandomSpawnChance == 1)
                            {
                                //Creates powerup
                                newEntity = new Entity("Disable(" + disableDronePowerUpCount + ")");
                                newEntity.AddComponent(new ComponentTransform(map[i, j].Location, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.1f, 0.1f, 0.1f)));
                                newEntity.AddComponent(new ComponentGeometry("Geometry/cylinder.obj"));
                                newEntity.AddComponent(new ComponentTexture("Textures/Yellow.png"));
                               //newEntity.AddComponent(new ComponentAudio("Audio/buzz.wav", true, true));
                                newEntity.AddComponent(new ComponentShader("Shaders/vLighting.glsl", "Shaders/fLighting.glsl"));
                                newEntity.AddComponent(new ComponentSphereCollider((0.075f), powerUpIgnoreCollisionWith));
                                entityManager.AddEntity(newEntity);

                                disableDronePowerUpCount++;
                            }
                        }
                    }
                }
            }
            #endregion

            //Creates player entity at beginning of maze
            newEntity = new Entity("Player");
            newEntity.AddComponent(new ComponentTransform(new Vector3(12.5f, 0, 12.5f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.5f, 0.5f, 0.5f)));
            newEntity.AddComponent(new ComponentGeometry("Geometry/cone.obj"));
            newEntity.AddComponent(new ComponentTexture("Textures/Green.png"));
            //newEntity.AddComponent(new ComponentAudio("Audio/IMPACT SOUND.wav", false, false));
            newEntity.AddComponent(new ComponentVelocity(2f));
            newEntity.AddComponent(new ComponentShader("Shaders/vLighting.glsl", "Shaders/fLighting.glsl"));
            newEntity.AddComponent(new ComponentSphereCollider((0.2f), playerIgnoreCollisionWith));
            newEntity.AddComponent(new ComponentHealth(3));
            entityManager.AddEntity(newEntity);



            //Create a house
            newEntity = new Entity("Char");
            newEntity.AddComponent(new ComponentTransform(new Vector3(-70f, -40, 30f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f)));
            newEntity.AddComponent(new ComponentGeometry("Geometry/Triangulated_Char.obj"));
            newEntity.AddComponent(new ComponentShader("Shaders/vLighting.glsl", "Shaders/fLighting.glsl"));
            newEntity.AddComponent(new ComponentTexture("Textures/Skin.png"));
            entityManager.AddEntity(newEntity);

            //Create a house
            newEntity = new Entity("Sniper");
            newEntity.AddComponent(new ComponentTransform(new Vector3(25f, 10, 25f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f)));
            newEntity.AddComponent(new ComponentGeometry("Geometry/Triangulated_Sniper.obj"));
            newEntity.AddComponent(new ComponentShader("Shaders/vLighting.glsl", "Shaders/fLighting.glsl"));
            newEntity.AddComponent(new ComponentTexture("Textures/Green.png"));
            entityManager.AddEntity(newEntity);
        }

        private void CreateSystems()
        {
            ISystem newSystem;

            //Render Systems
            newSystem = new SystemRender(cameraList, lightPosition, sceneManager.ClientRectangle);
            systemManager.AddRenderSystem(newSystem);

            //Update Systems
            newSystem = new SystemAudio(mainCamera);
            systemManager.AddUpdateSystem(newSystem);
            newSystem = new SystemAI(map, Maze.rows, Maze.columns, player);
            systemManager.AddUpdateSystem(newSystem);
            newSystem = new SystemCollisions();
            systemManager.AddUpdateSystem(newSystem);
            newSystem = new SystemPhysics(sceneManager);
            systemManager.AddUpdateSystem(newSystem);

            //Assigns entities to appropriate systems
            systemManager.AssignEntities(entityManager);
        }

        public void Render(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //Calls Render Systems every frame
            systemManager.RenderSystems();

            GL.Viewport(0, sceneManager.Height - (sceneManager.Height / 5), sceneManager.Width / 5, sceneManager.Height / 5);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, sceneManager.Width, 0, sceneManager.Height, -1, 1);

            float width = sceneManager.Width, height = sceneManager.Height, fontSize = Math.Min(width, height) / 10f;
            GUI.DrawText(new Rectangle(0, (int)(fontSize / 2f), (int)width, (int)(fontSize * 2f)), "Health: " + health + "/" + healthMax, (int)(fontSize * 1.5f), StringAlignment.Center, Color.White);
            GUI.DrawText(new Rectangle(0, (int)(fontSize / 2f + 200), (int)width, (int)(fontSize * 2f)), "Drones Remaining: " + droneCount, (int)(fontSize * 1.5f), StringAlignment.Center, Color.White);
            GUI.DrawText(new Rectangle(0, (int)(fontSize / 2f + 400), (int)width, (int)(fontSize * 2f)), "Power Ups:", (int)(fontSize * 1.5f), StringAlignment.Center, Color.White);
            if(speedBuff)
            {
                GUI.DrawText(new Rectangle(-650, (int)(fontSize / 2f + 600), (int)width, (int)(fontSize * 2f)), "Speed", (int)(fontSize * 1.5f), StringAlignment.Center, Color.Blue);
            }
            if(damageBuff)
            {
                GUI.DrawText(new Rectangle(0, (int)(fontSize / 2f + 600), (int)width, (int)(fontSize * 2f)), "Damage", (int)(fontSize * 1.5f), StringAlignment.Center, Color.Red);
            }
            if(disableDroneBuff)
            {
                GUI.DrawText(new Rectangle(650, (int)(fontSize / 2f + 600), (int)width, (int)(fontSize * 2f)), "Drone", (int)(fontSize * 1.5f), StringAlignment.Center, Color.Yellow);
            }
            if(paused)
            {
                GUI.DrawText(new Rectangle(0, (int)(fontSize / 2f + 800), (int)width, (int)(fontSize * 2f)), "Paused", (int)(fontSize * 1.5f), StringAlignment.Center, Color.White);
            }
            GUI.Render(Color.Transparent);
        }

        public void Update(FrameEventArgs e)
        {
            //Game time and delta time
            dt = (float)e.Time;
            sceneManager.dt = dt;
            time += (float)e.Time;
            SceneManager.time = time;

            //Pause button toggle timer
            pauseToggleTimer += dt;

            //Pauses game when escape is pressed and unpauses when pressed again
            if (keyboardInput.Contains("Escape") && pauseToggleTimer > 0.25f)
            {
                if (paused != true)
                {
                    paused = true;
                    pauseToggleTimer = 0;
                }
                else
                {
                    paused = false;
                    pauseToggleTimer = 0;
                }
            }

            //Checks game isn't paused
            if(paused != true)
            {
                //Button toggle on/off timers
                noClipToggleTimer += dt;
                disableCollisionToggleTimer += dt;
                disableDroneToggleTimer += dt;

                //Buff timers
                damageBuffTimer += dt;
                speedBuffTimer += dt;
                disableDroneTimer += dt;

                //Damage delay timers
                damageDelay += dt;

                //Gets mouse position from input manager every frame
                mousePos = inputManager.MousePosition();

                #region Player Movement and collision with drone logic
                if (keyboardInput.Contains("Up"))
                {
                    //Moves player forward when W is pressed
                    player.GetTransform().Translation += player.GetTransform().Forward * player.GetVelocity().Velocity * dt;
                    //Moves camera forward to stay in line with player position
                    mainCamera.MoveCamera(player.GetTransform().Translation);
                }
                if (keyboardInput.Contains("Down"))
                {
                    //Moves player backwards when S is pressed
                    player.GetTransform().Translation -= player.GetTransform().Forward * player.GetVelocity().Velocity * dt;
                    //Moves camera backwards to stay in line with player position
                    mainCamera.MoveCamera(player.GetTransform().Translation);
                }
                if (keyboardInput.Contains("Right"))
                {
                    //Moves player right when D is pressed
                    player.GetTransform().Translation += player.GetTransform().Right * player.GetVelocity().Velocity * dt;
                    //Moves camera right to stay in line with player position
                    mainCamera.MoveCamera(player.GetTransform().Translation);
                }
                if (keyboardInput.Contains("Left"))
                {
                    //Moves player left when A is pressed
                    player.GetTransform().Translation -= player.GetTransform().Right * player.GetVelocity().Velocity * dt;
                    //Moves camera left to stay in line with player position
                    mainCamera.MoveCamera(player.GetTransform().Translation);

                }

                if (player.GetCollidedWith().Count > 0)
                {
                    foreach (string entityName in player.GetCollidedWith())
                    {
                        //Removes a life from the player when colliding with a drone
                        if (entityName.Contains("Drone") && damageDelay > 0.5f)
                        {
                            player.GetHealth().Health -= 1;
                            health -= 1;
                            damageDelay = 0;
                        }

                        if(entityName.Contains("Health") || entityName.Contains("Disable") || entityName.Contains("Speed") || entityName.Contains("Damage"))
                        {
                        //    player.GetAudio().PlayOnAwake = true;
                        }
                    }
                }

                //Checks if drone collides with player
                foreach (Entity entity in entityManager.Entities())
                {
                    if (entity.Name.Contains("Drone"))
                    {
                        if (entity.GetCollidedWith().Count > 0)
                        {
                            foreach (string entityName in entity.GetCollidedWith())
                            {
                                if (entityName.Contains("Player"))
                                {
                               //     entity.GetAudio().PlayOnAwake = true;
                                }
                            }
                        }
                    }
                }
                    //Game over screen when player runs out of lives
                    if (player.GetHealth().Health <= 0)
                {
                    //Game over logic
                    sceneManager.LoadScene(new GameOverScene(sceneManager));
                }
                #endregion

                #region Player Rotation on the Y axis based on mouse movement on the X axis
                if (sceneManager.Focused)
                {
                    //Amount mouse has moved since last update call
                    mouseDelta = oldMousePos - mousePos;

                    //Adjusts player rotation on the x and y axis based on the mouse movement on the x and y axis if no clip is on
                    if (noClip == true)
                    {
                        player.GetTransform().Rotation += new Vector3(mouseDelta.Y, mouseDelta.X, 0.0f) * mouseSensitivity;
                    }
                    //Adjusts player rotation on the y axis based on mouse movement on the x axis if no clip is off
                    else
                    {
                        player.GetTransform().Rotation += new Vector3(0.0f, mouseDelta.X, 0.0f) * mouseSensitivity;
                    }

                    //clamp x rotation
                    if (player.GetTransform().Rotation.X > MathHelper.DegreesToRadians(89))
                    {
                        player.GetTransform().Rotation = new Vector3(MathHelper.DegreesToRadians(89), player.GetTransform().Rotation.Y, 0.0f);
                    }
                    if (player.GetTransform().Rotation.X < MathHelper.DegreesToRadians(-89))
                    {
                        player.GetTransform().Rotation = new Vector3(MathHelper.DegreesToRadians(-89), player.GetTransform().Rotation.Y, 0.0f);
                    }

                    //Adjusts camera rotation to stay in line with player rotation
                    mainCamera.RotateCamera(player.GetTransform().Rotation);

                    //Centers cursor to center of screen
                    inputManager.CenterCursor();

                    //Resets mouse position
                    oldMousePos = mousePos;
                }
                #endregion

                #region Shooting logic on left click
                //Creates new bullet entity on click based on fire rate
                if (mouseInput.Contains("Left") && time > nextShot)
                {
                    Entity bullet = new Entity("Bullet(" + bulletCount + ")");
                    bullet.AddComponent(new ComponentTransform(player.GetTransform().Translation - new Vector3(0, 0.25f, 0), player.GetTransform().Rotation, new Vector3(0.05f, 0.05f, 0.05f)));
                    bullet.AddComponent(new ComponentGeometry("Geometry/sphere.obj"));
                    bullet.AddComponent(new ComponentTexture("Textures/Blue.png"));
                    bullet.AddComponent(new ComponentVelocity(10f));
                    bullet.AddComponent(new ComponentShader("Shaders/vPulsating.glsl", "Shaders/fPulsating.glsl"));
                    bullet.AddComponent(new ComponentAudio("Audio/GUNSHOT.wav", false, true));
                    bullet.AddComponent(new ComponentSphereCollider((0.05f), bulletIgnoreCollisionWith));
                    entityManager.AddEntity(bullet);

                    //Assigns new entity to appropriate system(s)
                    systemManager.AssignNewEntity(bullet);

                    //Iterates bullet count
                    bulletCount++;

                    //Sets timestamp for when next shot can be fired
                    nextShot = time + fireRate;
                }

                //Bullet movement and collisions
                foreach (Entity entity in entityManager.Entities())
                {
                    //Moves bullet and destroys on collision
                    if (entity.Name.Contains("Bullet"))
                    {
                        entity.GetTransform().Translation += entity.GetTransform().Forward * entity.GetVelocity().Velocity * dt;

                        //Destroys bullet if it has collided with something
                        if (entity.GetCollidedWith().Count > 0)
                        {
                            //Removes entity from systems to remove it from game
                            systemManager.DestroyEntity(entity);

                            //Adds entity to despawn list to remove final reference of entity from entity manager to clear it from memory
                            despawnedEntities.Add(entity);
                        }
                    }

                    //Bullet collision with drones
                    if (entity.Name.Contains("Drone"))
                    {
                        if (entity.GetCollidedWith().Count > 0)
                        {
                            foreach (string entityName in entity.GetCollidedWith())
                            {
                                //Removes life from drone when bullet hits
                                if (entityName.Contains("Bullet"))
                                {
                                    if (damageBuff == true)
                                    {
                                        entity.GetHealth().Health -= 2;
                                    }
                                    else
                                    {
                                        entity.GetHealth().Health -= 1;
                                    }
                                }
                            }
                        }
                        //Destroys drone when it reaches 0 lives
                        if (entity.GetHealth().Health <= 0)
                        {
                            //Removes entity from systems to remove it from game
                            systemManager.DestroyEntity(entity);
                            droneCount--;

                            //Adds entity to despawn list to remove final reference of entity from entity manager to clear it from memory
                            despawnedEntities.Add(entity);
                        }
                    }
                }
                #endregion

                #region No Clip and debug keys logic
                //Activates no clip, allows player to move and rotate freely in 3d space, multiplies speed by 4
                if (keyboardInput.Contains("N") && noClipToggleTimer > 0.25f)
                {
                    if (noClip == false)
                    {
                        noClip = true;
                        disableCollisions = true;
                        noClipToggleTimer = 0;
                        player.GetVelocity().Velocity *= 4;
                    }
                    else
                    {
                        noClip = false;
                        disableCollisions = false;
                        noClipToggleTimer = 0;
                        player.GetVelocity().Velocity /= 4;
                    }
                }

                //Moves player up when noclip is true
                if (keyboardInput.Contains("Space") && noClip == true)
                {
                    player.GetTransform().Translation += new Vector3(0, 1, 0) * player.GetVelocity().Velocity * dt;
                    mainCamera.MoveCamera(player.GetTransform().Translation);
                }

                //Disables/Enables drones when D is pressed
                if (keyboardInput.Contains("D") && disableDroneToggleTimer > 0.25f)
                {
                    if (disableDrone == false)
                    {
                        disableDrone = true;
                        disableDroneToggleTimer = 0;
                    }
                    else
                    {
                        disableDrone = false;
                        disableDroneToggleTimer = 0;
                    }
                }

                //Disables/Enables collisions when C is pressed
                if (keyboardInput.Contains("C") && disableCollisionToggleTimer > 0.25f)
                {
                    if (disableCollisions == false)
                    {
                        disableCollisions = true;
                        disableCollisionToggleTimer = 0;
                    }
                    else
                    {
                        disableCollisions = false;
                        disableCollisionToggleTimer = 0;
                    }
                }
                #endregion

                #region Power Up logic
                foreach (Entity entity in entityManager.Entities())
                {
                    if (entity.Name.Contains("Health"))
                    {
                        if (entity.GetCollidedWith().Contains("Player"))
                        {
                            //If player 
                            if (player.GetHealth().Health < 3)
                            {
                                player.GetHealth().Health += 1;
                                health += 1;
                            }

                            //Removes entity from systems to remove it from game
                            systemManager.DestroyEntity(entity);

                            //Adds entity to despawn list to remove final reference of entity from entity manager to clear it from memory
                            despawnedEntities.Add(entity);
                        }
                    }

                    if (entity.Name.Contains("Damage"))
                    {
                        if (entity.GetCollidedWith().Contains("Player"))
                        {
                            //Sets damage buff to true and sets timer for damage buff
                            if (damageBuff == false)
                            {
                                damageBuff = true;
                                damageBuffTimer = 0;
                            }

                            //Removes entity from systems to remove it from game
                            systemManager.DestroyEntity(entity);

                            //Adds entity to despawn list to remove final reference of entity from entity manager to clear it from memory
                            despawnedEntities.Add(entity);
                        }
                    }

                    if (entity.Name.Contains("Speed"))
                    {
                        if (entity.GetCollidedWith().Contains("Player"))
                        {
                            //Sets speed buff to true and sets timer for speed buff
                            if (speedBuff == false)
                            {
                                speedBuff = true;
                                speedBuffTimer = 0;
                                player.GetVelocity().Velocity *= 2;
                            }

                            //Removes entity from systems to remove it from game
                            systemManager.DestroyEntity(entity);

                            //Adds entity to despawn list to remove final reference of entity from entity manager to clear it from memory
                            despawnedEntities.Add(entity);
                        }
                    }

                    if (entity.Name.Contains("Disable"))
                    {
                        if (entity.GetCollidedWith().Contains("Player"))
                        {
                            //Sets disable drones to true and sets timer for disabled drones
                            if (disableDroneBuff == false)
                            {
                                disableDroneBuff = true;
                                disableDroneTimer = 0;
                            }

                            //Removes entity from systems to remove it from game
                            systemManager.DestroyEntity(entity);

                            //Adds entity to despawn list to remove final reference of entity from entity manager to clear it from memory
                            despawnedEntities.Add(entity);
                        }
                    }

                    //Disables buffs when timers reach their end
                    if (damageBuff == true && damageBuffTimer > 3)
                    {
                        damageBuff = false;
                    }

                    if (speedBuff == true && speedBuffTimer > 3)
                    {
                        speedBuff = false;
                        player.GetVelocity().Velocity /= 2;
                    }

                    if (disableDroneBuff == true && disableDroneTimer > 3)
                    {
                        disableDroneBuff = false;
                    }
                }
                #endregion

                #region Victory condition check
                //Checks if all drones are destroyed
                dronesDestroyed = true;
                foreach (Entity entity in entityManager.Entities())
                {
                    if (entity.Name.Contains("Drone"))
                    {
                        dronesDestroyed = false;
                    }
                }

                //Exits game when all drones destroyed, planned to go to victory screen instead of exit.
                if (dronesDestroyed || keyboardInput.Contains("V"))
                {
                    sceneManager.LoadScene(new VictoryScene(sceneManager));
                }
                #endregion

                //Enables/Disables drone and collisions based on appropriate bools
                foreach (Entity entity in entityManager.Entities())
                {
                    //Collisions
                    if (entity.GetComponentBoxCollider() != null)
                    {
                        if (entity.GetComponentBoxCollider().Disabled != disableCollisions)
                        {
                            entity.GetComponentBoxCollider().Disabled = disableCollisions;
                        }
                    }
                    else if (entity.GetComponentSphereCollider() != null)
                    {
                        if (entity.GetComponentSphereCollider().Disabled != disableCollisions)
                        {
                            entity.GetComponentSphereCollider().Disabled = disableCollisions;
                        }
                    }

                    //Drones
                    if (entity.GetComponentAI() != null)
                    {
                        if (entity.GetComponentAI().Disabled != disableDrone)
                        {
                            entity.GetComponentAI().Disabled = disableDrone;
                        }

                        if (entity.GetComponentAI().Disabled != disableDroneBuff && disableDrone != true)
                        {
                            entity.GetComponentAI().Disabled = disableDroneBuff;
                        }
                    }
                }

                //Removes all references to destroyed entities to remove them from memory
                foreach (Entity entity in despawnedEntities)
                {
                    entityManager.Entities().Remove(entity);
                }
                despawnedEntities.Clear();

                //Calls Update Systems every frame
                systemManager.UpdateSystems();
            }
        }
    }
}
