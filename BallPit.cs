using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System;
using CPI311.GameEngine;

namespace CourseProject
{
    public class CourseProject : Game
    {
        class Scene
        {
            public delegate void CallMethod(GameTime gameTime);
            public CallMethod Update;
            public CallMethod Draw;
            public Scene(CallMethod update, CallMethod draw)
            { Update = update; Draw = draw; }
        }
        Dictionary<String, Scene> scenes;
        Scene currentScene;
        List<GUIElement> guiElements;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Texture2D texture;
        Texture2D bg;
        Texture2D gameover;
        Texture2D congrats;
        Texture2D menu;
        Texture2D help;

        SpriteFont font;
        Effect effect;

        SoundEffect dingSound;
        SoundEffect fail;
        SoundEffect success;

        SoundEffectInstance soundInstance;
        SoundEffectInstance soundInstance2;
        SoundEffectInstance soundInstance3;

        Model model;
        List<Transform> transforms;
        List<Collider> colliders;
        List<Rigidbody> rb;
        
        //List<GameObject> gameObjects;

        Camera topDownCamera;
        List<Camera> cameras;

        int score = 0;
        float speedModifier = 1;
        bool soundToggle = false;

        Random random;

        public CourseProject()
            : base()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";

            //gameObjects = new List<GameObject>();
            
            transforms = new List<Transform>();
            colliders = new List<Collider>();
            rb = new List<Rigidbody>();


            cameras = new List<Camera>();

            guiElements = new List<GUIElement>();
            scenes = new Dictionary<string, Scene>();

            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            Time.Initialize();
            InputManager.Initialize();
            ScreenManager.Initialize(graphics);

            scenes.Add("Menu", new Scene(MainMenuUpdate, MainMenuDraw));
            scenes.Add("Help", new Scene(HelpUpdate, HelpDraw));
            scenes.Add("Play", new Scene(PlayUpdate, PlayDraw));
            scenes.Add("GameOver", new Scene(GameOverUpdate, GameOverDraw));
            scenes.Add("Congrats", new Scene(CongratsUpdate, CongratsDraw));

            currentScene = scenes["Menu"];

            base.Initialize();
        }

        void MainMenuUpdate(GameTime gameTime)
        {
            if (InputManager.IsKeyPressed(Keys.Enter))
                currentScene = scenes["Play"];
            if (InputManager.IsKeyPressed(Keys.H))
                currentScene = scenes["Help"];
        }       

