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

	        const string html = "<App/>";
            StatePack statePack = null;

            Func<string, Task<List<List<(Color, int)>>>> colorHtml = async (code) => await Parser.ColorSyntaxHighlightedCSharpHtml(code);
            
            Func<int, string, Task<(HtmlNode, Exception, int)>> updateHtml = async (updateCount, text) => {
	            HtmlNode node = null;
	            Exception e = null;
	            
	            try {
		            node = await HtmlProcessor.GenHtml("<App/>", statePack,
			            macros: Macros.create(
				            "div(color, size)", "<div backgroundColor='$$color' dimens={$$size}/>",
				            "none", "<span/>"),
			            components: HtmlComponents.Create(text), 
			            intermediateUser: new HtmlIntermediateUser {
				            useCS = (code) => {
					            statePack.UpdateVar("code", code);
				            }
			            });
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

			            bool autoParens =    tag == "If"
			                              || tag == "Elif"
			                              || tag == "Else"
			                              || tag == "Try"
			                              || tag == "Catch"
			                              || tag == "Switch"
			                              || tag == "Case"
			                              || tag == "Default";

			            str = newStr[..tagStart] + $"<{tag}{(autoParens ? $" ()" : "")}></{tag}>" + newStr[typingState.cursorIndex..];
			            typingState.cursorIndex = tagStart + tag.Length + 2 + (autoParens ? 1 : 0);

		            } else if (newStr.CountOf("\n") > oldStr.CountOf("\n")) { // maintain tabs between lines
			            if (newStr[..(typingState.cursorIndex - 1)].EndsWith("cmp")) {
				            str = newStr[..(typingState.cursorIndex - 4)] + @"const  = () => {
	return (
		
	);
}" + newStr[typingState.cursorIndex..];

				            typingState.cursorIndex += 2;
			            } else { 
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

            Func<string[]> getMonoHtmlFilePaths = () => {
	            try {
		            return HtmlComponents.AllScriptFilePaths(GameMain.scriptPath);
	            }
	            catch (Exception e) {
		            Logger.log("TEST FAILED");
		            Logger.log(e, e.Message, e.StackTrace);
		            return new string[]{};
	            }
            };

            statePack = StatePack.Create(
                "updateHtml", updateHtml,
                "loadingText", (Func<float, string>) loadingText,
                "htmlDiff", htmlDiff,
                "renderTabs", renderTabs,
                "searchHtml", searchHtml,
                "colorHtml", colorHtml,
                "predict",  predict,
                "cursorPos", cursorPos,
                "findSearchFor", (Func<string, int, string>) CodePredictor.FindSearchFor,
                "getMonoHtmlFilePaths", getMonoHtmlFilePaths,
                "scriptPath", GameMain.scriptPath,
                "code", ""
            );

            gameMain.htmlInstance = await HtmlLiveEdit.Create(async () => {
	            
	            CSSHandler.SetCSS(Path.Join(GameMain.cssPath, "Styles.css"),
		            Path.Join(GameMain.cssPath, "IDE.css"));

	            return await HtmlProcessor.GenerateRunner(html,
		            statePack: statePack,
		            assemblies: new[] {typeof(HtmlWriter).Assembly},
		            imports: new[] {"System.Threading.Tasks", "System.IO"},
		            components: HtmlComponents.Create(
			            HtmlComponents.ReadFrom(Path.Join(GameMain.scriptPath, "HtmlWriter")),
			            HtmlComponents.AllInput, HtmlComponents.FrameCounter, HtmlComponents.AllControlFlow));
            }, GameMain.assetPath);
            
            /*await HtmlLiveEdit.Create(async () => await HtmlProcessor.GenerateRunner(html,
	            statePack: statePack,
	            assemblies: new[] {typeof(HtmlWriter).Assembly},
	            imports: new[] {"System.Threading.Tasks", "System.IO"},
	            components: HtmlComponents.Create(HtmlComponents.ReadFrom(Path.Join(GameMain.scriptPath, "HtmlWriter")),
		            HtmlComponents.AllInput, HtmlComponents.FrameCounter, HtmlComponents.AllControlFlow)));*/

            HtmlSettings.generateCache = false;
        }
    }
}