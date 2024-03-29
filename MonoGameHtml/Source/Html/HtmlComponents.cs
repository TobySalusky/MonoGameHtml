﻿﻿using System.Collections.Generic;
 using System.IO;
 using System.Linq;

 namespace MonoGameHtml {
	public static class HtmlComponents {
		public static string[] Create(params string[] componentFileContents) {
			var componentStrings = new List<string>();
			
			// extract multiple components from single strings
			foreach (string componentFile in componentFileContents) {
				
				var bracketPairs = DelimPair.genPairs(componentFile, DelimPair.CurlyBrackets).Where(pair => pair.nestCount == 0).ToArray();

				for (int i = 0; i < bracketPairs.Length; i++) {
					string component = componentFile.Sub((i == 0) ? 0 : bracketPairs[i - 1].AfterClose, bracketPairs[i].AfterClose);
					componentStrings.Add(component);
				}
			}
			return componentStrings.ToArray();
		}

		public static string[] AllScriptFilePaths(string folderAbsolutePath, bool subfolders = true) { 
			return Directory.GetFiles(folderAbsolutePath, 
				"*.monohtml", subfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
		}

		public static string ReadFrom(string folderAbsolutePath, bool subfolders = true) {
			string[] filePaths = AllScriptFilePaths(folderAbsolutePath, subfolders);
			var contents = filePaths.Select(File.ReadAllText);
			return string.Join("\n", contents);
		}

		public const string Slider = @"
const Slider = (
	Action<float> onChange,
	object back: 'darkgray',
	object front: 'lightgray',
	object width: 100,
	object height: 30,
	float init = 0F
) => {

    HtmlNode node = null;
    float amount = init, lastAmount = amount;
    
    void toMouse() {
    	amount = Math.Clamp((@mp.X-node.PaddedX)/node.PaddedWidth, 0F, 1F);
		if (amount != lastAmount) onChange?.Invoke(amount);    	
    	lastAmount = amount;
    }

	return (
		<div ref={(HtmlNode el)=>node=el} onPress={()=>{
			toMouse();
		}} onMouseDrag={()=>{
			if (node.clicked) toMouse();
		}} width={width} height={height} backgroundColor={back}>
            <div backgroundColor={front} -width={int~: node.PaddedWidth * amount} height='100%'/>
        </div>
	);
}
",
			Toggle = @"
const Toggle = (
    Action<bool> onChange,
    object back: 'darkgray',
    object front: 'lightgray',
    object width: 100,
    object height: 50
) => {

    bool [val, setVal] = useState(false);

	return (
		<span onPress={()=>{
                setVal(!val);
                onChange?.Invoke(val);
              }} backgroundColor={back} width={width} height={height} borderRadius='50%' padding={10}>
            {val ? <div flex={1}/> : null}
            <div dimens={height} backgroundColor={front} borderRadius='50%'/>
            {val ? null : <div flex={1}/>}
        </span>
	);
}
",
			TextInput = @"
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
}
",
			TextBox = @"
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
				if (!cursorVisible || !active || ((@t - typingState.lastEditOrMove > 1) && ((@t % 1F) < 0.5F))) return;
				TextInputUtil.drawCursor(spriteBatch, node, typingState, text());
			}}
			onPress={()=>{
				TextInputUtil.Click(@mp, node, typingState, text());
			}}
			onMouseDrag={()=>{
				if (!active) return;
				TextInputUtil.setCursorFromPos(@mp, node, typingState, text());
			}}
		>
			<TextInput text={text} setText={setText} diff={trueDiff} active={bool: active} multiline={multiline}
				useTypingState={(TypingState state)=>{
					typingState = state;
					useTypingState?.Invoke(typingState);
				}}
			/>
		</div>
	);
}
",
			KeyInput = @"
const KeyInput = () => {
    return (<todo/>);
}
",
			FrameCounter = @"
