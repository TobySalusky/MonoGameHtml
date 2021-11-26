using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameHtml;

namespace Example {
    public class ExamplePickerGame : Game {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public HtmlRunner htmlInstance;
        
        public ExamplePickerGame() {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize() {
            base.Initialize();

            InitializeHtml();
        }
        
        private async void InitializeHtml() {

            // Getting paths to assets (scripts & CSS)
            string assetPath = Path.Join(Directory.GetParent(Environment.CurrentDirectory).Parent!.Parent!.FullName, "Examples/ExamplePicker/Assets");
            string cssPath = Path.Join(assetPath, "CSS");
            string scriptPath = Path.Join(assetPath, "Scripts");
            string cachePath = Path.Join(assetPath, "Cache");

            // Initialize HtmlMain
            HtmlMain.Initialize(this,
                cachePath: cachePath,
                cacheIdentifier: "ExamplePicker",
                loggerSettings: new LoggerSettings {
                    logOutput = true
                });
            
            // Initialize CSS
            CSSHandler.SetCSS(Path.Join(cssPath, "Styles.css"));

            
            // Generate runner instance
            htmlInstance = await HtmlProcessor.GenerateRunner("<App/>", 
                    assemblies: new[] {typeof(Program).Assembly},
                    imports: new []{"System.Reflection"},
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
            
            // updates html
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