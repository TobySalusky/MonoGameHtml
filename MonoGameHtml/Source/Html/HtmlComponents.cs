﻿using System.Collections.Generic;
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
				if (!cursorVisible || !active || ((@t - typingState.lastEditOrMove > 1) && ((@t % 1F) < 0.5F))) return;
				TextInputUtil.drawCursor(spriteBatch, node, typingState, text());
			}}
			onPress={()=>{
				TextInputUtil.setCursorFromPos(@mp, node, typingState, text());
			}}
			onMouseDrag={()=>{
				TextInputUtil.setCursorFromPos(@mp, node, typingState, text());
			}}
		>
			<TextInput text={text} setText={setText} diff={diff} active={bool: active} multiline={multiline}
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
	
    return (<div/>);
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
", Switch = @"
const Switch = (object __parens__) => {

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
	
	foreach (HtmlNode node in children) {
		if (node.props.ContainsKey('default') && node.prop<bool>('default') == true) {
			def = node;
			continue;
		}
		if (!node.props.ContainsKey('case')) continue;
		string thisCase = node.prop<string>('case');
		nodeDict[thisCase] = node;
	};

	
	return (
		<div onTick={onTick}>
			<html/>
			{nodeDict.ContainsKey(currCase) ? nodeDict[currCase] : def}
		</div>
	);
}
", Case = @"
const Case = (object __parens__) => {
	return (
		<case case={__parens__.ToString()}>
			<html/>
			{children}
		</case>
	);
}
", Default = @"
const Default = () => {
	return (
		<case default={true}>
			<html/>
			{children}
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
const If = (bool __parens__ = false, Func<HtmlNode[]> childrenFunc) => {

    HtmlNode[] nodeArray = null;
    if (__parens__) {
        nodeArray = children ?? childrenFunc();
    }
    
	return (
	    <if ref={(HtmlNode node) => {
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
const Elif = (bool __parens__ = false, Func<HtmlNode[]> childrenFunc) => {
    
    HtmlNode[] [nodeArray, setNodeArray] = useState(null);
    
    var onTrigger = () => {
        if (__parens__) {
	        setNodeArray(children ?? childrenFunc());
	    } else {
	        HtmlNode next = ___node.GetNext();
	        if (next != null && (next.tag == 'else' || next.tag == 'elif')) {
	            next.prop<Action>('trigger')();
	        }
	    }
    };
    
	return (
	    <elif trigger={onTrigger}>
	        <html/>
	        {nodeArray}
        </elif>
	);
}
", Else = @"
const Else = (Func<HtmlNode[]> childrenFunc) => {
    
    HtmlNode[] [nodeArray, setNodeArray] = useState(null);
    
	return (
		<else trigger={()=>setNodeArray(children ?? childrenFunc())}>
			<html/>
		    {nodeArray}
        </else>
	);
}
", Try = @"
const Try = (Func<HtmlNode[]> childrenFunc) => { // TODO: add ability to force dynamic children!!!

    HtmlNode[] nodeArray = null;
	Exception exception = null;
    try {
        nodeArray = childrenFunc();
    } catch (Exception e) {
		exception = e;
	}
    
	return (
	    <if ref={(HtmlNode node) => {
	        if (exception == null) return;
	        HtmlNode next = node.GetNext();
	        if (next != null && next.tag == 'catch') {
	            next.prop<Action<Exception>>('trigger')(exception);
	        }
	    }}>
	        <html/>
	        {nodeArray}
        </if>
	);
}
", Catch = @"
const Catch = (Action<Exception> __parens__, Func<HtmlNode[]> childrenFunc) => {
    
    HtmlNode[] [nodeArray, setNodeArray] = useState(null);
    
	return (
		<catch trigger={(Exception exception)=>{
			setNodeArray(children ?? childrenFunc());
			__parens__?.Invoke(exception);
		}}>
			<html/>
		    {nodeArray}
        </catch>
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
";
	}
 }