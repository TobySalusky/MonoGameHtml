
<p align="center">
	<img src="/MonoGameHtml/docs/images/MonoGameHtmlLogo.png" width="50%" height="auto"/>
</p>


<h1 style="text-align: center;">MonoGameHtml</h1>


**MonoGameHtml** is a library that brings *React-like* **HTML-based UI** development to the MonoGame environment using C#.

> **Note:** Although it may look like it, **no Javascript is used**. MonoGameHtml is intended for use with **C#**.

Essentially, this package serves as an extension to the C# language in which Html UI-nodes can be easily created with *embedded C# fragments*.

**UI-components** are defined in a functional manner *(borrowing some JS syntax)*:
```cs
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
```
![fruit list example component](https://github.com/TobySalusky/MonoGameHtml/blob/main/MonoGameHtml/docs/images/FruitListExample.PNG?raw=true)

---
### Features:
* Functional Components
* State Hooks (cause node-rebuild/rerender)
* Default and custom macros
* Certain CSS attributes (still plenty yet to be implemented)
* Inline CSS attributes
* CSS classes
* Pre-written set of common components (Text box, Slider, FPS counter, control-flow, etc.)
* Loading components and CSS from files (.monohtml and .css, respectively)
* Caching
---
#### Simple Example Program:
```cs
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
                        
            // HtmlMain MUST be initialized with the Game instance.
            HtmlMain.Initialize(this);
            
            // this is how you pass in outside variables (without using static access)
	    // note: the name provided must be prefaced by a dollar sign when used in UI-code (ex. $exampleVariable)
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
            
            // updates html (uses a GameTime, MouseState, and KeyState)
            htmlInstance?.Update(gameTime, Mouse.GetState(), Keyboard.GetState());
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            base.Draw(gameTime);
            
            // renders html (uses a Spritebatch)
            htmlInstance?.Render(_spriteBatch);
        }
    }
}
```
