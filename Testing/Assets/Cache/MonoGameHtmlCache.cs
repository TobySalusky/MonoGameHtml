using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGameHtml;

namespace MonoGameHtmlGeneratedCode {
	public class Cache : StatePack {
	public Cache(params object[] initialVariableNamesAndObjects) : base(initialVariableNamesAndObjects) {}
	/*CACHE_START*/
protected override string[] cachedInput() {
	return new string[]{ @"<App/>", @"
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
}", @"

const TextInput = (
	Func<string> text, Action<string> setText, Func<bool> active, bool multiline = false, Func<string,string,string> diff
) => {

	TypingState typingState = new TypingState {
		multiline = multiline,
		diff = diff,
		undoFrequency = 1F
	};
	
	return (
		<div onTick={()=^{
			bool isActive = (active != null) ? active.Invoke() : true;
			if (isActive) {
				typingState.time = @t;
				typingState.deltaTime = @dt;
				string currStr = text();
				string newStr = TextInputUtil.getUpdatedText(keys, currStr, typingState);
				if (newStr != currStr) {
					setText(newStr);
				}
			}
		}}/>
	);
}", @"


const TextBox = (
	Func<string> text, Action<string> setText, Func<string,string,string> diff, bool multiline = false
) => {

	if (text == null && setText == null) {
		string str = textContent ?? '';
		text = () => str;
		setText = newStr => str = newStr;
	}
	
	bool active = false;
	HtmlNode node = null;

	return (
		<div ref={@setRef(node)} class='TextBox' props={props}
			onMouseDown={()=^active=node.clicked}
			-borderWidth={int: (active) ? 1 : 0} -textContent={string: text().Replace('\t', '   ')}
		>
			<TextInput text={text} setText={setText} diff={diff} active={bool: active} multiline={multiline}/>
		</div>
	);
}", @"


const KeyInput = () => {
	
    return (<div/>);
}", @"
const FrameCounter = (float updateTime = 1F) => {

	var fpsCounter = new FrameCounter();
	int updateCount = 0;

	return (
		<div onTick={()=^{
			fpsCounter.update(@dt);
			int currUpdate = (int) (@t / updateTime);
			if (currUpdate != updateCount) {
				updateCount = currUpdate;
				game.Window.Title = $'FPS: {(int) fpsCounter.AverageFramesPerSecond}';
			}
		}}/>
	);
}", @"time", @"t", @"deltaTime", @"dt", @"mp", @"r", @"setRef(varName)", @"timePassed", @"timePassed", @"deltaTime", @"deltaTime", @"mousePos", @"random()", @"(HtmlNode ___refNode)=^$$varName=___refNode" };
}

protected override HtmlNode cachedNode() {
	/*IMPORTS_DONE*/

HtmlNode CreateApp(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null) {
	
	HtmlNode ___node = null;
	
HtmlNode node = null;
Action<HtmlNode> setNode = (___val) => {
	node = ___val;
	___node.stateChangeDown();
};

string text = "";
int updateCount = 0, currUpdateCount = 0;
bool updating = false;
Exception exception = null;
Action<Exception> setException = (___val) => {
	exception = ___val;
	___node.stateChangeDown();
};

	___node = newNode("body", props: new Dictionary<string, object> {["flexDirection"]="row"}, children: nodeArr(CreateFrameCounter("FrameCounter", textContent: ""), newNode("div", props: new Dictionary<string, object> {["flex"]=(1), ["backgroundColor"]="gray"}, children: nodeArr(CreateTextBox("TextBox", props: new Dictionary<string, object> {["multiline"]=(true), ["width"]="49%", ["height"]="90%", ["text"]=((Func<string>)(() => (text))), ["setText"]=((Action<string>)((string str)=>text=str)), ["diff"]=((Func<string,string,string>)((string oldStr, string newStr)=>{
					updateCount++;
					return newStr;
				})), ["onTick"]=((Action)(()=>{
					if (!updating && currUpdateCount != updateCount) {
					
						updating = true;
						Task.Run(()=>{
						    ((System.Func<System.String,System.Threading.Tasks.Task<System.ValueTuple<MonoGameHtml.HtmlNode,System.Exception>>>)___vars["updateHtml"])(text).ContinueWith(task => {
						    	updating = false;
								currUpdateCount = updateCount;
						    	
								setNode(null); //TODO: make it so this is not required!!!
								setException(task.Result.Item2);
								setNode(task.Result.Item1);
							});
						});
						
					}
				})), ["color"]="red", ["fontSize"]=(30)}, textContent: "", multiline: (true), text: ((Func<string>)(() => (text))), setText: ((Action<string>)((string str)=>text=str)), diff: ((Func<string,string,string>)((string oldStr, string newStr)=>{
					updateCount++;
					return newStr;
				}))), newNode("p", textContent: (Func<string>)(()=> ""+(updating ? ((System.Func<System.Single,System.String>)___vars["loadingText"])(timePassed) : "")+"")))), newNode("div", props: new Dictionary<string, object> {["flex"]=(1), ["backgroundColor"]="white"}, childrenFunc: (Func<HtmlNode[]>) (() => nodeArr(newNode("html", textContent: ""), (node ?? 
					(
						(exception == null || text == "") ? 
							newNode("p", textContent: "Nothing to display...") : 
							newNode("p", props: new Dictionary<string, object> {["color"]="red"}, textContent: (Func<string>)(()=> ""+(exception.GetType().Name + "\n" + exception.Message)+""))
					)
				))))));
	return ___node;
}

HtmlNode CreateTextInput(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, Func<string>? text = null, Action<string>? setText = null, Func<bool>? active = null, bool multiline = false, Func<string,string,string>? diff = null) {
	
	HtmlNode ___node = null;
	
TypingState typingState = new TypingState {
multiline = multiline,
diff = diff,
undoFrequency = 1F
};
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

HtmlNode CreateTextBox(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, Func<string>? text = null, Action<string>? setText = null, Func<string,string,string>? diff = null, bool multiline = false) {
	
	HtmlNode ___node = null;
	
if (text == null && setText == null) {
string str = textContent ?? "";
text = () => str;
setText = newStr => str = newStr;
}
bool active = false;
HtmlNode node = null;
	___node = newNode("div", props: new Dictionary<string, object> {["ref"]=((Action<HtmlNode>)((HtmlNode ___refNode)=>node=___refNode)), ["class"]="TextBox", ["props"]=(props), ["onMouseDown"]=((Action)(()=>active=node.clicked)), ["-borderWidth"]=((Func<int>)(() => ((active) ? 1 : 0))), ["-textContent"]=((Func<string>)(() => (text().Replace("\t", "   "))))}, children: nodeArr(CreateTextInput("TextInput", props: new Dictionary<string, object> {["text"]=(text), ["setText"]=(setText), ["diff"]=(diff), ["active"]=((Func<bool>)(() => (active))), ["multiline"]=(multiline)}, textContent: "", text: (text), setText: (setText), diff: (diff), active: ((Func<bool>)(() => (active))), multiline: (multiline))));
	return ___node;
}

HtmlNode CreateKeyInput(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null) {
	
	HtmlNode ___node = null;
	
	___node = newNode("div", textContent: "");
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
HtmlNode node = CreateApp("App", textContent: "");
setupNode(node);
return node;
}/*CACHE_END*/
	}
}