using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameHtml;

namespace Example {
    public class InventoryGame : Game {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public HtmlRunner htmlInstance;

        private Inventory inventory;
        private KeyboardState lastKeyboardState;

        private Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

        public InventoryGame() {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize() {
            base.Initialize();
                        
            textures["Apple"] = Content.Load<Texture2D>("Apple");
            textures["Stick"] = Content.Load<Texture2D>("Stick");
            textures["Sword"] = Content.Load<Texture2D>("Sword");

            InitializeHtml();
        }
        
        private async void InitializeHtml() {

            // Getting paths to assets (scripts & CSS)
            string assetPath = Path.Join(Directory.GetParent(Environment.CurrentDirectory).Parent!.Parent!.FullName, "Examples/Inventory/Assets");
            string cssPath = Path.Join(assetPath, "CSS");
            string scriptPath = Path.Join(assetPath, "Scripts");
            string cachePath = Path.Join(assetPath, "Cache");

            // Initialize HtmlMain
            HtmlMain.Initialize(this,
                cachePath: cachePath,
                cacheIdentifier: "Inventory",
                loggerSettings: new LoggerSettings {
                    logOutput = true
                });

            // Initialize CSS
            CSSHandler.SetCSS(Path.Join(cssPath, "Styles.css"));
            
            // create outside variable
            inventory = new Inventory();
            
            // Generate runner instance
            htmlInstance = await HtmlProcessor.GenerateRunner("<App/>", 
                    statePack: StatePack.Create(
                        "inventory", inventory,
                        "textures", textures
                        ),
                    assemblies: new[] {typeof(Program).Assembly},
                    components: HtmlComponents.Create(HtmlComponents.ReadFrom(Path.Join(scriptPath))));
        }

        protected override void LoadContent() {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime) {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            
            base.Update(gameTime);

            // Game update
            KeyboardState keyboardState = Keyboard.GetState();
            
            if (keyboardState.IsKeyDown(Keys.I) && lastKeyboardState.IsKeyUp(Keys.I)) {
                inventory.Open = !inventory.Open;
            }

            lastKeyboardState = keyboardState;
            
            Program.GoBackIfPressedCtrlB(keyboardState);

            // HTML update
            htmlInstance?.Update(gameTime, Mouse.GetState(), Keyboard.GetState());
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            base.Draw(gameTime);
            
            // renders html
            htmlInstance?.Render(_spriteBatch);
        }
    }
}