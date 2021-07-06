using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
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

        public string cssPath;
        
        // test objects
        public Texture2D bush;

        public GameMain() {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
            // REMOVES LIMIT FROM FPS
            _graphics.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = false;
        }

        protected override void Initialize() {
            // TODO: Add your initialization logic here
            base.Initialize();
            
            _graphics.PreferredBackBufferHeight = 1080/2;
            _graphics.PreferredBackBufferWidth = 1920/2;
            _graphics.IsFullScreen = false;
            Window.IsBorderless = false;
            Window.AllowUserResizing = true;
            _graphics.ApplyChanges();
            
            string assetPath = Path.Join(Directory.GetParent(Environment.CurrentDirectory).Parent!.Parent!.FullName,
                "Assets");

            cssPath = Path.Join(assetPath, "CSS");
            
            HtmlMain.Initialize(this, 
                fontPath: Path.Join(assetPath, "Fonts"),
                cachePath: Path.Join(assetPath, "Cache"),
                loggerSettings: new LoggerSettings {
                    logOutput = true, 
                    allowColor = true,
                    colorOutputCS = false,
                    formatColoredCS = true,
                });
            
            CSSHandler.SetCSS(Path.Join(cssPath, "Styles.css"));
            
            //SetUpHtml();
            //Thesaurus.Init(this);
            HtmlWriter.Init(this);
        }

        private async void SetUpHtml() {
            
            const string components = @"
const Move = (string text) => {
    
    const float c = 150;
    float lastX = -1;
    float width = 400;

    return (
        <div -width={int~: width} -backgroundColor={Color: Color.Lerp(Color.Cyan, Color.Orange, (width - c) / (ScreenWidth-2*c))} onMouseExit={()=^lastX = -1} onHover={()=^ {
             float x = @mp.X;
             if (lastX != -1) width = Math.Clamp(width + (x - lastX), c, ScreenWidth-c);
             lastX = x;
        }}>
            {text}
        </div>
    );
}

const Container = (List<string> init) => {

    List^^string^ [rows, setRows] = useState(init);

    return (
        <div alignX='center' alignY='flexStart' width='100%'>
            {rows.map((str, i) =^
                <Row rows={rows} setRows={setRows} i={i}/>
            )}
            <div onPress={() =^ {
                rows.Add($'new {random()}');
                setRows(rows);
            }} textAlign='center' width='50%' height={$h} backgroundColor='white' borderColor='#888888' borderWidth={2}>+</div>
        </div>
    );
}

const Row = (List<string> rows, Action<List<string>> setRows, int i = -1) => {
    return (
        <div flexDirection='row' alignItems='center' width='50%' height={$h}>
            <div flex={5} alignY='center' borderColor='#888888' borderWidth={2} backgroundColor='white' textAlign='center'>
                {rows[i]}
            </div> 
            <div onPress={() =^ {
                rows.RemoveAt(i);
                setRows(rows); 
            }} flex={1} align='center' borderColor='#888888' borderWidth={2} backgroundColor='white' textAlign='center'>
                -
            </div>
        </div>
    );
}

const Test = (string str) => {

	var nestDict = DelimPair.searchAll(str, DelimPair.CurlyBrackets);    

    var dict = DelimPair.toDict(DelimPair.genPairs(str, '<', '>', (str, delim, i) => DelimPair.allNestOf(0, str.nestAmountsLen(i, delim.Length, nestDict))));

	return (
        <span backgroundColor='white'>
            {str.ToCharArray().map((c, i) =^
                <p color={(dict.ContainsKey(i)) ? 'red' : 'black'}>{c}</p>
            )} 
        </span>
    );
}
";

            const string html = @"
<body>
    <Switch caseFunc={string: $path}>
        <div backgroundColor='red' dimens={100} case='a' onPress={()=^UpdateVar('path', $path + 'b')}/>
        <div backgroundColor='blue' dimens={100} case='ab' onPress={()=^UpdateVar('path', $path + 'c')}/>
        <div backgroundColor='green' dimens={100} case='abc' onPress={()=^UpdateVar('path', 'a')}/>
    </Switch>
</body>
";
            // TODO: ADD OBJECT POOLING!
            
            var list = new List<string> {"Task 1", "Task 2", "Task 3"};

            var pack = StatePack.Create(
                "strs", list,
                "h", 59,
                "bush", bush,
                "text", "this is text",
                "test", "<div code={List<string>: () => new List<string>()}></div>",
                "path", "a"
                );
            
            htmlInstance = await HtmlProcessor.GenerateRunner(html, pack, 
                components: HtmlComponents.Create(
                components, HtmlComponents.Slider, HtmlComponents.Toggle, 
                HtmlComponents.FrameCounter, HtmlComponents.AllInput, HtmlComponents.Switch
            ), macros: Macros.create(

                    ));
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            bush = Content.Load<Texture2D>("bush");

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            htmlInstance?.Update(gameTime, Mouse.GetState(), Keyboard.GetState());

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
