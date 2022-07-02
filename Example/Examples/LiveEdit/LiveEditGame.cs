using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameHtml;

namespace Example {
    public class LiveEditGame : Game {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public HtmlRunner htmlInstance;

        private string scriptPath;

        public LiveEditGame() {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize() {
            base.Initialize();
            
            InitializeHtml();
        }

        protected override void OnExiting(object sender, EventArgs args) {

            HtmlSettings.generateCache = true;
            HtmlProcessor.GenerateRunner("<App/>", 
                components: HtmlComponents.Create(HtmlComponents.ReadFrom(Path.Join(scriptPath))));
            
            base.OnExiting(sender, args);
        }

        private async void InitializeHtml() {

            string assetPath = Path.Join(Directory.GetParent(Environment.CurrentDirectory).Parent!.Parent!.FullName, "Examples/LiveEdit/Assets");
            string cssPath = Path.Join(assetPath, "CSS");
            scriptPath = Path.Join(assetPath, "Scripts");
            string cachePath = Path.Join(assetPath, "Cache");
            
            // Initialize HtmlMain
            HtmlMain.Initialize(this,
                cachePath: cachePath,
                cacheIdentifier: "LiveEdit"
            );
            HtmlSettings.generateCache = false;

            
            // This is an example of how to use the **experimental** Live-Edit feature
            htmlInstance = await HtmlLiveEdit.Create(async () => { 
                
                CSSHandler.SetCSSFiles(Path.Join(cssPath, "Styles.css"));

                Console.WriteLine("Compiling...");

                // this compiles the components provided and creates a runnable HTML Instance.
                return await HtmlProcessor.GenerateRunner("<App/>", 
                    components: HtmlComponents.Create(HtmlComponents.ReadFrom(Path.Join(scriptPath))));
            }, assetPath);
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