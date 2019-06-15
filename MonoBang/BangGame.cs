using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ERS
{
    /// <summary>
    /// The top module for the Bang game.
    /// </summary>
    public class BangGame : Microsoft.Xna.Framework.Game
    {
        public const int SCREEN_HEIGHT = 693, SCREEN_WIDTH = 860;
        public Color defaultColor = Color.ForestGreen;

        GraphicsDeviceManager graphics;

        public SpriteFont smallFont, cardFont, chatFont, typeFont;

        public Texture2D spades, clubs, hearts, diamonds, card, faceDownCard, bullet, pixel, arrow;

        KeyboardState keyboard, oldKeyboard;

        private Section game;

        public BangGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            graphics.PreferredBackBufferHeight = SCREEN_HEIGHT;
            graphics.PreferredBackBufferWidth = SCREEN_WIDTH;
            graphics.ApplyChanges();
            IsMouseVisible = true;
            Window.Title = "Bang!";
            oldKeyboard = Keyboard.GetState();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            MainProgram.spriteBatch = new SpriteBatch(GraphicsDevice);
            smallFont = this.Content.Load<SpriteFont>("smallFont");
            cardFont = this.Content.Load<SpriteFont>("cardFont");
            chatFont = this.Content.Load<SpriteFont>("cardFont");
            typeFont = this.Content.Load<SpriteFont>("typeFont");

            spades = this.Content.Load<Texture2D>("Suits\\spades");
            clubs = this.Content.Load<Texture2D>("Suits\\clubs");
            diamonds = this.Content.Load<Texture2D>("Suits\\diamonds");
            hearts = this.Content.Load<Texture2D>("Suits\\hearts");
            card = this.Content.Load<Texture2D>("card");
            faceDownCard = this.Content.Load<Texture2D>("card faceDown");
            bullet = this.Content.Load<Texture2D>("bullet");
            pixel = this.Content.Load<Texture2D>("pixel");
            arrow = this.Content.Load<Texture2D>("arrow");

            game = new GameController(null);
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
            keyboard = Keyboard.GetState();

            game.Update();

            if (keyboard.IsKeyDown(Keys.F2))
                game = new GameController(null);


            oldKeyboard = keyboard;
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(defaultColor);
            game.Draw();
            base.Draw(gameTime);
        }
    }
}
