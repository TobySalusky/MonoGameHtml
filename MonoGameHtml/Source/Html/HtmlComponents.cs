﻿using System.Collections.Generic;
 using System.Linq;

 namespace MonoGameHtml {
	public static class HtmlComponents {
		public static string[] Create(params string[] componentFileContents) {
			List<string> componentStrings = new List<string>();
			
			// extract multiple components from single strings
			foreach (string componentFile in componentFileContents) {
				
				var bracketPairs = DelimPair.genPairs(componentFile, DelimPair.CurlyBrackets).Where(pair => pair.nestCount == 0).ToArray();

				for (int i = 0; i < bracketPairs.Length; i++) {
					string component = componentFile.sub((i == 0) ? 0 : bracketPairs[i - 1].AfterClose, bracketPairs[i].AfterClose);
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
		<div ref={(HtmlNode el)=^node=el} onPress={()=^{
			toMouse();
		}} onMouseDrag={()=^{
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
		<span onPress={()=^{
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
}
", TextBox = @"
const TextBox = (
	Func<string> text, Action<string> setText, bool multiline = false
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
			<TextInput text={text} setText={setText} active={bool: active} multiline={multiline}/>
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
		<div onTick={()=^{
			fpsCounter.update(@dt);
			int currUpdate = (int) (@t / updateTime);
			if (currUpdate != updateCount) {
				updateCount = currUpdate;
				game.Window.Title = $'FPS: {(int) fpsCounter.AverageFramesPerSecond}';
			}
		}}/>
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