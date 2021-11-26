

using System;
using System.Collections.Generic;
using System.Linq;
using MonoGameHtml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System.Threading.Tasks;
using System.IO;
using Testing;

namespace MonoGameHtmlGeneratedCode {
	public class Cache : StatePack {
	public Cache(params object[] initialVariableNamesAndObjects) : base(initialVariableNamesAndObjects) {}
		protected override string[] cachedInput() {
			return new string[]{ @"<App></App>", @"const App = () => {
		
	int [screen, setScreen] = useState(0);
	string activeFilePath = null;
	
	var useFileAtPath = (string path) => {
		activeFilePath = path;
		setScreen(0);
	};
	
	return (
		<body>
			{<dynamic></dynamic>} 
			<If (screen == 0) dimens='100%'>
				<MainScreen openFileScreen={()=>setScreen(1)} activeFilePath={activeFilePath}></MainScreen>
			</If>
			<Else dimens='100%' position='fixed' top={0} left={0}>
				<FileScreen useFileAtPath={useFileAtPath}></FileScreen>
			</Else>
		</body>
	);
}", @"

const FileTab = (string path) => {
	return (
		<div props={props} class='FileTab'>
			<h3>{path.Substring(path.LastIndexOf(@'\') + 1)}</h3>
			<div class='Divide'></div>
			<p>{path}</p>
		</div>
	);
}", @"

const FileScreen = (Action<string> useFileAtPath) => {

	string[] paths = $getMonoHtmlFilePaths();

	return (
		<body props={props} class='FileTabContainer'>
			{paths.map(path =>
				<FileTab onPress={()=>useFileAtPath(path)} path={path}></FileTab>
			)}
		</body>
	);
}", @"
const SaveBox = (Func<bool> open, Action<string,string> save, Func<string> contents, Func<string> activePath, Action close) => {

	bool [saveBoxOpen, setSaveBoxOpen] = useState(false);
	string text = '';

	var submit = () => {
		Logger.log('submitted', text);
		save(text.EndsWith('.monohtml') ? text : text + '.monohtml', contents());
		close();
	};

	return (
		<div class='SaveBoxContainer' onTick={()=>{
				if (open() != saveBoxOpen) {
					setSaveBoxOpen(!saveBoxOpen);
					if (activePath() != null) {
						text = activePath();
					} else {
						text = Path.Join($scriptPath, 'untitled');
					}
				}
			}}>
			{!saveBoxOpen ? null :
				<div class='SaveBox'>
					<TextBox class='SaveTextBox' onEnter={submit} label='absolute path:' text={string: text} setText={(string str)=> text=str}></TextBox>
					<div onPress={submit} class='SaveSubmit'>
						Save...
					</div>
				</div>
			}
		</div>
	);
}", @"

const MainScreen = (Action openFileScreen, string activeFilePath) => {

	var trySaveFile = (string path, string contents) => {
		try {
			Logger.log('successfully saved to:', path);
			File.WriteAllText(path, contents);
			activeFilePath = path;
		} catch (Exception e) {
			Logger.log('failed to save file!', e.StackTrace, e);
		}
	};
	
	bool [showCode, setShowCode] = useState(false);
	string [code, setCode] = useState('');
	
	List<string> predictions = null;
	var setPredictions = (List<string> list) => predictions = list;

	HtmlNode [node, setNode] = useState(null);
	bool saveBoxOpen = false;

	string text = $'const App = () => {{{'\n'}{'\t'}return ({'\n'}{'\t'}{'\t'}{'\n'}{'\t'});{'\n'}}}';
	
	if (activeFilePath != null) {
		try {
			text = File.ReadAllText(activeFilePath);
		} catch (Exception) {
			Logger.log('IDE FAILED TO READ FROM FILE PATH');
		}
	}
	
	Action<string> setText = (string str) => text=str;
	int updateCount = 1, currUpdateCount = 0;
	bool updating = false;
	Exception [exception, setException] = useState(null);
	
	TypingState typingState = null;
	
	string correctText() {
		return text.Replace('\t', TextInputUtil.spacesPerTab);
	}

    return (
        <body flexDirection='row' props={props}>
        	
        	<FrameCounter></FrameCounter>
			
        	<div flex={1} backgroundColor='#34353D'>
				<TextBox 
				class='HtmlBox'
				selectionColor={new Color(1F, 1F, 1F, 0.2F)} 
				-borderWidth={int: 0}
				multiline={true}
				useTypingState={(TypingState ___setTemp)=>typingState=___setTemp}
				text={string: text} setText={setText}
				diff={(Func<string,string,string>)((string oldStr, string newStr)=>{
					updateCount++;
					return $htmlDiff(oldStr, newStr, typingState, predictions);
				})}
				onTick={()=>{
					if (code != $code) setCode($code);
					
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
				></TextBox>
				<h6 color='white'>{currUpdateCount}/{updateCount} {updating ? $loadingText(timePassed) : ''}</h6>
				<pseudo class='ReplaceText' 
				renderAdd={(SpriteBatch spriteBatch)=>{ 
					$renderTabs(spriteBatch, text, typingState);
				}}
				></pseudo>
				<TextRender textFunc={string: correctText()}></TextRender>
				<Predictor textFunc={string: text} indexFunc={int: typingState.cursorIndex} 
				setPredictions={setPredictions} typingState={typingState}></Predictor>
				
				<span>
					<div class='FileOptionButton' onPress={()=>setShowCode(!showCode)}>
						view code
					</div>
					<div class='FileOptionButton' onPress={openFileScreen}>
						files...
					</div>
					<div class='FileOptionButton' onPress={()=>{
							if (activeFilePath != null) {
								trySaveFile(activeFilePath, text);
							} else {
								saveBoxOpen = true;
							}
						}}>
						save...
					</div>
					<div class='FileOptionButton' onPress={()=>saveBoxOpen = true}>
						save as...
					</div>
				</span>
			</div>
			<div flex={1} backgroundColor='white'>
				<html></html>
				{showCode ? <p>{code}</p> : (node ??  
					(
						(exception == null || text == '') ? 
							<p>Nothing to display...</p> : 
							<p color='red'>{exception == null ? 'NULL?' : (exception.GetType().Name + '\n' + exception.Message)}</p>
					)
				)}
			</div>
			<SaveBox open={bool: saveBoxOpen} close={()=>saveBoxOpen=false} save={trySaveFile} contents={string: text} activePath={string: activeFilePath}></SaveBox>
		</body>
    );
}", @"

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
					//Logger.log(e.StackTrace);
					//Logger.log(e.Message);
					clear();
				}
			});			
		}
	};

	return (
		<pseudo onTick={tick}>
			{(list == null || list.Count == 0) ? <p></p> :
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
}", @"
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
				return colorData.Concat(new []{(Color.White, text.Length - len)});
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
							//Logger.log(e.StackTrace);
							//Logger.log(e.Message);
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
	Action<TypingState> useTypingState, Action onEnter, string label, bool scrollable = false, Color selectionColor: new Color(0F, 0F, 1F, 0.3F)
) => {

	if (text == null && setText == null) {
		string str = textContent ?? '';
		text = () => str;
		setText = newStr => str = newStr;
	}
	
	bool active = false;
	HtmlNode node = null;
	TypingState typingState = null;

	var trueDiff = (string oldStr, string newStr):string => {
		if (oldStr.Length + 1 == newStr.Length && oldStr.CountOf('\n') < newStr.CountOf('\n')) onEnter.Invoke();
		return diff == null ? newStr : diff(oldStr, newStr);
	};
	if (onEnter == null) trueDiff = diff;

	return (
		<div ref={(HtmlNode el)=>{
			node = el;
			typingState.node = el;
		}} class='TextBox' props={props}
			onMouseDown={()=>active=node.clicked}
			-borderWidth={int: (active) ? 1 : 0} -textContent={string: ((label != null && text().Length == 0) ? label : text()).Replace('\t', TextInputUtil.spacesPerTab)}
			renderAdd={(SpriteBatch spriteBatch)=>{
				if (typingState.HasRealSelection()) TextInputUtil.drawSelection(spriteBatch, node, typingState, text(), selectionColor);
				if (!cursorVisible || !active || ((timePassed - typingState.lastEditOrMove > 1) && ((timePassed % 1F) < 0.5F))) return;
				TextInputUtil.drawCursor(spriteBatch, node, typingState, text());
			}}
			onPress={()=>{
				TextInputUtil.Click(mousePos, node, typingState, text());
			}}
			onMouseDrag={()=>{
				if (!active) return;
				TextInputUtil.setCursorFromPos(mousePos, node, typingState, text());
			}}
		>
			<TextInput text={text} setText={setText} diff={trueDiff} active={bool: active} multiline={multiline}
				useTypingState={(TypingState state)=>{
					typingState = state;
					useTypingState?.Invoke(typingState);
				}}
			></TextInput>
		</div>
	);
}", @"


const KeyInput = () => {
    return (<todo></todo>);
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
}", @"

const If = (bool __parens__ = false) => {

    HtmlNode[] nodeArray = null;
    if (__parens__) {
        nodeArray = (childrenFunc != null ? childrenFunc() : children);
    }
    
	return (
	    <if propsUnder={props} ref={(HtmlNode node) => {
	        if (__parens__) return;
	        HtmlNode next = node.GetNext();
	        if (next != null && (next.tag == 'else' || next.tag == 'elif')) {
	            next.prop<Action>('trigger')();
	        }
	    }}>
	        <html></html>
	        {nodeArray}
        </if>
	);
}", @"


const Elif = (bool __parens__ = false) => {
    
    HtmlNode[] [nodeArray, setNodeArray] = useState(null);
    
    var onTrigger = () => {
        if (__parens__) {
	        setNodeArray((childrenFunc != null ? childrenFunc() : children));
	    } else {
	        HtmlNode next = ___node.GetNext();
	        if (next != null && (next.tag == 'else' || next.tag == 'elif')) {
	            next.prop<Action>('trigger')();
	        }
	    }
    };
    
	return (
	    <elif propsUnder={props} trigger={onTrigger}>
	        <html></html>
	        {nodeArray}
        </elif>
	);
}", @"


const Else = () => {
    
    HtmlNode[] [nodeArray, setNodeArray] = useState(null);
    
	return (
		<else propsUnder={props} trigger={()=>setNodeArray((childrenFunc != null ? childrenFunc() : children))}>
			<html></html>
		    {nodeArray}
        </else>
	);
}", @"


const Try = () => { // TODO: add ability to force dynamic children!!!

    HtmlNode[] nodeArray = null;
	Exception exception = null;
    try {
        nodeArray = childrenFunc();
    } catch (Exception e) {
		exception = e;
	}
    
	return (
	    <try propsUnder={props} ref={(HtmlNode node) => {
	        if (exception == null) return;
	        HtmlNode next = node.GetNext();
	        if (next != null && next.tag == 'catch') {
	            next.prop<Action<Exception>>('trigger')(exception);
	        }
	    }}>
	        <html></html>
	        {nodeArray}
        </try>
	);
}", @"


const Catch = (Action<Exception> __parens__) => {
    
    HtmlNode[] [nodeArray, setNodeArray] = useState(null);
    
	return (
		<catch propsUnder={props} trigger={(Exception exception)=>{
			setNodeArray((childrenFunc != null ? childrenFunc() : children));
			__parens__?.Invoke(exception);
		}}>
			<html></html>
		    {nodeArray}
        </catch>
	);
}", @"


const Switch = (object __parens__) => {

	if (__parens__ == null) throw new Exception('Switch-value may not be null');

	string [currCase, setCurrCase] = useState('');

	Action onTick = null;
	string init = null;
	if (__parens__ is Func<string> func) {
		Action action = () => {
			string newCase = func();
			if (newCase != currCase) {
				setCurrCase(newCase);
			}
		};
		onTick = action;
		init = func();
	} else {
		init = __parens__.ToString();
	}
	setCurrCase(init);

	Dictionary<string, HtmlNode> nodeDict = new Dictionary<string, HtmlNode>();
	HtmlNode def = null;
	
	var extractContent = (HtmlNode node): HtmlNode => {
		HtmlNode[] nodeContents = node.Contents;
		if (nodeContents.Length == 1) return nodeContents[0];
		return (
			<fragment>
				<html></html>{nodeContents}
			</fragment>
		);
	};
	
	HtmlNode[] contents = (childrenFunc != null ? childrenFunc() : children);

	if (contents != null && contents.Length > 0) {
		foreach (HtmlNode node in contents) {
			if (node.props.ContainsKey('default') && node.prop<bool>('default') == true) {
				def = (node.tag == 'default') ? extractContent(node) : node;
				continue;
			}
			if (!node.props.ContainsKey('case')) continue;
			string thisCase = node.prop<string>('case');
			nodeDict[thisCase] = (node.tag == 'case') ? extractContent(node) : node;
		};
	}

	return (nodeDict.ContainsKey(currCase) ? nodeDict[currCase] : def);
}", @"


const Case = (object __parens__) => {
	
	if (__parens__ == null) throw new Exception('Case-value may not be null');

	return (
		<case case={__parens__.ToString()} propsUnder={props}>
			{((childrenFunc != null ? childrenFunc() : children) ?? (new HtmlNode[]{<p>{textContent}</p>}))}
		</case>
	);
}", @"


const Default = () => {
	return (
		<case default={true} propsUnder={props}>
			{((childrenFunc != null ? childrenFunc() : children) ?? (new HtmlNode[]{<p>{textContent}</p>}))}
		</case>
	);
}" };
		}

