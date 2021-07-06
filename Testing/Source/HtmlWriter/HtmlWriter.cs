using System;
using System.Threading.Tasks;
using MonoGameHtml;

namespace Testing {
    public static class HtmlWriter {
	    
        public static async void Init(GameMain gameMain) {
            
            const string componentDefs = @"
const App = () => {

	HtmlNode [node, setNode] = useState(null);

	string text = '';
	int updateCount = 0, currUpdateCount = 0;
	bool updating = false;
	Exception [exception, setException] = useState(null);

    return (
        <body flexDirection='row'>
        	<FrameCounter/>
        	<div flex={1} backgroundColor='gray'>
				<TextBox multiline={true} width='49%' height='90%'
				text={string: text} setText={(string str)=^text=str}
				diff={(Func^^string,string,string^)((string oldStr, string newStr)=^{
					updateCount++;
					return newStr;
				})}
				onTick={()=^{
					if (!updating && currUpdateCount != updateCount) {
					
						updating = true;
						Task.Run(()=^{
						    $updateHtml(text).ContinueWith(task =^ {
						    	updating = false;
								currUpdateCount = updateCount;
						    	
								setNode(null); //TODO: make it so this is not required!!!
								setException(task.Result.Item2);
								setNode(task.Result.Item1);
							});
						});
						
					}
				}}
				color='red' fontSize={30}
				/>
				<p>{updating ? $loadingText(@t) : ''}</p>
			</div>
			<div flex={1} backgroundColor='white'>
				<html/>
				{node ?? 
					(
						(exception == null || text == '') ? 
							<p>Nothing to display...</p> : 
							<p color='red'>{exception.GetType().Name + '\n' + exception.Message}</p>
					)
				}
			</div>
		</body>
    );
}
";

            const string html = "<App/>";
            StatePack pack = null;

            Func<string, Task<(HtmlNode, Exception)>> updateHtml = async (string text) => {
	            HtmlNode node = null;
	            Exception e = null;

	            try {
		            Task.Run(() => {
			            Task.Delay(1000).ContinueWith(task => {
				            //Logger.log("Ran out of time");
				            throw new Exception();
			            });
		            });
		            node = await HtmlProcessor.GenHtml(text, pack, macros: Macros.create(
			            "div(color, size)", "<div backgroundColor='$$color' dimens={$$size}/>",
			            "none", "<span/>"
		            ));
	            } catch (Exception err) {
		            e = err;
		            Logger.log(e.GetType().Name, e.Message);
	            }

	            return (node, e);
            };

            static string loadingText(float time) {
	            string str = "loading";
	            int dotCount = (int) (time % 1F / (1/3F)) + 1;
	            for (int i = 0; i < dotCount; i++) {
		            str += ".";
	            }
	            return str;
            }

            pack = StatePack.Create(
                "updateHtml", (Func<string, Task<(HtmlNode, Exception)>>) updateHtml,
                "loadingText", (Func<float, string>) loadingText
            );

            gameMain.htmlInstance = await HtmlProcessor.GenerateRunner(html, pack, 
                components: HtmlComponents.Create(componentDefs, HtmlComponents.AllInput, HtmlComponents.FrameCounter));

            HtmlSettings.generateCache = false;
        }
    }
}