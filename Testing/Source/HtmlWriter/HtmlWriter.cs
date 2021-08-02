using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

const Predictor = (
	Func<string> textFunc, 
	Func<int> indexFunc, 
	Action<List<string>> setPredictions, 
	TypingState typingState
) => {

	string text = '';
	int index = 0;
	
	int cursorX = 0, cursorY = 0;
	
	string searchFor = '';
	List<string> newList = null;
	List<string> [list, setListState] = useState(null);
	var setList = (List<string> list) => {
		setPredictions(list);
		setListState(list);
	};

	var clear = () => {
		if (list != null) setList(null);
	};

	var tick = () => {
	
		if (newList != null) {
			setList(newList);
			newList = null;
		}
	
		int newIndex = indexFunc();
		if (index != newIndex) {
			index = newIndex;
			clear();
		}
	
		string newText = textFunc();
		if (text != newText) {
			text = newText;
			searchFor = $findSearchFor(text, typingState.cursorIndex);
			(cursorX, cursorY) = $cursorPos(typingState, text);
			Task.Run(() => {
				try {
					$predict(searchFor, text, typingState.cursorIndex).ContinueWith((task) => {
						if (text == newText) newList = task.Result;
					});
				} catch (Exception e) {
					Logger.log(e.StackTrace);
					Logger.log(e.Message);
					clear();
				}
			});			
		}
	};

	return (
		<pseudo onTick={tick}>
			{(list == null || list.Count == 0) ? <p>Nothing Here</p> :
				<div left={cursorX} top={cursorY} class='CodePredictionBox'>
					{list.map(str => {
						int searchIndex = str.IndexOf(searchFor);
						return (
							<span>
								<h6 class='CodePrediction'>{str[..searchIndex]}</h6>
								<h6 class='CodePrediction' color='orange'>{searchFor}</h6>
								<h6 class='CodePrediction'>{str[(searchIndex+searchFor.Length)..]}</h6>
							</span>
						);
					})}
				</div>
			}
		</pseudo>
	);
}

const TextRender = (Func<string> textFunc) => {
	
	string [text, setText] = useState('');

	List<List<(Color, int)>> colorData = null;

	int i = 0;

	List<List<(Color, int)>> FindColorData() {
		i = 0;

		if (colorData != null) {
			int len = colorData.Select(line => line.Select(data => data.Item2).Sum()).Sum();
			if (len <= text.Length) return colorData;
		}

		/*
		if (colorData != null) {
			int len = colorData.Select(data => data.Item2).Sum();
			if (len == text.Length) return colorData;
			
			if (len < text.Length) {
				return colorData.Concat(arr[(Color.White, text.Length - len)]);
			}
		}*/

		
		return null;
	}

	return (
		<pseudo class='ReplaceText' 
			onTick={()=>{
				string newText = textFunc();
				if (text != newText) {
					colorData = null;
					setText(newText);
					Task.Run(()=>{
						try {
							$colorHtml(text).ContinueWith(task => {
								if (newText == text) {
									colorData = task.Result;
									setText(newText);
								}
							});
						} catch (Exception e) {
							Logger.log(e.StackTrace);
							Logger.log(e.Message);
						}
					});
				}
			}}
		>
			{FindColorData()?.map(line => 
				<span>
					{line.map(data => {
						int currI = i;
						var node = <p class='Text' color={data.Item1}>{text.Replace('\n', ' ').Substring(currI, data.Item2)}</p>;
						i += data.Item2;
						return node;
					})}
				</span>
			)}	
		</pseudo>
	);
}

