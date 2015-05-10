using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace GameProject
{
    /// <summary>
    /// A class for a burger
    /// </summary>
    public class Burger
    {
        #region Fields

        // graphic and drawing info
        Texture2D sprite;
        Rectangle drawRectangle;

        // burger stats
        int health = 100;

        // shooting support
        bool canShoot = true;
        int elapsedCooldownTime = 0;

        // sound effect
        SoundEffect shootSound;

        #endregion

        #region Constructors

        /// <summary>
        ///  Constructs a burger
        /// </summary>
        /// <param name="contentManager">the content manager for loading content</param>
        /// <param name="spriteName">the sprite name</param>
        /// <param name="x">the x location of the center of the burger</param>
        /// <param name="y">the y location of the center of the burger</param>
        /// <param name="shootSound">the sound the burger plays when shooting</param>
        public Burger(ContentManager contentManager, string spriteName, int x, int y,
            SoundEffect shootSound)
        {
            LoadContent(contentManager, spriteName, x, y);
            this.shootSound = shootSound;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the collision rectangle for the burger
        /// </summary>
        public Rectangle CollisionRectangle
        {
            get { return drawRectangle; }
        }

        /// <summary>
        /// The health of the burger.
        /// </summary>
        public int Health
        {
            get
            {
                return health;
            }

            set
            {
                if (value > 100)
                    health = 100;
                else if (value < 0)
                    health = 0;
                else
                    health = value;
            }
        }

        #endregion

        #region Private properties

        /// <summary>
        /// Gets and sets the x location of the center of the burger
        /// </summary>
        private int X
        {
            get { return drawRectangle.Center.X; }
            set
            {
                drawRectangle.X = value - drawRectangle.Width / 2;

                // clamp to keep in range
                if (drawRectangle.X < 0)
                {
                    drawRectangle.X = 0;
                }
                else if (drawRectangle.X > GameConstants.WINDOW_WIDTH - drawRectangle.Width)
                {
                    drawRectangle.X = GameConstants.WINDOW_WIDTH - drawRectangle.Width;
                }
            }
        }

        /// <summary>
        /// Gets and sets the y location of the center of the burger
        /// </summary>
        private int Y
        {
            get { return drawRectangle.Center.Y; }
            set
            {
                drawRectangle.Y = value - drawRectangle.Height / 2;

                // clamp to keep in range
                if (drawRectangle.Y < 0)
                {
                    drawRectangle.Y = 0;
                }
                else if (drawRectangle.Y > GameConstants.WINDOW_HEIGHT - drawRectangle.Height)
                {
                    drawRectangle.Y = GameConstants.WINDOW_HEIGHT - drawRectangle.Height;
                }
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Updates the burger's location based on mouse. Also fires 
        /// French fries as appropriate
        /// </summary>
        /// <param name="gameTime">game time</param>
        /// <param name="mouse">the current state of the mouse</param>
        public void Update(GameTime gameTime, KeyboardState keyboard)
        {
            // burger should only respond to input if it still has health
            if (health > 0)
            {
                // move burger using the keyboard and clamp it (the X and Y properties already clamp it).
                int movementX = 0;
                int movementY = 0;
                if (keyboard.IsKeyDown(Keys.W))
                    movementY -= GameConstants.BURGER_MOVEMENT_AMOUNT;
                if (keyboard.IsKeyDown(Keys.S))
                    movementY += GameConstants.BURGER_MOVEMENT_AMOUNT;
                if (keyboard.IsKeyDown(Keys.A))
                    movementX -= GameConstants.BURGER_MOVEMENT_AMOUNT;
                if (keyboard.IsKeyDown(Keys.D))
                    movementX += GameConstants.BURGER_MOVEMENT_AMOUNT;

                //If moving diagonally, reduce the movement speed this frame to prevent the Doom Strafe 40 bug.
                if ( Math.Abs(movementX) == Math.Abs(movementY))
                {
                    movementX /= 2;
                    movementY /= 2;
                }

                X += movementX;
                Y += movementY;

                // update shooting allowed
                //If shooting is disabled, 
                //  If enough time has passed or if the player released the firing button, enable shooting.
                if (!canShoot)
                {
                    elapsedCooldownTime += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

                    if (elapsedCooldownTime >= GameConstants.BURGER_COOLDOWN_MILLISECONDS || keyboard.IsKeyUp(Keys.Space))
                    {
                        canShoot = true;
                        elapsedCooldownTime = 0;
                    }
                }

                // timer concept (for animations) introduced in Chapter 7

                // shoot if appropriate
                if (keyboard.IsKeyDown(Keys.Space) && canShoot)
                {
                    //Create a new projectile to fire from the burger.
                    canShoot = false;
                    ProjectileType projectileType = ProjectileType.FrenchFries;
                    int locationY = Y - GameConstants.FRENCH_FRIES_PROJECTILE_OFFSET;
                    Projectile projectile = new Projectile(projectileType, Game1.GetProjectileSprite(projectileType), X, locationY,
                        -GameConstants.FRENCH_FRIES_PROJECTILE_SPEED);
                    Game1.AddProjectile(projectile);
                    shootSound.Play();
                }
            }
        }

        /// <summary>
        /// Draws the burger
        /// </summary>
        /// <param name="spriteBatch">the sprite batch to use</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(sprite, drawRectangle, Color.White);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Loads the content for the burger
        /// </summary>
        /// <param name="contentManager">the content manager to use</param>
        /// <param name="spriteName">the name of the sprite for the burger</param>
        /// <param name="x">the x location of the center of the burger</param>
        /// <param name="y">the y location of the center of the burger</param>
        private void LoadContent(ContentManager contentManager, string spriteName,
            int x, int y)
        {
            // load content and set remainder of draw rectangle
            sprite = contentManager.Load<Texture2D>(spriteName);
            drawRectangle = new Rectangle(x - sprite.Width / 2,
                y - sprite.Height / 2, sprite.Width,
                sprite.Height);
        }

        #endregion
    }
}
