using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameHtml;

namespace MonoGameHtmlGeneratedCode {
	public class Cache : StatePack {
	public Cache(params object[] initialVariableNamesAndObjects) : base(initialVariableNamesAndObjects) {}
	/*CACHE_START*/
protected override string[] cachedInput() {
	return new string[]{ @"<App></App>", @"
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
}", @"

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
        	<FrameCounter></FrameCounter>
			
			<SearchBar path={path} setText={setText}></SearchBar>
			
        	<div flex={1} backgroundColor='#34353D'>
				<TextBox 
				class='HtmlBox'
				multiline={true}
				useTypingState={(TypingState ___setTemp)=>typingState=___setTemp}
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
				></TextBox>
				<h6 color='white'>{currUpdateCount}/{updateCount} {updating ? $loadingText(timePassed) : ''}</h6>
				<pseudo 
				renderAdd={(SpriteBatch spriteBatch)=>{ 
					$renderTabs(spriteBatch, text, typingState);
				}}
				></pseudo>
			</div>
			<div flex={1} backgroundColor='white'>
				<html></html>
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
}", @"

const TextInput = (
	Func<string> text, Action<string> setText, Func<bool> active, bool multiline = false, Func<string,string,string> diff,
	Action<TypingState> useTypingState
) => {

	TypingState typingState = new TypingState {
		multiline = multiline,
		diff = diff,
		undoFrequency = 1F,
	};

	useTypingState?.Invoke(typingState);
	
	return (
		<div onTick={()=>{
			bool isActive = (active != null) ? active.Invoke() : true;
			if (isActive) {
				typingState.time = timePassed;
				typingState.deltaTime = deltaTime;
				string currStr = text();
				string newStr = TextInputUtil.getUpdatedText(keys, currStr, typingState);
				if (newStr != currStr) {
					setText(newStr);
				}
			}
		}}></div>
	);
}", @"


const TextBox = (
	Func<string> text, Action<string> setText, Func<string,string,string> diff, bool multiline = false, bool cursorVisible = true,
	Action<TypingState> useTypingState
) => {

	if (text == null && setText == null) {
		string str = textContent ?? '';
		text = () => str;
		setText = newStr => str = newStr;
	}
	
	bool active = false;
	HtmlNode node = null;
	TypingState typingState = null;

	return (
		<div ref={(HtmlNode el)=>{
			node = el;
			typingState.node = el;
		}} class='TextBox' props={props}
			onMouseDown={()=>active=node.clicked}
			-borderWidth={int: (active) ? 1 : 0} -textContent={string: text().Replace('\t', TextInputUtil.spacesPerTab)}
			renderAdd={(SpriteBatch spriteBatch)=>{
				if (!cursorVisible || !active || ((timePassed - typingState.lastEditOrMove > 1) && ((timePassed % 1F) < 0.5F))) return;
				TextInputUtil.drawCursor(spriteBatch, node, typingState, text());
			}}
			onPress={()=>{
				TextInputUtil.setCursorFromPos(mousePos, node, typingState, text());
			}}
			onMouseDrag={()=>{
				TextInputUtil.setCursorFromPos(mousePos, node, typingState, text());
			}}
		>
			<TextInput text={text} setText={setText} diff={diff} active={bool: active} multiline={multiline}
				useTypingState={(TypingState state)=>{
					typingState = state;
					useTypingState?.Invoke(typingState);
				}}
				
			></TextInput>
		</div>
	);
}", @"


const KeyInput = () => {
	
    return (<div></div>);
}", @"
const FrameCounter = (float updateTime = 1F) => {

	var fpsCounter = new FrameCounter();
	int updateCount = 0;

	return (
		<div onTick={()=>{
			fpsCounter.update(deltaTime);
			int currUpdate = (int) (timePassed / updateTime);
			if (currUpdate != updateCount) {
				updateCount = currUpdate;
				game.Window.Title = $'FPS: {(int) fpsCounter.AverageFramesPerSecond}';
			}
		}}></div>
	);
}" };
}

protected override HtmlNode cachedNode() {
	/*IMPORTS_DONE*/

HtmlNode CreateSearchBar(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, Action<string>? setText = null, string path = "") {
	
	HtmlNode ___node = null;
	
List<(string stringName, string contents)> htmlSearchList = ((System.Func<System.String,System.Collections.Generic.List<System.ValueTuple<System.String,System.String>>>)___vars["searchHtml"])(path);
	___node = newNode("div", props: new Dictionary<string, object> {}, childrenFunc: (Func<HtmlNode[]>) (() => nodeArr((true ? null : htmlSearchList.Select(instance =>
				newNode("p", props: new Dictionary<string, object> {["onPress"]=((Action)(()=>setText(instance.contents)))}, textContent: (Func<string>)(()=> ""+(instance.stringName)+""))
			).ToArray()))));
	return ___node;
}

HtmlNode CreateApp(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null) {
	
	HtmlNode ___node = null;
	
HtmlNode node = null;
Action<HtmlNode> setNode = (___val) => {
	node = ___val;
	___node.stateChangeDown();
};

string text = "";
Action<string> setText = (string str)=> text=str;
int updateCount = 0, currUpdateCount = 0;
bool updating = false;
Exception exception = null;
Action<Exception> setException = (___val) => {
	exception = ___val;
	___node.stateChangeDown();
};

TypingState typingState = null;
string path = "/Users/toby/Documents/GitHub/MonoGameHtml/Testing/Source/HtmlWriter/HtmlWriter.cs";
string correctText() {
return text.Replace("\t", TextInputUtil.spacesPerTab);
}
	___node = newNode("body", props: new Dictionary<string, object> {["flexDirection"]="row"}, childrenFunc: (Func<HtmlNode[]>) (() => nodeArr(CreateFrameCounter("FrameCounter", props: new Dictionary<string, object> {}, textContent: ""), CreateSearchBar("SearchBar", props: new Dictionary<string, object> {["path"]=(path), ["setText"]=(setText)}, textContent: "", path: (path), setText: (setText)), newNode("div", props: new Dictionary<string, object> {["flex"]=(1), ["backgroundColor"]="#34353D"}, childrenFunc: (Func<HtmlNode[]>) (() => nodeArr(CreateTextBox("TextBox", props: new Dictionary<string, object> {["class"]="HtmlBox", ["multiline"]=(true), ["useTypingState"]=((Action<TypingState>)((TypingState ___setTemp)=>typingState=___setTemp)), ["text"]=((Func<string>)(() => (text))), ["setText"]=(setText), ["diff"]=((Func<string,string,string>)((string oldStr, string newStr)=>{
					updateCount++;
					return ((System.Func<System.String,System.String,MonoGameHtml.TypingState,System.String>)___vars["htmlDiff"])(oldStr, newStr, typingState);
				})), ["onTick"]=((Action)(()=>{
					if (!updating && currUpdateCount != updateCount) {
					
						updating = true;
						Task.Run(()=>{
						    ((System.Func<System.Int32,System.String,System.Threading.Tasks.Task<System.ValueTuple<MonoGameHtml.HtmlNode,System.Exception,System.Int32>>>)___vars["updateHtml"])(updateCount, text).ContinueWith(task => {
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
				}))}, textContent: "", multiline: (true), useTypingState: ((Action<TypingState>)((TypingState ___setTemp)=>typingState=___setTemp)), text: ((Func<string>)(() => (text))), setText: (setText), diff: ((Func<string,string,string>)((string oldStr, string newStr)=>{
					updateCount++;
					return ((System.Func<System.String,System.String,MonoGameHtml.TypingState,System.String>)___vars["htmlDiff"])(oldStr, newStr, typingState);
				}))), newNode("h6", props: new Dictionary<string, object> {["color"]="white"}, textContent: (Func<string>)(()=> ""+(currUpdateCount)+"/"+(updateCount)+" "+(updating ? ((System.Func<System.Single,System.String>)___vars["loadingText"])(timePassed) : "")+"")), newNode("pseudo", props: new Dictionary<string, object> {["renderAdd"]=((Action<SpriteBatch>)((SpriteBatch spriteBatch)=>{ 
					((System.Action<Microsoft.Xna.Framework.Graphics.SpriteBatch,System.String,MonoGameHtml.TypingState>)___vars["renderTabs"])(spriteBatch, text, typingState);
				}))}, textContent: "")))), newNode("div", props: new Dictionary<string, object> {["flex"]=(1), ["backgroundColor"]="white"}, childrenFunc: (Func<HtmlNode[]>) (() => nodeArr(newNode("html", props: new Dictionary<string, object> {}, textContent: ""), (node ?? 
					(
						(exception == null || text == "") ? 
							newNode("p", props: new Dictionary<string, object> {}, textContent: "Nothing to display...") : 
							newNode("p", props: new Dictionary<string, object> {["color"]="red"}, textContent: (Func<string>)(()=> ""+(exception == null ? "NULL?" : (exception.GetType().Name + "\n" + exception.Message))+""))
					)
				)))))));
	return ___node;
}

HtmlNode CreateTextInput(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, Func<string>? text = null, Action<string>? setText = null, Func<bool>? active = null, bool multiline = false, Func<string,string,string>? diff = null, Action<TypingState>? useTypingState = null) {
	
	HtmlNode ___node = null;
	
TypingState typingState = new TypingState {
multiline = multiline,
diff = diff,
undoFrequency = 1F,
};
useTypingState?.Invoke(typingState);
	___node = newNode("div", props: new Dictionary<string, object> {["onTick"]=((Action)(()=>{
			bool isActive = (active != null) ? active.Invoke() : true;
			if (isActive) {
				typingState.time = timePassed;
				typingState.deltaTime = deltaTime;
				string currStr = text();
				string newStr = TextInputUtil.getUpdatedText(keys, currStr, typingState);
				if (newStr != currStr) {
					setText(newStr);
				}
			}
		}))}, textContent: "");
	return ___node;
}

HtmlNode CreateTextBox(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, Func<string>? text = null, Action<string>? setText = null, Func<string,string,string>? diff = null, bool multiline = false, bool cursorVisible = true, Action<TypingState>? useTypingState = null) {
	
	HtmlNode ___node = null;
	
if (text == null && setText == null) {
string str = textContent ?? "";
text = () => str;
setText = newStr => str = newStr;
}
bool active = false;
HtmlNode node = null;
TypingState typingState = null;
	___node = newNode("div", props: new Dictionary<string, object> {["ref"]=((Action<HtmlNode>)((HtmlNode el)=>{
			node = el;
			typingState.node = el;
		})), ["class"]="TextBox", ["props"]=(props), ["onMouseDown"]=((Action)(()=>active=node.clicked)), ["-borderWidth"]=((Func<int>)(() => ((active) ? 1 : 0))), ["-textContent"]=((Func<string>)(() => (text().Replace("\t", TextInputUtil.spacesPerTab)))), ["renderAdd"]=((Action<SpriteBatch>)((SpriteBatch spriteBatch)=>{
				if (!cursorVisible || !active || ((timePassed - typingState.lastEditOrMove > 1) && ((timePassed % 1F) < 0.5F))) return;
				TextInputUtil.drawCursor(spriteBatch, node, typingState, text());
			})), ["onPress"]=((Action)(()=>{
				TextInputUtil.setCursorFromPos(mousePos, node, typingState, text());
			})), ["onMouseDrag"]=((Action)(()=>{
				TextInputUtil.setCursorFromPos(mousePos, node, typingState, text());
			}))}, childrenFunc: (Func<HtmlNode[]>) (() => nodeArr(CreateTextInput("TextInput", props: new Dictionary<string, object> {["text"]=(text), ["setText"]=(setText), ["diff"]=(diff), ["active"]=((Func<bool>)(() => (active))), ["multiline"]=(multiline), ["useTypingState"]=((Action<TypingState>)((TypingState state)=>{
					typingState = state;
					useTypingState?.Invoke(typingState);
				}))}, textContent: "", text: (text), setText: (setText), diff: (diff), active: ((Func<bool>)(() => (active))), multiline: (multiline), useTypingState: ((Action<TypingState>)((TypingState state)=>{
					typingState = state;
					useTypingState?.Invoke(typingState);
				}))))));
	return ___node;
}

HtmlNode CreateKeyInput(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null) {
	
	HtmlNode ___node = null;
	
	___node = newNode("div", props: new Dictionary<string, object> {}, textContent: "");
	return ___node;
}

HtmlNode CreateFrameCounter(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, float updateTime = 1F) {
	
	HtmlNode ___node = null;
	
var fpsCounter = new FrameCounter();
int updateCount = 0;
	___node = newNode("div", props: new Dictionary<string, object> {["onTick"]=((Action)(()=>{
			fpsCounter.update(deltaTime);
			int currUpdate = (int) (timePassed / updateTime);
			if (currUpdate != updateCount) {
				updateCount = currUpdate;
				game.Window.Title = $"FPS: {(int) fpsCounter.AverageFramesPerSecond}";
			}
		}))}, textContent: "");
	return ___node;
}
HtmlNode node = CreateApp("App", props: new Dictionary<string, object> {}, textContent: "");
setupNode(node);
return node;
}/*CACHE_END*/
	}
}