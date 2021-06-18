using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameHtml;

namespace Testing
{
    public class GameMain : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public HtmlRunner htmlInstance;

        public GameMain()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            base.Initialize();
            
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.IsFullScreen = true;
            Window.IsBorderless = false;
            Window.AllowUserResizing = true;
            _graphics.ApplyChanges();
            
            HtmlMain.Initialize(this, 
                fontPath: @"D:\Users\Tobafett\Documents\GitHub\MonoGameHtml\Testing\Assets\Fonts\",
                logOutput: false);

            SetUpHtml();
        }

        public async void SetUpHtml() { 
            const string stateTest = @"
                const Test = (string name, int number) => {

                    return (
                        <h3 backgroundColor='green' borderRadius='50%' borderColor='#FFFFFF'  borderWidth={5}>
                            {name}: {number}
                        </h3>
                    );
                }
                ";
            

            const string html = @"
                <div flexDirection='row' dimens='100%' alignX='center' alignY='flexStart'>

                    <div dimens={500} backgroundColor='red'>HI</div>
                    
                    {$array.map((name, i) => 
                        <Test name={name} number={i}/>
                    )}
                </div>
                ";
            
            var statePack = new StatePack(
                "array", new []{"hi", "hello"}
            );

            htmlInstance = await HtmlProcessor.GenerateRunner(html, statePack, 
                macros: Macros.create(
                "div(html)", "<div>$$html</div>"
            ), components: HtmlComponents.Create(
                stateTest
            ));
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            htmlInstance?.Update(gameTime, Mouse.GetState());

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            htmlInstance?.Render(_spriteBatch);
            
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