        void MainMenuDraw(GameTime gameTime)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(menu, new Rectangle(0, 0, 1280, 720), Color.White);
            spriteBatch.DrawString(font, "Press H for Help", new Vector2(25, 15), Color.DarkSlateBlue);
            spriteBatch.End();
        }

        void HelpUpdate(GameTime gameTime)
        {
            if (InputManager.IsKeyPressed(Keys.Escape))
                currentScene = scenes["Menu"];
        }

        void HelpDraw(GameTime gameTime)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(help, new Rectangle(0, 0, 1280, 720), Color.White);
            spriteBatch.End();
        }

        void GameOverUpdate(GameTime gameTime)
        {
            if(!soundToggle)
            {
                soundInstance2.Volume = 0.6f;
                soundInstance2.IsLooped = false;
                soundInstance2.Play();
                soundToggle = true;
            }
            
            if (InputManager.IsKeyPressed(Keys.Escape))
                Exit();
        }
        void GameOverDraw(GameTime gameTime)
        {

            spriteBatch.Begin();
            spriteBatch.Draw(gameover, new Rectangle(0, 0, 1280, 720), Color.White);
            spriteBatch.End();
        }

        void CongratsUpdate(GameTime gameTime)
        {
            if(!soundToggle)
            {
                soundInstance3.Volume = 0.6f;
                soundInstance3.IsLooped = false;
                soundInstance3.Play();
                soundToggle = true;
            }
            
            if (InputManager.IsKeyPressed(Keys.Escape))
                Exit();
        }
        void CongratsDraw(GameTime gameTime)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(congrats, new Rectangle(0, 0, 1280, 720), Color.White);
            spriteBatch.End();
        }

        void PlayDraw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.LightSkyBlue);
            spriteBatch.Begin();
            spriteBatch.Draw(bg, new Rectangle(0, 0, 1280, 720), Color.White);
            spriteBatch.DrawString(font, "POINTS: " + score, new Vector2(25, 15), Color.DarkSlateBlue);
            spriteBatch.End();
            foreach (Camera camera in cameras)
            {
                GraphicsDevice.DepthStencilState = new DepthStencilState();
                GraphicsDevice.Viewport = topDownCamera.Viewport;
                Matrix view = topDownCamera.View;
                Matrix projection = topDownCamera.Projection;

                effect.CurrentTechnique = effect.Techniques[1];
                effect.Parameters["View"].SetValue(view);
                effect.Parameters["Projection"].SetValue(projection);
                effect.Parameters["LightPosition"].SetValue(Vector3.Up * 10);
                effect.Parameters["CameraPosition"].SetValue(camera.Transform.Position);
                effect.Parameters["Shininess"].SetValue(20f);
                effect.Parameters["AmbientColor"].SetValue(new Vector3(0.2f, 0.2f, 0.2f));
                effect.Parameters["SpecularColor"].SetValue(new Vector3(0, 0, 0.5f));
                effect.Parameters["DiffuseTexture"].SetValue(texture);
                foreach (Transform transform in transforms)
                {
                    effect.Parameters["World"].SetValue(transform.World);
                    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        foreach (ModelMesh mesh in model.Meshes)
                            foreach (ModelMeshPart part in mesh.MeshParts)
                            {
                                GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                                GraphicsDevice.Indices = part.IndexBuffer;
                                GraphicsDevice.DrawIndexedPrimitives(
                                    PrimitiveType.TriangleList, part.VertexOffset, 0,
                                    part.NumVertices, part.StartIndex, part.PrimitiveCount);
                            }
                    }
                }
            }
            base.Draw(gameTime);
        }

        void PlayUpdate(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            foreach (Rigidbody rigidbody in rb)
                rigidbody.Update();

            if (score == 100)
            {
                currentScene = scenes["Congrats"];
            }
            else
            {
                if(score == 10)
                {
                    speedModifier = 1.5f;
                }
                else if(score == 20)
                {
                    speedModifier = 2f;
                }
                else if (score == 30)
                {
                    speedModifier = 2.5f;
                }
                else if (score == 40)
                {
                    speedModifier = 3f;
                }
                else if (score == 50)
                {
                    speedModifier = 3.5f;
                }
                else if (score == 60)
                {
                    speedModifier = 4f;
                }
                else if (score == 70)
                {
                    speedModifier = 4.5f;
                }
                else if (score == 80)
                {
                    speedModifier = 5f;
                }
                else if (score == 90)
                {
                    speedModifier = 5.5f;
                }
            }
            

            Ray ray = topDownCamera.ScreenPointToWorldRay(InputManager.GetMousePosition());
            for (int i = 0; i < colliders.Count; i++)
            {
                //Console.WriteLine((colliders[i].Transform.LocalPosition).ToString());
                if (colliders[i].Transform.LocalPosition.Y <= -100)
                {
                    currentScene = scenes["GameOver"];
                }

                colliders[i].Transform.Rotate(Vector3.Up, Time.ElapsedGameTime * 4);
                colliders[i].Transform.Rotate(Vector3.Right, Time.ElapsedGameTime * 4);
                colliders[i].Transform.Rotate(Vector3.Forward, Time.ElapsedGameTime * 4);

                foreach (Collider collider in colliders)
                {
                    effect.Parameters["DiffuseColor"].SetValue(
                        Color.Red.ToVector3());
                    (model.Meshes[0].Effects[0] as BasicEffect).DiffuseColor = Color.Red.ToVector3();


                }

                if (colliders[i].Intersects(ray) != null)
                {
                    if (InputManager.IsMousePressed(0))
                    {
                        soundInstance.Volume = 0.5f;
                        soundInstance.IsLooped = false;
                        soundInstance.Play();
                        
                        colliders.Remove(colliders[i]);
                        CreateSphere();
                        score++;
                    }
                    else
                    {
                        effect.Parameters["DiffuseColor"].SetValue(Color.GhostWhite.ToVector3());
                        (model.Meshes[0].Effects[0] as BasicEffect).DiffuseColor = Color.Blue.ToVector3();
                    }
                }
            }
        }

        protected override void LoadContent()
        {
            ScreenManager.Setup(false, 1280, 720);

            spriteBatch = new SpriteBatch(GraphicsDevice);
            effect = Content.Load<Effect>("SimpleShading");
            model = Content.Load<Model>("Sphere");
            font = Content.Load<SpriteFont>("Font");

            texture = Content.Load<Texture2D>("Square");
            menu = Content.Load<Texture2D>("Menu");
            help = Content.Load<Texture2D>("help");
            bg = Content.Load<Texture2D>("pit2");
            gameover = Content.Load<Texture2D>("gameover");
            congrats = Content.Load<Texture2D>("congrats");

            dingSound = Content.Load<SoundEffect>("Ding2");
            fail = Content.Load<SoundEffect>("fail");
            success = Content.Load<SoundEffect>("success");
            soundInstance = dingSound.CreateInstance();
            soundInstance2 = fail.CreateInstance();
            soundInstance3 = success.CreateInstance();

            random = new Random();

            (model.Meshes[0].Effects[0] as BasicEffect).EnableDefaultLighting();

            CreateSphere();

            topDownCamera = new Camera();
            topDownCamera.Transform = new Transform();
            topDownCamera.Transform.LocalPosition = Vector3.Up * 10;
            topDownCamera.Transform.Rotate(Vector3.Right, -MathHelper.PiOver2);
            topDownCamera.Position = new Vector2(0f, 0f);
            topDownCamera.Size = new Vector2(1, 1);
            topDownCamera.AspectRatio = topDownCamera.Viewport.AspectRatio;

            cameras.Add(topDownCamera);
        }

        protected override void Update(GameTime gameTime)
        {
            InputManager.Update();
            Time.Update(gameTime);
            currentScene.Update(gameTime);
            base.Update(gameTime);
        }



        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.DepthStencilState = new DepthStencilState();
            currentScene.Draw(gameTime);
            base.Draw(gameTime);
        }

        void CreateSphere()
        {
            //fGameObject gameObject = new GameObject();
            Transform transform = new Transform();
            SphereCollider collider = new SphereCollider();
            Rigidbody rigidbody = new Rigidbody();
          
            rigidbody.Transform = transform;
            rigidbody.Mass = 0.3f + (float)random.NextDouble();
            rigidbody.Acceleration = Vector3.Down * 9.81f * speedModifier;
            Vector3 direction = new Vector3(-1.2f * (float)random.NextDouble(), 0, -1.2f * (float)random.NextDouble());
            rigidbody.Velocity = direction * ((float)random.NextDouble() * 6);

            transform.LocalPosition = new Vector3((float)random.Next(-14, 14), -5, (float)random.Next(-13, 13));
            collider.Radius = 1f;
            collider.Transform = transform;

            colliders.Add(collider);
            rb.Add(rigidbody);
            transforms.Add(transform);

            //gameObject.Add<Rigidbody>(rigidbody);
            //gameObject.Add<Collider>(collider);

            //gameObjects.Add(gameObject);

        }
    }
}