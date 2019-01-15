using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Game_Engine.Components;
using Game_Engine.Objects;
using System.Drawing;

namespace Game_Engine.Systems
{
    public class SystemRender : ISystem
    {

        const ComponentTypes MASK = (ComponentTypes.COMPONENT_TRANSFORM | ComponentTypes.COMPONENT_GEOMETRY | ComponentTypes.COMPONENT_TEXTURE | ComponentTypes.COMPONENT_SHADER);

        protected int attribute_vtex;
        protected int attribute_vpos;
        protected int attribute_vnorm;
        protected int uniform_stex;
        protected int uniform_mModel;
        protected int uniform_mView;
        protected int uniform_mProj;
        protected int uniform_vlightPosition;
        protected int uniform_veyePosition;
        protected int uniform_vambient;
        protected int uniform_vdiffuse;
        protected int uniform_vspecular;
        protected int uniform_fspecularPower;
        protected int currentShader = -1;
        protected int uniform_ftime;

        List<Entity> entityList;
        List<Camera> cameraList;

        Vector4 lightPosition;

        Rectangle clientRectangle;

        public SystemRender(List<Camera> inCameraList, Vector4 inLightPosition, Rectangle clientRectangleIn)
        {
            entityList = new List<Entity>();
            cameraList = inCameraList;
            lightPosition = inLightPosition;
            clientRectangle = clientRectangleIn;
        }

        public string Name
        {
            get { return "SystemRender"; }
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

                IComponent geometryComponent = components.Find(delegate (IComponent component)
                {
                    return component.ComponentType == ComponentTypes.COMPONENT_GEOMETRY;
                });
                Geometry geometry = ((ComponentGeometry)geometryComponent).Geometry();

                IComponent transformComponent = components.Find(delegate (IComponent component)
                {
                    return component.ComponentType == ComponentTypes.COMPONENT_TRANSFORM;
                });

                IComponent textureComponent = components.Find(delegate (IComponent component)
                {
                    return component.ComponentType == ComponentTypes.COMPONENT_TEXTURE;
                });
                int texture = ((ComponentTexture)textureComponent).Texture;

                IComponent shaderComponent = components.Find(delegate (IComponent component)
                {
                    return component.ComponentType == ComponentTypes.COMPONENT_SHADER;
                });
                int pgmID = ((ComponentShader)shaderComponent).PgmID;

                if (((ComponentTransform)transformComponent).SetTransform == false)
                {
                    SetTransform((ComponentTransform)transformComponent);
                    ((ComponentTransform)transformComponent).SetTransform = true;
                }
                Matrix4 transform = ((ComponentTransform)transformComponent).Transform;

                if (entity.Name.ToLower() == "skybox")
                {
                    DrawSkyBox(transform, geometry, texture, pgmID);
                }
                else
                {
                    Draw(transform, geometry, texture, pgmID, entity.Name.ToLower());
                }
            }
        }

        public void SetTransform(ComponentTransform transform)
        {
            Matrix4 scaleMat = Matrix4.CreateScale(transform.Scale);
            Matrix4 rotateMat = Matrix4.CreateRotationX(transform.Rotation.X) * Matrix4.CreateRotationY(transform.Rotation.Y) * Matrix4.CreateRotationZ(transform.Rotation.Z);
            Matrix4 translateMat = Matrix4.CreateTranslation(transform.Translation);

            transform.Transform = scaleMat * rotateMat * translateMat;

            transform.Up = new Vector3(transform.Transform[1, 0], transform.Transform[1, 1], transform.Transform[1, 2]);
            transform.Forward = -new Vector3(transform.Transform[2, 0], transform.Transform[2, 1], transform.Transform[2, 2]);
            transform.Right = new Vector3(transform.Transform[0, 0], transform.Transform[0, 1], transform.Transform[0, 2]);
        }

