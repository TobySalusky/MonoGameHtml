using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameHtml;

namespace Example {
    public class Game1 : Game {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public HtmlRunner htmlInstance;

        public Game1() {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize() {
            base.Initialize();
            
            InitializeHtml();
        }

        private async void InitializeHtml() {
            
            // this line gets the absolute path to the asset folder
            string assetPath = Path.Join(Directory.GetParent(Environment.CurrentDirectory).Parent!.Parent!.FullName, "Assets");
            
            // HtmlMain MUST be initialized with the Game instance, as well as a path to the fonts.
            HtmlMain.Initialize(this, 
                fontPath: Path.Join(assetPath, "Fonts"));
            
            // this is how you pass in outside variables (without simply using static access)
            StatePack pack = StatePack.Create(
                "exampleVariable", 10
            );

            // components can be defined either in strings or .monohtml files
            const string components = @"
const App = () => {
    
    var fruitList = new List<string> {'apple', 'orange', 'banana'};
    
    return (
        <div backgroundColor='white' @fill>
            <h4 color='blue'>Comprehensive List of Fruit:</h4>
            {fruitList.map(fruit => 
                <p>{fruit}</p>
            )}
        </div>
    );
}
";
            // this compiles the components provided and creates a runnable HTML Instance.
            htmlInstance = await HtmlProcessor.GenerateRunner("<App/>", pack, 
                components: HtmlComponents.Create(components));
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