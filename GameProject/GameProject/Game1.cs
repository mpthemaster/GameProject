using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace GameProject
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        #region Fields

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // game objects. Using inheritance would make this
        // easier, but inheritance isn't a GDD 1200 topic
        Burger burger;
        List<TeddyBear> bears = new List<TeddyBear>();
        static List<Projectile> projectiles = new List<Projectile>();
        List<Explosion> explosions = new List<Explosion>();

        // projectile and explosion sprites. Saved so they don't have to
        // be loaded every time projectiles or explosions are created
        static Texture2D frenchFriesSprite;
        static Texture2D teddyBearProjectileSprite;
        static Texture2D explosionSpriteStrip;

        // scoring support
        int score = 0;
        string scoreString = GameConstants.SCORE_PREFIX + 0;

        // health support
        string healthString = GameConstants.HEALTH_PREFIX + 
            GameConstants.BURGER_INITIAL_HEALTH;
        bool burgerDead = false;

        // text display support
        SpriteFont font;

        // sound effects
        SoundEffect burgerDamage;
        SoundEffect burgerDeath;
        SoundEffect burgerShot;
        SoundEffect explosion;
        SoundEffect teddyBounce;
        SoundEffect teddyShot;

        #endregion

        #region Constructor

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // set resolution
            graphics.PreferredBackBufferWidth = GameConstants.WINDOW_WIDTH;
            graphics.PreferredBackBufferHeight = GameConstants.WINDOW_HEIGHT;
        }

        #endregion

        #region Overridden Methods

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            RandomNumberGenerator.Initialize();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // load audio content
            burgerDamage = Content.Load<SoundEffect>(@"sounds\BurgerDamage");
            burgerDeath = Content.Load<SoundEffect>(@"sounds\BurgerDeath");
            burgerShot = Content.Load<SoundEffect>(@"sounds\BurgerShot");
            explosion = Content.Load<SoundEffect>(@"sounds\Explosion");
            teddyBounce = Content.Load<SoundEffect>(@"sounds\TeddyBounce");
            teddyShot = Content.Load<SoundEffect>(@"sounds\TeddyShot");

            // load sprite font
            font = Content.Load<SpriteFont>("Arial20");

            // load projectile and explosion sprites
            teddyBearProjectileSprite = Content.Load<Texture2D>("teddybearprojectile");
            frenchFriesSprite = Content.Load<Texture2D>("frenchfries");
            explosionSpriteStrip = Content.Load<Texture2D>("explosion");

            // add initial game objects
            burger = new Burger(Content, "burger", GameConstants.WINDOW_WIDTH / 2, GameConstants.WINDOW_HEIGHT * 7 / 8, burgerShot);

            //Spawn teddy bears.
            for (int i = 0; i < GameConstants.MAX_BEARS; i++)
                SpawnBear();

            // set initial health and score strings
            healthString = GameConstants.HEALTH_PREFIX + burger.Health;
            scoreString = GameConstants.SCORE_PREFIX + score;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            // get current mouse state and update burger
            burger.Update(gameTime, Keyboard.GetState());

            // update other game objects
            foreach (TeddyBear bear in bears)
            {
                bear.Update(gameTime);
            }
            foreach (Projectile projectile in projectiles)
            {
                projectile.Update(gameTime);
            }
            foreach (Explosion explosion in explosions)
            {
                explosion.Update(gameTime);
            }

            // check and resolve collisions between teddy bears
            for (int i = 0; i < bears.Count; i++)
                for (int j = i + 1; j < bears.Count; j++)
                {
                    TeddyBear firstBear = bears[i];
                    TeddyBear secondBear = bears[j];

                    //If both bears are still active, check if they collided.
                    if (firstBear.Active && secondBear.Active)
                    {
                        //Get collision data because if the bears collided they need to bounce off of each other.
                        CollisionResolutionInfo collisonResolutionInfo = CollisionUtils.CheckCollision((int)gameTime.ElapsedGameTime.TotalMilliseconds,
                            GameConstants.WINDOW_WIDTH, GameConstants.WINDOW_HEIGHT, firstBear.Velocity, firstBear.DrawRectangle, secondBear.Velocity,
                            secondBear.DrawRectangle);

                        //If the bears collided, resolve the collision
                        if (collisonResolutionInfo != null)
                        {
                            //If the first bear has been bounced outside of the window, kill it.
                            //Else the first bear has not been bounced outside of the window, so update its velocity and draw rectangle for the bounce.
                            if (collisonResolutionInfo.FirstOutOfBounds)
                                firstBear.Active = false;
                            else
                            {
                                firstBear.Velocity = collisonResolutionInfo.FirstVelocity;
                                firstBear.DrawRectangle = collisonResolutionInfo.FirstDrawRectangle;
                            }

                            //If the second bear has been bounced outside of the window, kill it.
                            //Else the second bear has not been bounced outside of the window, so update its velocity and draw rectangle for the bounce.
                            if (collisonResolutionInfo.SecondOutOfBounds)
                                secondBear.Active = false;
                            else
                            {
                                secondBear.Velocity = collisonResolutionInfo.SecondVelocity;
                                secondBear.DrawRectangle = collisonResolutionInfo.SecondDrawRectangle;
                            }
                            teddyBounce.Play();
                        }
                    }
                }


            // check and resolve collisions between burger and teddy bears
            foreach (TeddyBear bear in bears)
                if (bear.Active && bear.CollisionRectangle.Intersects(burger.CollisionRectangle))
                {
                    bear.Active = false;
                    burger.Health -= GameConstants.BEAR_DAMAGE;
                    explosions.Add(new Explosion(explosionSpriteStrip, bear.Location.X, bear.Location.Y, explosion));
                    burgerDamage.Play();
                    CheckBurgerKill();
                    healthString = GameConstants.HEALTH_PREFIX + burger.Health;
                }

            // check and resolve collisions between burger and projectiles
            foreach (Projectile projectile in projectiles)
                if (projectile.Active && projectile.Type == ProjectileType.TeddyBear && projectile.CollisionRectangle.Intersects(burger.CollisionRectangle))
                {
                    projectile.Active = false;
                    burger.Health -= GameConstants.TEDDY_BEAR_PROJECTILE_DAMAGE;
                    burgerDamage.Play();
                    CheckBurgerKill();
                    healthString = GameConstants.HEALTH_PREFIX + burger.Health;
                }

            //Check and resolve collisions between teddy bears and projectiles.
            foreach (TeddyBear bear in bears)
                foreach (Projectile projectile in projectiles)
                    if (projectile.Type == ProjectileType.FrenchFries && bear.Active && projectile.Active &&
                        bear.CollisionRectangle.Intersects(projectile.CollisionRectangle))
                    {
                        bear.Active = false;
                        projectile.Active = false;
                        explosions.Add(new Explosion(explosionSpriteStrip, bear.Location.X, bear.Location.Y, explosion));
                        score += GameConstants.BEAR_POINTS;
                        scoreString = GameConstants.SCORE_PREFIX + score;
                    }

            // clean out inactive teddy bears and add new ones as necessary
            for (int i = bears.Count - 1; i >= 0; i--)
                if (!bears[i].Active)
                    bears.RemoveAt(i);

            while (bears.Count < GameConstants.MAX_BEARS)
                SpawnBear();

            // clean out inactive projectiles
            for (int i = projectiles.Count - 1; i >= 0; i--)
                if (!projectiles[i].Active)
                    projectiles.RemoveAt(i);

            // clean out finished explosions
            for (int i = explosions.Count - 1; i >= 0; i--)
                if (explosions[i].Finished)
                    explosions.RemoveAt(i);

                base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            // draw game objects
            burger.Draw(spriteBatch);
            foreach (TeddyBear bear in bears)
            {
                bear.Draw(spriteBatch);
            }
            foreach (Projectile projectile in projectiles)
            {
                projectile.Draw(spriteBatch);
            }
            foreach (Explosion explosion in explosions)
            {
                explosion.Draw(spriteBatch);
            }

            // draw score and health
            spriteBatch.DrawString(font, healthString, GameConstants.HEALTH_LOCATION, Color.White);
            spriteBatch.DrawString(font, scoreString, GameConstants.SCORE_LOCATION, Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Gets the projectile sprite for the given projectile type
        /// </summary>
        /// <param name="type">the projectile type</param>
        /// <returns>the projectile sprite for the type</returns>
        public static Texture2D GetProjectileSprite(ProjectileType type)
        {
            //If the projectile i s a French fry, then return the French fry sprite.
            //Else the projectile is a teddy bear projectile, so return the teddy bear projectile sprite.
            if (type == ProjectileType.FrenchFries)
                return frenchFriesSprite;
            else
                return teddyBearProjectileSprite;
        }

        /// <summary>
        /// Adds the given projectile to the game
        /// </summary>
        /// <param name="projectile">the projectile to add</param>
        public static void AddProjectile(Projectile projectile)
        {
            projectiles.Add(projectile);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Spawns a new teddy bear at a random location
        /// </summary>
        private void SpawnBear()
        {
            // generate random location
            int x = GetRandomLocation(GameConstants.SPAWN_BORDER_SIZE, GameConstants.WINDOW_WIDTH - GameConstants.SPAWN_BORDER_SIZE * 2);
            int y = GetRandomLocation(GameConstants.SPAWN_BORDER_SIZE, GameConstants.WINDOW_HEIGHT - GameConstants.SPAWN_BORDER_SIZE * 2);

            // generate random velocity
            float xVelocity = GameConstants.MIN_BEAR_SPEED + RandomNumberGenerator.NextFloat(GameConstants.BEAR_SPEED_RANGE);
            float yVelocity = GameConstants.MIN_BEAR_SPEED + RandomNumberGenerator.NextFloat(GameConstants.BEAR_SPEED_RANGE);
            float angle = RandomNumberGenerator.NextFloat((float)Math.PI * 2);

            Vector2 velocity = new Vector2(xVelocity * (float)Math.Cos(angle), yVelocity * (float)Math.Sin(angle));

            // create new bear
            TeddyBear newBear = new TeddyBear(Content, "teddybear", x, y, velocity, teddyBounce, teddyShot);

            // make sure we don't spawn into a collision
            List<Rectangle> collisionRectangles = GetCollisionRectangles();
            while (!CollisionUtils.IsCollisionFree(newBear.CollisionRectangle, collisionRectangles))
            {
                newBear.X = GetRandomLocation(GameConstants.SPAWN_BORDER_SIZE, GameConstants.WINDOW_WIDTH - GameConstants.SPAWN_BORDER_SIZE * 2);
                newBear.Y = GetRandomLocation(GameConstants.SPAWN_BORDER_SIZE, GameConstants.WINDOW_HEIGHT - GameConstants.SPAWN_BORDER_SIZE * 2);
            }

            // add new bear to list
            bears.Add(newBear);
        }

        /// <summary>
        /// Gets a random location using the given min and range
        /// </summary>
        /// <param name="min">the minimum</param>
        /// <param name="range">the range</param>
        /// <returns>the random location</returns>
        private int GetRandomLocation(int min, int range)
        {
            return min + RandomNumberGenerator.Next(range);
        }

        /// <summary>
        /// Gets a list of collision rectangles for all the objects in the game world
        /// </summary>
        /// <returns>the list of collision rectangles</returns>
        private List<Rectangle> GetCollisionRectangles()
        {
            List<Rectangle> collisionRectangles = new List<Rectangle>();
            collisionRectangles.Add(burger.CollisionRectangle);
            foreach (TeddyBear bear in bears)
            {
                collisionRectangles.Add(bear.CollisionRectangle);
            }
            foreach (Projectile projectile in projectiles)
            {
                collisionRectangles.Add(projectile.CollisionRectangle);
            }
            foreach (Explosion explosion in explosions)
            {
                collisionRectangles.Add(explosion.CollisionRectangle);
            }
            return collisionRectangles;
        }

        /// <summary>
        /// Checks to see if the burger has just been killed
        /// </summary>
        private void CheckBurgerKill()
        {
            if (burger.Health == 0 && !burgerDead)
            {
                burgerDead = true;
                burgerDeath.Play();
            }

        }

        #endregion
    }
}