const App = () => {

	List<string> predictions = null;
	var setPredictions = (List<string> list) => predictions = list;

	HtmlNode [node, setNode] = useState(null);

	string text = $'const App = () => {{{'\n'}{'\t'}return ({'\n'}{'\t'}{'\t'}{'\n'}{'\t'});{'\n'}}}' + HtmlComponents.Table;
	Action<string> setText = (string str) => text=str;
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
				-borderWidth={int: 0}
				multiline={true}
				useTypingState={@set(TypingState, typingState)}
				text={string: text} setText={setText}
				diff={(Func<string,string,string>)((string oldStr, string newStr)=>{
					updateCount++;
					return $htmlDiff(oldStr, newStr, typingState, predictions);
				})}
				onTick={()=>{
					if (!updating && currUpdateCount != updateCount) {

						Logger.log(text);
					
						updating = true;
						Task.Run(()=>{
							try {
						    $updateHtml(updateCount, text).ContinueWith(task => {
						    	int thisUpdateCount = task.Result.Item3;
						    	if (thisUpdateCount > currUpdateCount) {
						    		updating = false;
									currUpdateCount = thisUpdateCount;
						    		
									setException(task.Result.Item2);
									setNode(task.Result.Item1);
						    	}
							});
							} catch (Exception e) {Logger.log('????', e.StackTrace);}
						});
						
					}
				}}
				/>
				<h6 color='white'>{currUpdateCount}/{updateCount} {updating ? $loadingText(@t) : ''}</h6>
				<pseudo class='ReplaceText' 
				renderAdd={(SpriteBatch spriteBatch)=>{ 
					$renderTabs(spriteBatch, text, typingState);
				}}
				/>
				<TextRender textFunc={string: correctText()}/>
				<Predictor textFunc={string: text} indexFunc={int: typingState.cursorIndex} 
				setPredictions={setPredictions} typingState={typingState}/>
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

            Func<string, Task<List<List<(Color, int)>>>> colorHtml = async (code) => await Parser.ColorSyntaxHighlightedCSharpHtml(code);
            
            Func<int, string, Task<(HtmlNode, Exception, int)>> updateHtml = async (int updateCount, string text) => {
	            HtmlNode node = null;
	            Exception e = null;
	            
	            try {
		            node = await HtmlProcessor.GenHtml("<App/>", pack, 
			            macros: Macros.create(
			            "div(color, size)", "<div backgroundColor='$$color' dimens={$$size}/>",
			            "none", "<span/>"),
			            components: HtmlComponents.Create(text));
	            } catch (Exception err) {
		            e = err;
		            Logger.log(e.StackTrace);
		            Logger.log(e.Message);
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

            Func<string, string, TypingState, List<string>, string> htmlDiff = (oldStr, newStr, typingState, predictions) => {
	            string str = newStr;

	            static bool valid(char c) {
		            return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9');
	            }

	            if (newStr.Length == oldStr.Length + 1) {

		            char? onChar = null;
		            if (typingState.cursorIndex < newStr.Length) onChar = newStr[typingState.cursorIndex - 1];

		            var closeInto = new[] { ')','}','\''};
		            if (typingState.cursorIndex > 0 && typingState.cursorIndex < newStr.Length && onChar.HasValue && closeInto.Contains(onChar.Value) && newStr[typingState.cursorIndex] == onChar.Value) {
			            str = str[..(typingState.cursorIndex-1)] + str[typingState.cursorIndex..];
		            } else if (typingState.cursorIndex > 0 && newStr[typingState.cursorIndex - 1] == '{') {
			            str = str[..typingState.cursorIndex] + '}' + str[typingState.cursorIndex..];
		            } else if (typingState.cursorIndex > 0 && newStr[typingState.cursorIndex - 1] == '(') {
			            str = str[..typingState.cursorIndex] + ')' + str[typingState.cursorIndex..];
		            } else if (typingState.cursorIndex > 0 && newStr[typingState.cursorIndex - 1] == '\'') {
			            str = str[..typingState.cursorIndex] + '\'' + str[typingState.cursorIndex..];
		            } else if ((predictions != null && predictions.Any()) && ((newStr.CountOf("\t") > oldStr.CountOf("\t")) || (newStr.CountOf("\n") > oldStr.CountOf("\n")))) {
			            // use completion
			            string prediction = predictions[0];
			            for (int i = typingState.cursorIndex - 2; i >= 0; i--) {
				            if (newStr[i].IsValidReferenceNameCharacter()) continue;
				            str = str[..(i + 1)] + prediction + str[typingState.cursorIndex..];
				            typingState.cursorIndex = i + prediction.Length + 1;
				            break;
			            }
		            } else if (typingState.cursorIndex >= 2 && newStr.CountOf("\t") > oldStr.CountOf("\t")) { // creating html open/closed tags on tab
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

            Func<TypingState, string, (int, int)> cursorPos = (typingState, realText) => {
	            Vector2 pos = TextInputUtil.cursorPositionAtIndex(typingState.node, typingState, realText, typingState.cursorIndex);
	            pos.Y += typingState.node.font.FindHeight();
	            return ((int)pos.X, (int)pos.Y);
            };

            Func<string, string, int, Task<List<string>>> predict = async (searchFor, code, index) => await CodePredictor.Predict(searchFor, code, index);
            Func<string, int, string> findSearchFor = (code, index) => CodePredictor.FindSearchFor(code, index);

            pack = StatePack.Create(
                "updateHtml", updateHtml,
                "loadingText", (Func<float, string>) loadingText,
                "htmlDiff", htmlDiff,
                "renderTabs", renderTabs,
                "searchHtml", searchHtml,
                "colorHtml", colorHtml,
                "predict",  predict,
                "cursorPos", cursorPos,
                "findSearchFor", findSearchFor
            );

            gameMain.htmlInstance = await HtmlProcessor.GenerateRunner(html, pack, 
                components: HtmlComponents.Create(componentDefs, HtmlComponents.AllInput, HtmlComponents.FrameCounter));

            HtmlSettings.generateCache = false;
        }
    }
}