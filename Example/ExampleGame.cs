using System;
using System.IO;
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
    int [childCount, setChildCount] = useState(1);
    
    return (
        <span onPress={()=>{
            setChildCount(childCount + 1);
        }}>
            {nStream(childCount).map(i => 
                <h4 borderWidth={1} borderColor='black' backgroundColor='lightgray'>{i}</h4>
            )}
        </span>
    );
}
";
            
            string assetPath = Path.Join(Directory.GetParent(Environment.CurrentDirectory).Parent!.Parent!.FullName, "Assets");
            string cssPath = Path.Join(assetPath, "CSS");
            string scriptPath = Path.Join(assetPath, "Scripts");

            
            // This is an example of how to use the **experimental** Live-Edit feature
            htmlInstance = await HtmlLiveEdit.Create(async () => { 
                
                CSSHandler.SetCSS(Path.Join(cssPath, "Styles.css"));

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