const FrameCounter = (float updateTime = 1F) => {

	var fpsCounter = new FrameCounter();
	int updateCount = 0;

	return (
		<div onTick={()=>{
			fpsCounter.update(@dt);
			int currUpdate = (int) (@t / updateTime);
			if (currUpdate != updateCount) {
				updateCount = currUpdate;
				game.Window.Title = $'FPS: {(int) fpsCounter.AverageFramesPerSecond}';
			}
		}}/>
	);
}
", // TODO: recursion on Switch? perhaps add RecursiveSwitch/DeepSwitch component?
			Switch = @"
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
				<html/>{nodeContents}
			</fragment>
		);
	};
	
	HtmlNode[] contents = @contents;

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
}
", Case = @"
const Case = (object __parens__) => {
	
	if (__parens__ == null) throw new Exception('Case-value may not be null');

	return (
		<case case={__parens__.ToString()} propsUnder={props}>
			{@inner}
		</case>
	);
}
", Default = @"
const Default = () => {
	return (
		<case default={true} propsUnder={props}>
			{@inner}
		</case>
	);
}
", Table = @"
const Table = () => {
	var headers = new List<HtmlNode>();
	var data = new List<HtmlNode>();
	var rowHeights = new List<int>();
	
	void Recurse(HtmlNode[] childArr) {
		foreach (var child in childArr) {
			if (child.tag == 'th') headers.Add(child);
			else if (child.tag == 'td') data.Add(child);
	    	else if (child.children != null) Recurse(child.children);
		}
	}

	Recurse(children);

	var cols = new List<HtmlNode>[headers.Count]; // TODO: more efficient calculation of column lengths
	for (int i = 0; i < cols.Length; i++) {
		cols[i] = new List<HtmlNode>();
	}
	for (int i = 0; i < data.Count; i++) {
	    var list = cols[i % cols.Length];
		list.Add(data[i]);
		if (list.Count > rowHeights.Count) rowHeights.Add(-1);
	}
	
	props = null; // TODO: automatic memory release on unnecessary construction vars?
	children = null;

    void SetRowHeight(int row, int val, HtmlNode mainNode) {
	    rowHeights[row] = val;
	    for (int i = 0; i < cols.Length; i++) {
	        if (row < cols[i].Count) {
	            var node = cols[i][row];
	            node.width = mainNode.children[i].width;
	            node.height = val; // TODO: make width property, this is pretty unsafe
	        }
	    }
	}

	return (
		<span onResize={(HtmlNode node)=>{
            bool flag = false;
            for (int i = 0; i < rowHeights.Count; i++) {
                int max = 0;
                for (int j = 0; j < cols.Length; j++) {
                    if (i < cols[j].Count) {
                        max = Math.Max(max, cols[j][i].height);
                    }
                }
                if (max != rowHeights[i]) {
                    SetRowHeight(i, max, node);
                    flag = true;
                }
            }
            
            if (flag) node.triggerOnResize();
		}}>
			{nStream(cols.Length).map(i =>
				<div>
					<html/>
					{headers[i]}
					{cols[i].ToArray()}
				</div>
			)}
		</div>
	);
}
", If = @"
const If = (bool __parens__ = false) => {

    HtmlNode[] nodeArray = null;
    if (__parens__) {
        nodeArray = @contents;
    }
    
	return (
	    <if propsUnder={props} ref={(HtmlNode node) => {
	        if (__parens__) return;
	        HtmlNode next = node.GetNext();
	        if (next != null && (next.tag == 'else' || next.tag == 'elif')) {
	            next.prop<Action>('trigger')();
	        }
	    }}>
	        <html/>
	        {nodeArray}
        </if>
	);
}
", Elif = @"
const Elif = (bool __parens__ = false) => {
    
    HtmlNode[] [nodeArray, setNodeArray] = useState(null);
    
    var onTrigger = () => {
        if (__parens__) {
	        setNodeArray(@contents);
	    } else {
	        HtmlNode next = ___node.GetNext();
	        if (next != null && (next.tag == 'else' || next.tag == 'elif')) {
	            next.prop<Action>('trigger')();
	        }
	    }
    };
    
	return (
	    <elif propsUnder={props} trigger={onTrigger}>
	        <html/>
	        {nodeArray}
        </elif>
	);
}
", Else = @"
const Else = () => {
    
    HtmlNode[] [nodeArray, setNodeArray] = useState(null);
    
	return (
		<else propsUnder={props} trigger={()=>setNodeArray(@contents)}>
			<html/>
		    {nodeArray}
        </else>
	);
}
", Try = @"
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
	        <html/>
	        {nodeArray}
        </try>
	);
}
", Catch = @"
const Catch = (Action<Exception> __parens__) => {
    
    HtmlNode[] [nodeArray, setNodeArray] = useState(null);
    
	return (
		<catch propsUnder={props} trigger={(Exception exception)=>{
			setNodeArray(@contents);
			__parens__?.Invoke(exception);
		}}>
			<html/>
		    {nodeArray}
        </catch>
	);
}
", PanelView = @"
const PanelView = () => {

	HtmlNode[] contents = @contents;
	float totalFlex = 0F;
	
	Func<HtmlNode, int, int> adjustDiff = (panel, diff) => {
		if (diff >= 0) return diff;
		if (panel.prop<Func<int>>('getWidth').Invoke() + diff < panel.prop<int>('minWidth')) {
			return panel.prop<int>('minWidth') - panel.prop<Func<int>>('getWidth').Invoke();
		}
		return diff;
	};
	
	Action<HtmlNode, int> diffPanelWidth = (panel, diff) => {
		panel.prop<Action<int>>('setWidth').Invoke(panel.prop<Func<int>>('getWidth').Invoke() + diff);
	};
	
	for (int i = 0; i < contents.Length; i++) {
		HtmlNode node = contents[i];
		if (i % 2 == 0) {
			if (node.tag != 'panel') {
				throw new Exception($'element {i} of PanelView must be a panel');
			}
			totalFlex += node.prop<float>('initFlex');
			
		} else {
		
			if (node.tag != 'splitter') {
				throw new Exception($'element {i} of PanelView must be a splitter');
			}
						
			int thisI = i;
			Func<int, int> shiftFunc = (int diff) => {
				diff = adjustDiff(contents[thisI - 1], diff);
				diff = adjustDiff(contents[thisI + 1], -diff) * -1;
				
				diffPanelWidth(contents[thisI - 1], diff);
				diffPanelWidth(contents[thisI + 1], -diff);
								
				___node.triggerOnResize();
				return diff;
			};
			
			node.prop<Action<Func<int, int>>>('setShiftFunc')(shiftFunc);
		}
	}

	return (
		<span dimens='100%' props={props}
			ref={(HtmlNode thisNode) => {
				int splitterWidthUsed = 0;
				
				for (int i = 0; i < contents.Length; i++) {
					if (i % 2 == 1) {
						HtmlNode splitter = contents[i];
						splitterWidthUsed += splitter.FullWidth;
					}
				}
				
				Logger.log(splitterWidthUsed);
				
				int availSpace = thisNode.width - splitterWidthUsed;
				int spaceUsed = 0;
				
				Logger.log(availSpace, thisNode.width, splitterWidthUsed);
				
				for (int i = 0; i < contents.Length; i++) {
					if (i % 2 == 0) {
						HtmlNode panel = contents[i];
						if (i == contents.Length - 1) {
							panel.props['width'] = availSpace - spaceUsed;
						} else {
							int thisPanelWidth = (int) ((float) availSpace * (panel.prop<float>('initFlex') / totalFlex));
							panel.props['width'] = thisPanelWidth;
							spaceUsed += thisPanelWidth;
						}
					}
				}
				thisNode.triggerOnResize();
			}}
		>
			{contents}
			<html/>
		</span>
	);
}
", Panel = @"
const Panel = (int minWidth = 20, float initFlex = 1F) => {

	return (
		<panel minWidth={minWidth} props={props} initFlex={initFlex} height='100%' class='BasePanel' getWidth={int: ___node.width} setWidth={(int newWidth) => ___node.width=newWidth}>
			<html/>
			{@contents}
		</panel>
	);
}
", Splitter = @"
const Splitter = () => {

	Func<int, int> shiftFunc = null;
	bool mouseOver = false;
	bool dragging = false;
	float dragStartX = 0f;
	
	return (
		<splitter height='100%' class='BaseSplitter' props={props}
			setShiftFunc={(Func<int,int> newShiftFunc) => shiftFunc = newShiftFunc}
			onMouseEnter={() => mouseOver=true}
			onMouseExit={() => mouseOver=false}
			onMouseDown={() => {
				if (!mouseOver) return;
				dragging = true;
				dragStartX = @mp.X;
			}}
			onMouseUp={() => dragging=false}
			
			onTick={()=> {
				if (!dragging) return;
				
				int fullDragX = (int) (@mp.X - dragStartX);
				if (fullDragX != 0) {
					if (shiftFunc != null) {
						dragStartX += shiftFunc.Invoke(fullDragX);
					}
				}
			}}
		/>
	);
}
";

		public static string AllInput = @$"
{TextInput}
{TextBox}
{KeyInput}
", AllControlFlow = @$"
{If}
{Elif}
{Else}
{Try}
{Catch}
{Switch}
{Case}
{Default}
", AllPanel = $@"
{PanelView}
{Panel}
{Splitter}
", All = $@"
{AllInput}
{AllControlFlow}
{Slider}
{Toggle}
{FrameCounter}
{AllPanel}
";
	}
 }