		protected override HtmlNode cachedNode() {
			/*IMPORTS_DONE*/
HtmlNode CreateApp(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, Func<HtmlNode[]> childrenFunc = null) {
	
	HtmlNode ___node = null;
		

	
int screen = 0;
Action<int> setScreen = (___val) => {
	screen = ___val;
	___node?.stateChangeDown();
};

string activeFilePath = null;
var useFileAtPath = (Action<string>)((path)=>{
		activeFilePath = path;
		setScreen(0);
	});
;
	___node = newNode("body", props: new Dictionary<string, object> {}, childrenFunc: (Func<HtmlNode[]>) (() => nodeArr((newNode("dynamic", props: new Dictionary<string, object> {}, textContent: "")), CreateIf("If", props: new Dictionary<string, object> {["__parens__"]=screen == 0, ["dimens"]="100%"}, children: nodeArr(CreateMainScreen("MainScreen", props: new Dictionary<string, object> {["openFileScreen"]=(Action)(()=>setScreen(1)), ["activeFilePath"]=activeFilePath}, children: null, childrenFunc: null, textContent: "", openFileScreen: (Action)(()=>setScreen(1)), activeFilePath: activeFilePath)), childrenFunc: null, textContent: null, __parens__: screen == 0), CreateElse("Else", props: new Dictionary<string, object> {["dimens"]="100%", ["position"]="fixed", ["top"]=0, ["left"]=0}, children: nodeArr(CreateFileScreen("FileScreen", props: new Dictionary<string, object> {["useFileAtPath"]=useFileAtPath}, children: null, childrenFunc: null, textContent: "", useFileAtPath: useFileAtPath)), childrenFunc: null, textContent: null))));
	
	return ___node;
}

HtmlNode CreateFileTab(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, Func<HtmlNode[]> childrenFunc = null, string? path = null) {
	
	HtmlNode ___node = null;
		

	
;
	___node = newNode("div", props: new Dictionary<string, object> {["props"]=props, ["class"]="FileTab"}, children: nodeArr(newNode("h3", props: new Dictionary<string, object> {}, textContent: (Func<string>)(()=> ""+(path.Substring(path.LastIndexOf(@"\") + 1))+"")), newNode("div", props: new Dictionary<string, object> {["class"]="Divide"}, textContent: ""), newNode("p", props: new Dictionary<string, object> {}, textContent: (Func<string>)(()=> ""+(path)+""))));
	
	return ___node;
}

HtmlNode CreateFileScreen(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, Func<HtmlNode[]> childrenFunc = null, Action<string>? useFileAtPath = null) {
	
	HtmlNode ___node = null;
		

	
string[] paths = ((System.Func<System.String[]>)___vars["getMonoHtmlFilePaths"])();
;
	___node = newNode("body", props: new Dictionary<string, object> {["props"]=props, ["class"]="FileTabContainer"}, childrenFunc: (Func<HtmlNode[]>) (() => nodeArr((paths.Select(path =>
				CreateFileTab("FileTab", props: new Dictionary<string, object> {["onPress"]=(Action)(()=>useFileAtPath(path)), ["path"]=path}, children: null, childrenFunc: null, textContent: "", path: path)
			).ToArray()))));
	
	return ___node;
}

HtmlNode CreateSaveBox(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, Func<HtmlNode[]> childrenFunc = null, Func<bool>? open = null, Action<string,string>? save = null, Func<string>? contents = null, Func<string>? activePath = null, Action? close = null) {
	
	HtmlNode ___node = null;
		

	
bool saveBoxOpen = false;
Action<bool> setSaveBoxOpen = (___val) => {
	saveBoxOpen = ___val;
	___node?.stateChangeDown();
};

string text = "";
var submit = (Action)(()=>{
		Logger.log("submitted", text);
		save(text.EndsWith(".monohtml") ? text : text + ".monohtml", contents());
		close();
	});
;
	___node = newNode("div", props: new Dictionary<string, object> {["class"]="SaveBoxContainer", ["onTick"]=(Action)(()=>{
				if (open() != saveBoxOpen) {
					setSaveBoxOpen(!saveBoxOpen);
					if (activePath() != null) {
						text = activePath();
					} else {
						text = Path.Join(((System.String)___vars["scriptPath"]), "untitled");
					}
				}
			})}, childrenFunc: (Func<HtmlNode[]>) (() => nodeArr((!saveBoxOpen ? null :
				newNode("div", props: new Dictionary<string, object> {["class"]="SaveBox"}, children: nodeArr(CreateTextBox("TextBox", props: new Dictionary<string, object> {["class"]="SaveTextBox", ["onEnter"]=submit, ["label"]="absolute path:", ["text"]=(Func<string>)(() => text), ["setText"]=(Action<string>)((string str)=> text=str)}, children: null, childrenFunc: null, textContent: "", onEnter: submit, label: "absolute path:", text: (Func<string>)(() => text), setText: (Action<string>)((string str)=> text=str)), newNode("div", props: new Dictionary<string, object> {["onPress"]=submit, ["class"]="SaveSubmit"}, textContent: "Save...")))
			))));
	
	return ___node;
}

HtmlNode CreateMainScreen(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, Func<HtmlNode[]> childrenFunc = null, Action? openFileScreen = null, string? activeFilePath = null) {
	
	HtmlNode ___node = null;
		

	
var trySaveFile = (Action<string, string>)((path, contents)=>{
		try {
			Logger.log("successfully saved to:", path);
			File.WriteAllText(path, contents);
			activeFilePath = path;
		} catch (Exception e) {
			Logger.log("failed to save file!", e.StackTrace, e);
		}
	});
bool showCode = false;
Action<bool> setShowCode = (___val) => {
	showCode = ___val;
	___node?.stateChangeDown();
};

string code = "";
Action<string> setCode = (___val) => {
	code = ___val;
	___node?.stateChangeDown();
};

List<string> predictions = null;
var setPredictions = (Action<List<string>>)((list)=>predictions = list);
HtmlNode node = null;
Action<HtmlNode> setNode = (___val) => {
	node = ___val;
	___node?.stateChangeDown();
};

bool saveBoxOpen = false;
string text = $"const App = () => {{{"\n"}{"\t"}return ({"\n"}{"\t"}{"\t"}{"\n"}{"\t"});{"\n"}}}";
if (activeFilePath != null) {
		try {
			text = File.ReadAllText(activeFilePath);
		} catch (Exception) {
			Logger.log("IDE FAILED TO READ FROM FILE PATH");
		}
	}
	
	Action<string> setText = (string str) => text=str;
int updateCount = 1, currUpdateCount = 0;
bool updating = false;
Exception exception = null;
Action<Exception> setException = (___val) => {
	exception = ___val;
	___node?.stateChangeDown();
};

TypingState typingState = null;
string correctText() {
		return text.Replace("\t", TextInputUtil.spacesPerTab);
	}

    ;
	___node = newNode("body", props: new Dictionary<string, object> {["flexDirection"]="row", ["props"]=props}, children: nodeArr(CreateFrameCounter("FrameCounter", props: new Dictionary<string, object> {}, children: null, childrenFunc: null, textContent: ""), newNode("div", props: new Dictionary<string, object> {["flex"]=1, ["backgroundColor"]="#34353D"}, children: nodeArr(CreateTextBox("TextBox", props: new Dictionary<string, object> {["class"]="HtmlBox", ["selectionColor"]=new Color(1F, 1F, 1F, 0.2F), ["-borderWidth"]=(Func<int>)(() => 0), ["multiline"]=true, ["useTypingState"]=(Action<TypingState>)((TypingState ___setTemp)=>typingState=___setTemp), ["text"]=(Func<string>)(() => text), ["setText"]=setText, ["diff"]=(Func<string,string,string>)((string oldStr, string newStr)=>{
					updateCount++;
					return ((System.Func<System.String,System.String,MonoGameHtml.TypingState,System.Collections.Generic.List<System.String>,System.String>)___vars["htmlDiff"])(oldStr, newStr, typingState, predictions);
				}), ["onTick"]=(Action)(()=>{
					if (code != ((System.String)___vars["code"])) setCode(((System.String)___vars["code"]));
					
					if (!updating && currUpdateCount != updateCount) {

						Logger.log(text);
					
						updating = true;
						Task.Run(()=>{
							try {
						    ((System.Func<System.Int32,System.String,System.Threading.Tasks.Task<System.ValueTuple<MonoGameHtml.HtmlNode,System.Exception,System.Int32>>>)___vars["updateHtml"])(updateCount, text).ContinueWith(task => {
						    	int thisUpdateCount = task.Result.Item3;
						    	if (thisUpdateCount > currUpdateCount) {
						    		updating = false;
									currUpdateCount = thisUpdateCount;
						    		
									setException(task.Result.Item2);
									setNode(task.Result.Item1);
						    	}
							});
							} catch (Exception e) {Logger.log("????", e.StackTrace);}
						});
						
					}
				})}, children: null, childrenFunc: null, textContent: "", ____selectionColor: new Color(1F, 1F, 1F, 0.2F), multiline: true, useTypingState: (Action<TypingState>)((TypingState ___setTemp)=>typingState=___setTemp), text: (Func<string>)(() => text), setText: setText, diff: (Func<string,string,string>)((string oldStr, string newStr)=>{
					updateCount++;
					return ((System.Func<System.String,System.String,MonoGameHtml.TypingState,System.Collections.Generic.List<System.String>,System.String>)___vars["htmlDiff"])(oldStr, newStr, typingState, predictions);
				})), newNode("h6", props: new Dictionary<string, object> {["color"]="white"}, textContent: (Func<string>)(()=> ""+(currUpdateCount)+"/"+(updateCount)+" "+(updating ? ((System.Func<System.Single,System.String>)___vars["loadingText"])(timePassed) : "")+"")), newNode("pseudo", props: new Dictionary<string, object> {["class"]="ReplaceText", ["renderAdd"]=(Action<SpriteBatch>)((SpriteBatch spriteBatch)=>{ 
					((System.Action<Microsoft.Xna.Framework.Graphics.SpriteBatch,System.String,MonoGameHtml.TypingState>)___vars["renderTabs"])(spriteBatch, text, typingState);
				})}, textContent: ""), CreateTextRender("TextRender", props: new Dictionary<string, object> {["textFunc"]=(Func<string>)(() => correctText())}, children: null, childrenFunc: null, textContent: "", textFunc: (Func<string>)(() => correctText())), CreatePredictor("Predictor", props: new Dictionary<string, object> {["textFunc"]=(Func<string>)(() => text), ["indexFunc"]=(Func<int>)(() => typingState.cursorIndex), ["setPredictions"]=setPredictions, ["typingState"]=typingState}, children: null, childrenFunc: null, textContent: "", textFunc: (Func<string>)(() => text), indexFunc: (Func<int>)(() => typingState.cursorIndex), setPredictions: setPredictions, typingState: typingState), newNode("span", props: new Dictionary<string, object> {}, children: nodeArr(newNode("div", props: new Dictionary<string, object> {["class"]="FileOptionButton", ["onPress"]=(Action)(()=>setShowCode(!showCode))}, textContent: "view code"), newNode("div", props: new Dictionary<string, object> {["class"]="FileOptionButton", ["onPress"]=openFileScreen}, textContent: "files..."), newNode("div", props: new Dictionary<string, object> {["class"]="FileOptionButton", ["onPress"]=(Action)(()=>{
							if (activeFilePath != null) {
								trySaveFile(activeFilePath, text);
							} else {
								saveBoxOpen = true;
							}
						})}, textContent: "save..."), newNode("div", props: new Dictionary<string, object> {["class"]="FileOptionButton", ["onPress"]=(Action)(()=>saveBoxOpen = true)}, textContent: "save as..."))))), newNode("div", props: new Dictionary<string, object> {["flex"]=1, ["backgroundColor"]="white"}, childrenFunc: (Func<HtmlNode[]>) (() => nodeArr(newNode("html", props: new Dictionary<string, object> {}, textContent: ""), (showCode ? newNode("p", props: new Dictionary<string, object> {}, textContent: (Func<string>)(()=> ""+(code)+"")) : (node ??  
					(
						(exception == null || text == "") ? 
							newNode("p", props: new Dictionary<string, object> {}, textContent: "Nothing to display...") : 
							newNode("p", props: new Dictionary<string, object> {["color"]="red"}, textContent: (Func<string>)(()=> ""+(exception == null ? "NULL?" : (exception.GetType().Name + "\n" + exception.Message))+""))
					)
				))))), CreateSaveBox("SaveBox", props: new Dictionary<string, object> {["open"]=(Func<bool>)(() => saveBoxOpen), ["close"]=(Action)(()=>saveBoxOpen=false), ["save"]=trySaveFile, ["contents"]=(Func<string>)(() => text), ["activePath"]=(Func<string>)(() => activeFilePath)}, children: null, childrenFunc: null, textContent: "", open: (Func<bool>)(() => saveBoxOpen), close: (Action)(()=>saveBoxOpen=false), save: trySaveFile, contents: (Func<string>)(() => text), activePath: (Func<string>)(() => activeFilePath))));
	
	return ___node;
}

HtmlNode CreatePredictor(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, Func<HtmlNode[]> childrenFunc = null, Func<string>? textFunc = null, Func<int>? indexFunc = null, Action<List<string>>? setPredictions = null, TypingState? typingState = null) {
	
	HtmlNode ___node = null;
		

	
string text = "";
int index = 0;
int cursorX = 0, cursorY = 0;
string searchFor = "";
List<string> newList = null;
List<string> list = null;
Action<List<string>> setListState = (___val) => {
	list = ___val;
	___node?.stateChangeDown();
};

var setList = (Action<List<string>>)((list)=>{
		setPredictions(list);
		setListState(list);
	});
var clear = (Action)(()=>{
		if (list != null) setList(null);
	});
var tick = (Action)(()=>{
	
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
			searchFor = ((System.Func<System.String,System.Int32,System.String>)___vars["findSearchFor"])(text, typingState.cursorIndex);
			(cursorX, cursorY) = ((System.Func<MonoGameHtml.TypingState,System.String,System.ValueTuple<System.Int32,System.Int32>>)___vars["cursorPos"])(typingState, text);
			Task.Run(() => {
				try {
					((System.Func<System.String,System.String,System.Int32,System.Threading.Tasks.Task<System.Collections.Generic.List<System.String>>>)___vars["predict"])(searchFor, text, typingState.cursorIndex).ContinueWith((task) => {
						if (text == newText) newList = task.Result;
					});
				} catch (Exception e) {
					//Logger.log(e.StackTrace);
					//Logger.log(e.Message);
					clear();
				}
			});			
		}
	});
;
	___node = newNode("pseudo", props: new Dictionary<string, object> {["onTick"]=tick}, childrenFunc: (Func<HtmlNode[]>) (() => nodeArr(((list == null || list.Count == 0) ? newNode("p", props: new Dictionary<string, object> {}, textContent: "") :
				newNode("div", props: new Dictionary<string, object> {["left"]=cursorX, ["top"]=cursorY, ["class"]="CodePredictionBox"}, childrenFunc: (Func<HtmlNode[]>) (() => nodeArr((list.Select(str => {
						int searchIndex = str.IndexOf(searchFor);
						return (
							newNode("span", props: new Dictionary<string, object> {}, children: nodeArr(newNode("h6", props: new Dictionary<string, object> {["class"]="CodePrediction"}, textContent: (Func<string>)(()=> ""+(str[..searchIndex])+"")), newNode("h6", props: new Dictionary<string, object> {["class"]="CodePrediction", ["color"]="orange"}, textContent: (Func<string>)(()=> ""+(searchFor)+"")), newNode("h6", props: new Dictionary<string, object> {["class"]="CodePrediction"}, textContent: (Func<string>)(()=> ""+(str[(searchIndex+searchFor.Length)..])+""))))
						);
					}).ToArray()))))
			))));
	
	return ___node;
}

HtmlNode CreateSearchBar(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, Func<HtmlNode[]> childrenFunc = null, Action<string>? setText = null, string path = "") {
	
	HtmlNode ___node = null;
		

	
List<(string stringName, string contents)> htmlSearchList = ((System.Func<System.String,System.Collections.Generic.List<System.ValueTuple<System.String,System.String>>>)___vars["searchHtml"])(path);
;
	___node = newNode("div", props: new Dictionary<string, object> {}, childrenFunc: (Func<HtmlNode[]>) (() => nodeArr((true ? null : htmlSearchList.Select(instance =>
				newNode("p", props: new Dictionary<string, object> {["onPress"]=(Action)(()=>setText(instance.contents))}, textContent: (Func<string>)(()=> ""+(instance.stringName)+""))
			).ToArray()))));
	
	return ___node;
}

HtmlNode CreateTextRender(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, Func<HtmlNode[]> childrenFunc = null, Func<string>? textFunc = null) {
	
	HtmlNode ___node = null;
		

	
string text = "";
Action<string> setText = (___val) => {
	text = ___val;
	___node?.stateChangeDown();
};

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
				return colorData.Concat(new []{(Color.White, text.Length - len)});
			}
		}*/

		
		return null;
	}

	;
	___node = newNode("pseudo", props: new Dictionary<string, object> {["class"]="ReplaceText", ["onTick"]=(Action)(()=>{
				string newText = textFunc();
				if (text != newText) {
					colorData = null;
					setText(newText);
					Task.Run(()=>{
						try {
							((System.Func<System.String,System.Threading.Tasks.Task<System.Collections.Generic.List<System.Collections.Generic.List<System.ValueTuple<Microsoft.Xna.Framework.Color,System.Int32>>>>>)___vars["colorHtml"])(text).ContinueWith(task => {
								if (newText == text) {
									colorData = task.Result;
									setText(newText);
								}
							});
						} catch (Exception e) {
							//Logger.log(e.StackTrace);
							//Logger.log(e.Message);
						}
					});
				}
			})}, childrenFunc: (Func<HtmlNode[]>) (() => nodeArr((FindColorData()?.Select(line => 
				newNode("span", props: new Dictionary<string, object> {}, childrenFunc: (Func<HtmlNode[]>) (() => nodeArr((line.Select(data => {
						int currI = i;
						var node = newNode("p", props: new Dictionary<string, object> {["class"]="Text", ["color"]=data.Item1}, textContent: (Func<string>)(()=> ""+(text.Replace("\n", " ").Substring(currI, data.Item2))+""));
						i += data.Item2;
						return node;
					}).ToArray()))))
			).ToArray()))));
	
	return ___node;
}

HtmlNode CreateTextInput(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, Func<HtmlNode[]> childrenFunc = null, Func<string>? text = null, Action<string>? setText = null, Func<bool>? active = null, bool multiline = false, Func<string,string,string>? diff = null, Action<TypingState>? useTypingState = null) {
	
	HtmlNode ___node = null;
		

	
TypingState typingState = new TypingState {
		multiline = multiline,
		diff = diff,
		undoFrequency = 1F,
	};
useTypingState?.Invoke(typingState);
;
	___node = newNode("div", props: new Dictionary<string, object> {["onTick"]=(Action)(()=>{
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
		})}, textContent: "");
	
	return ___node;
}

HtmlNode CreateTextBox(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, Func<HtmlNode[]> childrenFunc = null, Func<string>? text = null, Action<string>? setText = null, Func<string,string,string>? diff = null, bool multiline = false, bool cursorVisible = true, Action<TypingState>? useTypingState = null, Action? onEnter = null, string? label = null, bool scrollable = false, Color? ____selectionColor = null) {
	Color selectionColor = ____selectionColor ?? new Color(0F, 0F, 1F, 0.3F);

	HtmlNode ___node = null;
		

	
if (text == null && setText == null) {
		string str = textContent ?? "";
		text = () => str;
		setText = newStr => str = newStr;
	}
	
	bool active = false;
HtmlNode node = null;
TypingState typingState = null;
var trueDiff = (Func<string, string, string>)((oldStr, newStr)=>{
		if (oldStr.Length + 1 == newStr.Length && oldStr.CountOf("\n") < newStr.CountOf("\n")) onEnter.Invoke();
		return diff == null ? newStr : diff(oldStr, newStr);
	});
if (onEnter == null) trueDiff = diff;
;
	___node = newNode("div", props: new Dictionary<string, object> {["ref"]=(Action<HtmlNode>)((HtmlNode el)=>{
			node = el;
			typingState.node = el;
		}), ["class"]="TextBox", ["props"]=props, ["onMouseDown"]=(Action)(()=>active=node.clicked), ["-borderWidth"]=(Func<int>)(() => (active) ? 1 : 0), ["-textContent"]=(Func<string>)(() => ((label != null && text().Length == 0) ? label : text()).Replace("\t", TextInputUtil.spacesPerTab)), ["renderAdd"]=(Action<SpriteBatch>)((SpriteBatch spriteBatch)=>{
				if (typingState.HasRealSelection()) TextInputUtil.drawSelection(spriteBatch, node, typingState, text(), selectionColor);
				if (!cursorVisible || !active || ((timePassed - typingState.lastEditOrMove > 1) && ((timePassed % 1F) < 0.5F))) return;
				TextInputUtil.drawCursor(spriteBatch, node, typingState, text());
			}), ["onPress"]=(Action)(()=>{
				TextInputUtil.Click(mousePos, node, typingState, text());
			}), ["onMouseDrag"]=(Action)(()=>{
				if (!active) return;
				TextInputUtil.setCursorFromPos(mousePos, node, typingState, text());
			})}, children: nodeArr(CreateTextInput("TextInput", props: new Dictionary<string, object> {["text"]=text, ["setText"]=setText, ["diff"]=trueDiff, ["active"]=(Func<bool>)(() => active), ["multiline"]=multiline, ["useTypingState"]=(Action<TypingState>)((TypingState state)=>{
					typingState = state;
					useTypingState?.Invoke(typingState);
				})}, children: null, childrenFunc: null, textContent: "", text: text, setText: setText, diff: trueDiff, active: (Func<bool>)(() => active), multiline: multiline, useTypingState: (Action<TypingState>)((TypingState state)=>{
					typingState = state;
					useTypingState?.Invoke(typingState);
				}))));
	
	return ___node;
}

HtmlNode CreateKeyInput(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, Func<HtmlNode[]> childrenFunc = null) {
	
	HtmlNode ___node = null;
		

	
;
	___node = newNode("todo", props: new Dictionary<string, object> {}, textContent: "");
	
	return ___node;
}

HtmlNode CreateFrameCounter(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, Func<HtmlNode[]> childrenFunc = null, float updateTime = 1F) {
	
	HtmlNode ___node = null;
		

	
var fpsCounter = new FrameCounter();
int updateCount = 0;
;
	___node = newNode("div", props: new Dictionary<string, object> {["onTick"]=(Action)(()=>{
			fpsCounter.update(deltaTime);
			int currUpdate = (int) (timePassed / updateTime);
			if (currUpdate != updateCount) {
				updateCount = currUpdate;
				game.Window.Title = $"FPS: {(int) fpsCounter.AverageFramesPerSecond}";
			}
		})}, textContent: "");
	
	return ___node;
}

HtmlNode CreateIf(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, Func<HtmlNode[]> childrenFunc = null, bool __parens__ = false) {
	
	HtmlNode ___node = null;
		

	
HtmlNode[] nodeArray = null;
if (__parens__) {
        nodeArray = (childrenFunc != null ? childrenFunc() : children);
    }
    
	;
	___node = newNode("if", props: new Dictionary<string, object> {["propsUnder"]=props, ["ref"]=(Action<HtmlNode>)((HtmlNode node) => {
	        if (__parens__) return;
	        HtmlNode next = node.GetNext();
	        if (next != null && (next.tag == "else" || next.tag == "elif")) {
	            next.prop<Action>("trigger")();
	        }
	    })}, childrenFunc: (Func<HtmlNode[]>) (() => nodeArr(newNode("html", props: new Dictionary<string, object> {}, textContent: ""), (nodeArray))));
	
	return ___node;
}

HtmlNode CreateElif(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, Func<HtmlNode[]> childrenFunc = null, bool __parens__ = false) {
	
	HtmlNode ___node = null;
		

	
HtmlNode[] nodeArray = null;
Action<HtmlNode[]> setNodeArray = (___val) => {
	nodeArray = ___val;
	___node?.stateChangeDown();
};

var onTrigger = (Action)(()=>{
        if (__parens__) {
	        setNodeArray((childrenFunc != null ? childrenFunc() : children));
	    } else {
	        HtmlNode next = ___node.GetNext();
	        if (next != null && (next.tag == "else" || next.tag == "elif")) {
	            next.prop<Action>("trigger")();
	        }
	    }
    });
;
	___node = newNode("elif", props: new Dictionary<string, object> {["propsUnder"]=props, ["trigger"]=onTrigger}, childrenFunc: (Func<HtmlNode[]>) (() => nodeArr(newNode("html", props: new Dictionary<string, object> {}, textContent: ""), (nodeArray))));
	
	return ___node;
}

HtmlNode CreateElse(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, Func<HtmlNode[]> childrenFunc = null) {
	
	HtmlNode ___node = null;
		

	
HtmlNode[] nodeArray = null;
Action<HtmlNode[]> setNodeArray = (___val) => {
	nodeArray = ___val;
	___node?.stateChangeDown();
};

;
	___node = newNode("else", props: new Dictionary<string, object> {["propsUnder"]=props, ["trigger"]=(Action)(()=>setNodeArray((childrenFunc != null ? childrenFunc() : children)))}, childrenFunc: (Func<HtmlNode[]>) (() => nodeArr(newNode("html", props: new Dictionary<string, object> {}, textContent: ""), (nodeArray))));
	
	return ___node;
}

HtmlNode CreateTry(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, Func<HtmlNode[]> childrenFunc = null) {
	
	HtmlNode ___node = null;
		

	
// TODO: add ability to force dynamic children!!!

    HtmlNode[] nodeArray = null;
Exception exception = null;
try {
        nodeArray = childrenFunc();
    } catch (Exception e) {
		exception = e;
	}
    
	;
	___node = newNode("try", props: new Dictionary<string, object> {["propsUnder"]=props, ["ref"]=(Action<HtmlNode>)((HtmlNode node) => {
	        if (exception == null) return;
	        HtmlNode next = node.GetNext();
	        if (next != null && next.tag == "catch") {
	            next.prop<Action<Exception>>("trigger")(exception);
	        }
	    })}, childrenFunc: (Func<HtmlNode[]>) (() => nodeArr(newNode("html", props: new Dictionary<string, object> {}, textContent: ""), (nodeArray))));
	
	return ___node;
}

HtmlNode CreateCatch(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, Func<HtmlNode[]> childrenFunc = null, Action<Exception>? __parens__ = null) {
	
	HtmlNode ___node = null;
		

	
HtmlNode[] nodeArray = null;
Action<HtmlNode[]> setNodeArray = (___val) => {
	nodeArray = ___val;
	___node?.stateChangeDown();
};

;
	___node = newNode("catch", props: new Dictionary<string, object> {["propsUnder"]=props, ["trigger"]=(Action<Exception>)((Exception exception)=>{
			setNodeArray((childrenFunc != null ? childrenFunc() : children));
			__parens__?.Invoke(exception);
		})}, childrenFunc: (Func<HtmlNode[]>) (() => nodeArr(newNode("html", props: new Dictionary<string, object> {}, textContent: ""), (nodeArray))));
	
	return ___node;
}

HtmlNode CreateSwitch(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, Func<HtmlNode[]> childrenFunc = null, object? __parens__ = null) {
	
	HtmlNode ___node = null;
		

	
if (__parens__ == null) throw new Exception("Switch-value may not be null");
string currCase = "";
Action<string> setCurrCase = (___val) => {
	currCase = ___val;
	___node?.stateChangeDown();
};

Action onTick = null;
string init = null;
if (__parens__ is Func<string> func) {
		Action action = () => {
			string newCase = func();
			if (newCase != currCase) {
				setCurrCase(newCase);
			}
		};
		onTick = action;
		init = func();
	} else {
		init = __parens__.ToString();
	}
	setCurrCase(init);
Dictionary<string, HtmlNode> nodeDict = new Dictionary<string, HtmlNode>();
HtmlNode def = null;
var extractContent = (Func<HtmlNode, HtmlNode>)((node)=>{
		HtmlNode[] nodeContents = node.Contents;
		if (nodeContents.Length == 1) return nodeContents[0];
		return (
			newNode("fragment", props: new Dictionary<string, object> {}, childrenFunc: (Func<HtmlNode[]>) (() => nodeArr(newNode("html", props: new Dictionary<string, object> {}, textContent: ""), (nodeContents))))
		);
	});
HtmlNode[] contents = (childrenFunc != null ? childrenFunc() : children);
if (contents != null && contents.Length > 0) {
		foreach (HtmlNode node in contents) {
			if (node.props.ContainsKey("default") && node.prop<bool>("default") == true) {
				def = (node.tag == "default") ? extractContent(node) : node;
				continue;
			}
			if (!node.props.ContainsKey("case")) continue;
			string thisCase = node.prop<string>("case");
			nodeDict[thisCase] = (node.tag == "case") ? extractContent(node) : node;
		};
	}

	;
	___node = nodeDict.ContainsKey(currCase) ? nodeDict[currCase] : def;
	
	return ___node;
}

HtmlNode CreateCase(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, Func<HtmlNode[]> childrenFunc = null, object? __parens__ = null) {
	
	HtmlNode ___node = null;
		

	
if (__parens__ == null) throw new Exception("Case-value may not be null");
;
	___node = newNode("case", props: new Dictionary<string, object> {["case"]=__parens__.ToString(), ["propsUnder"]=props}, childrenFunc: (Func<HtmlNode[]>) (() => nodeArr((((childrenFunc != null ? childrenFunc() : children) ?? (new HtmlNode[]{newNode("p", props: new Dictionary<string, object> {}, textContent: (Func<string>)(()=> ""+(textContent)+""))}))))));
	
	return ___node;
}

HtmlNode CreateDefault(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, Func<HtmlNode[]> childrenFunc = null) {
	
	HtmlNode ___node = null;
		

	
;
	___node = newNode("case", props: new Dictionary<string, object> {["default"]=true, ["propsUnder"]=props}, childrenFunc: (Func<HtmlNode[]>) (() => nodeArr((((childrenFunc != null ? childrenFunc() : children) ?? (new HtmlNode[]{newNode("p", props: new Dictionary<string, object> {}, textContent: (Func<string>)(()=> ""+(textContent)+""))}))))));
	
	return ___node;
}
HtmlNode node = CreateApp("App", props: new Dictionary<string, object> {}, children: null, childrenFunc: null, textContent: "");
setupNode(node);
return node;
		}
	}
}
