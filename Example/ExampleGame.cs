using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameHtml;

namespace Example {
    public class ExampleGame : Game {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public HtmlRunner htmlInstance;

        public ExampleGame() {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize() {
            base.Initialize();
            
            InitializeHtml();
        }

        private async void InitializeHtml() {
            // HtmlMain MUST be initialized with the Game instance.
            HtmlMain.Initialize(this);

            // components can be defined either in strings or .monohtml files
            const string components = @"
const App = () => {
    
    var fruitList = new List<string> {'apple', 'orange', 'banana'};
    
    return (
        <div backgroundColor='white' @fill>
            <h4 color='blue'>Comprehensive List of Fruit:</h4>
            <div marginLeft={20}>
                {fruitList.map(fruit => 
                    <p>{fruit}</p>
                )}
            </div>
        </div>
    );
}
";
            // this compiles the components provided and creates a runnable HTML Instance.
            htmlInstance = await HtmlProcessor.GenerateRunner("<App/>", 
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