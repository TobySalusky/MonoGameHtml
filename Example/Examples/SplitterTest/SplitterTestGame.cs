using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameHtml;

namespace Example {
    public class SplitterTestGame : Game {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public HtmlRunner htmlInstance;
        
        public SplitterTestGame() {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize() {
            base.Initialize();

            InitializeHtml();
        }
        
        private async void InitializeHtml() {

            // Getting paths to asset subsections
            string assetPath = Path.Join(Directory.GetParent(Environment.CurrentDirectory).Parent!.Parent!.FullName, "Examples/SplitterTest/Assets");
            string cssPath = Path.Join(assetPath, "CSS");
            string scriptPath = Path.Join(assetPath, "Scripts");
            string fontPath = Path.Join(assetPath, "Fonts");
            string cachePath = Path.Join(assetPath, "Cache");

            // Initialize HtmlMain
            HtmlMain.Initialize(this,
                fontPath: fontPath,
                cachePath: cachePath,
                cacheIdentifier: "SplitterTest",
                loggerSettings: new LoggerSettings {
                    logOutput = true
                });
            
            // Initialize CSS
            CSSHandler.SetCSSFiles(Path.Join(cssPath, "Styles.css"));

            
            // Generate runner instance
            htmlInstance = await HtmlProcessor.GenerateRunner("<App/>", 
                    components: HtmlComponents.Create(HtmlComponents.AllPanel, HtmlComponents.ReadFrom(Path.Join(scriptPath))));
        }

        protected override void LoadContent() {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime) {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            
            base.Update(gameTime);

            Program.GoBackIfPressedCtrlB(Keyboard.GetState());
            
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