        /// <summary>
        /// binds all attributes to the shader
        /// </summary>
        /// <param name="pgmID"> the id of the shader to be bound </param>
        public void BindShader(int pgmID)
        {
            GL.UseProgram(pgmID);

            attribute_vpos = GL.GetAttribLocation(pgmID, "a_Position");
            attribute_vtex = GL.GetAttribLocation(pgmID, "a_TexCoord");
            attribute_vnorm = GL.GetAttribLocation(pgmID, "a_Normal");

            uniform_veyePosition = GL.GetUniformLocation(pgmID, "eyePosition");
            uniform_mModel = GL.GetUniformLocation(pgmID, "uModel");
            uniform_mView = GL.GetUniformLocation(pgmID, "uView");
            uniform_mProj = GL.GetUniformLocation(pgmID, "uProj");
            uniform_vlightPosition = GL.GetUniformLocation(pgmID, "lightPosition");

            uniform_stex = GL.GetUniformLocation(pgmID, "s_texture");
            uniform_vambient = GL.GetUniformLocation(pgmID, "ambientFactor");
            uniform_vdiffuse = GL.GetUniformLocation(pgmID, "diffuseFactor");
            uniform_vspecular = GL.GetUniformLocation(pgmID, "specularFactor");
            uniform_fspecularPower = GL.GetUniformLocation(pgmID, "specularPower");

            uniform_ftime = GL.GetUniformLocation(pgmID, "time");

            //if (attribute_vpos == -1 || attribute_vtex == -1 || uniform_stex == -1 || uniform_mModel == -1 || uniform_mView == -1 || uniform_mProj == -1 || uniform_veyePosition == -1)
            //{
            //    Console.WriteLine("Error binding attributes");
            //}
        }

        public void DrawSkyBox(Matrix4 transform, Geometry geometry, int texture, int pgmID)
        {
            if (currentShader != pgmID)
            {
                BindShader(pgmID);
                currentShader = pgmID;
            }
            else
            {
                GL.UseProgram(currentShader);
            }

            GL.Uniform1(uniform_stex, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, texture);

            Matrix4 view = cameraList[0].View;
            view.M41 = 0;
            view.M42 = 0;
            view.M43 = 0;
            Matrix4 proj = cameraList[0].Projection;
            GL.UniformMatrix4(uniform_mModel, true, ref transform);
            GL.UniformMatrix4(uniform_mView, true, ref view);
            GL.UniformMatrix4(uniform_mProj, true, ref proj);

            GL.Viewport(clientRectangle);

            geometry.Render();

            GL.BindVertexArray(0);
            GL.UseProgram(0);
        }

        /// <summary>
        /// Draws the current object using the correct shader
        /// </summary>
        /// <param name="transform"> the transform of the current entity to be drawn </param>
        /// <param name="geometry"> the geometry of the current entity </param>
        /// <param name="texture"> the location of the texture for the current entity </param>
        /// <param name="pgmID"> the program id of the shader to be used </param>
        public void Draw(Matrix4 transform, Geometry geometry, int texture, int pgmID, string name)
        {
            if (currentShader != pgmID)
            {
                BindShader(pgmID);
                currentShader = pgmID;
            }
            else
            {
                GL.UseProgram(currentShader);
            }

            GL.Uniform1(uniform_stex, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texture);

            //Matrix4 worldViewProjection = transform * camera.View * camera.Projection;
            Matrix4 view = cameraList[0].View;
            Matrix4 proj = cameraList[0].Projection;
            GL.UniformMatrix4(uniform_mModel, true, ref transform);
            GL.UniformMatrix4(uniform_mView, true, ref view);
            GL.UniformMatrix4(uniform_mProj, true, ref proj);

            GL.Viewport(clientRectangle);

            Vector4 eyePos = new Vector4(cameraList[0].Position); // get camera position
            GL.Uniform4(uniform_veyePosition, eyePos);
            Vector4 lightPos = lightPosition; // get light position
            GL.Uniform4(uniform_vlightPosition, lightPos);
            Vector4 ambientCol = new Vector4(0.2f); // ambient colour
            GL.Uniform4(uniform_vambient, ambientCol);
            Vector4 diffuseColour = new Vector4(0.8f); // diffuse colour
            GL.Uniform4(uniform_vdiffuse, diffuseColour);
            Vector4 specularColour = new Vector4(0.9f); // specular colour
            GL.Uniform4(uniform_vspecular, specularColour);
            GL.Uniform1(uniform_fspecularPower, 2f); // specular power
            float timer = Managers.SceneManager.time;
            GL.Uniform1(uniform_ftime, timer);

            if (name != "player")
            {
                geometry.Render();
            }

            GL.Viewport(clientRectangle.Width - 300, clientRectangle.Height - 300, 300, 300);

            view = cameraList[1].View;
            proj = cameraList[1].Projection;
            GL.UniformMatrix4(uniform_mView, true, ref view);
            GL.UniformMatrix4(uniform_mProj, true, ref proj);

            eyePos = new Vector4(cameraList[1].Position); // get camera position
            GL.Uniform4(uniform_veyePosition, eyePos);
            lightPos = new Vector4(0, 10, 0, 1); // get light position
            GL.Uniform4(uniform_vlightPosition, lightPos);

            geometry.Render();

            GL.BindVertexArray(0);
            GL.UseProgram(0);
        }
    }
}
