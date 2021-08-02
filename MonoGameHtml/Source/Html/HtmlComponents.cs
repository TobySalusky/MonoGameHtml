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
", Toggle = @"
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
", TextInput = @"
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
", TextBox = @"
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
", KeyInput = @"
const KeyInput = () => {
	
    return (<div/>);
}
", FrameCounter = @"
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
const Switch = (Func<string> caseFunc) => {

	Dictionary<string, HtmlNode> nodeDict = new Dictionary<string, HtmlNode>();
	
	foreach (HtmlNode node in children) {
		if (!node.props.ContainsKey('case')) continue;
		string thisCase = node.prop<string>('case');
		nodeDict[thisCase] = node;
	}
	
	string [currCase, setCurrCase] = useState(caseFunc());
	
	return (
		<div onTick={()=>{
			string newCase = caseFunc();
			if (newCase != currCase) {
				setCurrCase(newCase);
			}
		}}>
			<html/>
			{nodeDict.ContainsKey(currCase) ? nodeDict[currCase] : null}
		</div>
	);
}
", Table = @"
const Table = () => {
  var headers = new List<HtmlNode>();
  var data = new List<HtmlNode>();
  
  void recurse(HtmlNode[] childArr) {
    foreach (var child in childArr) {
      if (child.tag == 'th') headers.Add(child);
      else if (child.tag == 'td') data.Add(child);
	  else if (child.children != null) recurse(child.children);
    }
  }

  recurse(children);

  var cols = new List<HtmlNode>[headers.Count]; // TODO: more efficient calculation of column lengths
  for (int i = 0; i < cols.Length; i++) {
    cols[i] = new List<HtmlNode>();
  }
  for (int i = 0; i < data.Count; i++) {
    cols[i % cols.Length].Add(data[i]);
  }

  return (
    <span>
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

";

		public static string AllInput = @$"
{TextInput}
{TextBox}
{KeyInput}
";
	}
 }