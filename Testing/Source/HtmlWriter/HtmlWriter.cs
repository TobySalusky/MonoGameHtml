using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameHtml;

namespace Testing {
    public static class HtmlWriter {
	    
        public static async void Init(GameMain gameMain) {
	        const string componentDefs = @"
const SearchBar = (Action<string> setText, string path = '') => {

	List<(string stringName, string contents)> htmlSearchList = $searchHtml(path);

	return (
		<div>
			{true ? null : htmlSearchList.map(instance =>
				<p onPress={()=>setText(instance.contents)}>
					{instance.stringName}
				</p>
			)}
		</div>
	);
}

const App = () => {

	HtmlNode [node, setNode] = useState(null);

	string text = '';
	Action<string> setText = (string str)=> text=str;
	int updateCount = 0, currUpdateCount = 0;
	bool updating = false;
	Exception [exception, setException] = useState(null);
	
	TypingState typingState = null;
	
	string path = '/Users/toby/Documents/GitHub/MonoGameHtml/Testing/Source/HtmlWriter/HtmlWriter.cs';

	string correctText() {
		return text.Replace('\t', TextInputUtil.spacesPerTab);
	}

    return (
        <body flexDirection='row'>
        	<FrameCounter/>
			
			<SearchBar path={path} setText={setText}/>
			
        	<div flex={1} backgroundColor='#34353D'>
				<TextBox 
				class='HtmlBox'
				multiline={true}
				useTypingState={@set(TypingState, typingState)}
				text={string: text} setText={setText}
				diff={(Func<string,string,string>)((string oldStr, string newStr)=>{
					updateCount++;
					return $htmlDiff(oldStr, newStr, typingState);
				})}
				onTick={()=>{
					if (!updating && currUpdateCount != updateCount) {
					
						updating = true;
						Task.Run(()=>{
						    $updateHtml(updateCount, text).ContinueWith(task => {
						    	int thisUpdateCount = task.Result.Item3;
						    	if (thisUpdateCount > currUpdateCount) {
						    		updating = false;
									currUpdateCount = thisUpdateCount;
						    		
									setException(task.Result.Item2);
									setNode(task.Result.Item1);
						    	}
							});
						});
						
					}
				}}
				/>
				<h6 color='white'>{currUpdateCount}/{updateCount} {updating ? $loadingText(@t) : ''}</h6>
				<pseudo 
				renderAdd={(SpriteBatch spriteBatch)=>{ 
					$renderTabs(spriteBatch, text, typingState);
				}}
				/>
			</div>
			<div flex={1} backgroundColor='white'>
				<html/>
				{node ?? 
					(
						(exception == null || text == '') ? 
							<p>Nothing to display...</p> : 
							<p color='red'>{exception == null ? 'NULL?' : (exception.GetType().Name + '\n' + exception.Message)}</p>
					)
				}
			</div>
		</body>
    );
}
";

            const string html = "<App/>";
            StatePack pack = null;

            Func<int, string, Task<(HtmlNode, Exception, int)>> updateHtml = async (int updateCount, string text) => {
	            HtmlNode node = null;
	            Exception e = null;
	            
	            try {
		            node = await HtmlProcessor.GenHtml(text, pack, 
			            macros: Macros.create(
			            "div(color, size)", "<div backgroundColor='$$color' dimens={$$size}/>",
			            "none", "<span/>"),
			            components: HtmlComponents.Create(HtmlComponents.Slider, HtmlComponents.AllInput));
	            } catch (Exception err) {
		            e = err;
		            Logger.log(e.StackTrace);
	            }

	            return (node, e, updateCount);
            };

            static string loadingText(float time) {
	            string str = "loading";
	            int dotCount = (int) (time % 1F / (1/3F)) + 1;
	            for (int i = 0; i < dotCount; i++) {
		            str += ".";
	            }
	            return str;
            }

            Func<string, string, TypingState, string> htmlDiff = (oldStr, newStr, typingState) => {
	            string str = newStr;

	            static bool valid(char c) {
		            return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9');
	            }

	            if (newStr.Length == oldStr.Length + 1) {
		            // creating html open/closed tags on tab
		            if (typingState.cursorIndex >= 2 && newStr.CountOf("\t") > oldStr.CountOf("\t")) {
			            int beforeIndex = typingState.cursorIndex - 2;
		            
			            if (!valid(newStr[beforeIndex])) return str;
		            
			            int tagStart = beforeIndex;
			            for (int i = beforeIndex - 1; i >= 0; i--) {
				            char c = newStr[i];
				            if (!valid(c)) break;
				            tagStart = i;
			            }

			            string tag = newStr.Sub(tagStart, beforeIndex + 1);

			            str = newStr[..tagStart] + $"<{tag}></{tag}>" + newStr[typingState.cursorIndex..];
			            typingState.cursorIndex = tagStart + tag.Length + 2;

		            } else if (newStr.CountOf("\n") > oldStr.CountOf("\n")) { // maintain tabs between lines
			            int enterIndex = typingState.cursorIndex - 1;
			            bool openUpTags = (typingState.cursorIndex > 1 && typingState.cursorIndex < newStr.Length - 1) && 
			                              (newStr[enterIndex - 1] == '>' && newStr.Substring(typingState.cursorIndex, 2) == "</");
			            if (newStr[enterIndex] != '\n') return str;
			            int lastLineStart = 0;
			            for (int i = enterIndex - 1; i >= 0; i--) {
				            if (newStr[i] == '\n') {
					            lastLineStart = i;
					            break;
				            }
			            }

			            string tabs = "";
			            for (int i = lastLineStart + 1; i < newStr.Length; i++) {
				            if (newStr[i] == '\t') {
					            tabs += '\t';
					            continue;
				            }
				            break;
			            }

			            str = newStr[..typingState.cursorIndex] + tabs + (openUpTags ? $"\t\n{tabs}" : "") + newStr[typingState.cursorIndex..];
			            typingState.cursorIndex += tabs.Length;

			            if (openUpTags) typingState.cursorIndex++;
		            }
	            } else if (newStr.Length == oldStr.Length - 1) { // on delete
		            
		            // delete full line when only tabs // TODO: tabCount must be equal to or less than previous line
		            if (oldStr[typingState.cursorIndex] != '\t') return str;
		            int removeExtra = 0;
		            for (int i = typingState.cursorIndex - 1; i >= 0; i--) {
			            char c = newStr[i];
			            if (c == '\t') {
				            removeExtra++;
				            continue;
			            }
			            if (c == '\n') {
				            removeExtra++;
				            break;
			            }

			            removeExtra = 0;
			            break;
		            }

		            str = newStr[..(typingState.cursorIndex - removeExtra)] + newStr[typingState.cursorIndex..];
		            typingState.cursorIndex -= removeExtra;
	            }

	            return str;
            };

            Action<SpriteBatch, string, TypingState> renderTabs = (spriteBatch, text, typingState) => {
	            float height = typingState.node.font.FindHeight();
	            bool render = true;
	            for (int i = 0; i < text.Length; i++) {
		            if (text[i] == '\n') {
			            render = true;
			            continue;
		            }

		            if (text[i] != '\t') {
			            render = false;
			            continue;
		            }

		            if (!render) continue;

		            Vector2 pos = TextInputUtil.cursorPositionAtIndex(typingState.node, typingState, text, i);
		            spriteBatch.Draw(Textures.rect, new Rectangle((int)pos.X, (int)pos.Y, 1, (int) height), Color.Gray);
	            }
            };

            Func<string, List<(string stringName, string contents)>> searchHtml = (path) => {
	            string code = File.ReadAllText(path);
	            const string htmlStringStartRegex = @"\s([a-zA-Z0-9_@]+)\s=\s@""(.*?)""";
	            MatchCollection matches = Regex.Matches(code, htmlStringStartRegex, RegexOptions.Singleline);

	            var list = new List<(string, string)>();
	        
	            foreach (Match match in matches) {
		            list.Add((match.Groups[1].Value, match.Groups[2].Value));
	            }

	            return list;
            };

            pack = StatePack.Create(
                "updateHtml", updateHtml,
                "loadingText", (Func<float, string>) loadingText,
                "htmlDiff", htmlDiff,
                "renderTabs", renderTabs,
                "searchHtml", searchHtml
            );

            gameMain.htmlInstance = await HtmlProcessor.GenerateRunner(html, pack, 
                components: HtmlComponents.Create(componentDefs, HtmlComponents.AllInput, HtmlComponents.FrameCounter));

            HtmlSettings.generateCache = false;
        }
    }
}