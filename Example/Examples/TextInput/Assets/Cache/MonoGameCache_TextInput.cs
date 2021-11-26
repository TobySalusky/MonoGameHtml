

using System;
using System.Collections.Generic;
using System.Linq;
using MonoGameHtml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace MonoGameHtmlGeneratedCode.TextInput {
	public class Cache_TextInput : StatePack {
	public Cache_TextInput(params object[] initialVariableNamesAndObjects) : base(initialVariableNamesAndObjects) {}
		protected override string[] cachedInput() {
			return new string[]{ @"<App></App>", @"

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
}", @"const App = () => {

    HtmlNode basicTextBox = (
        <TextBox>
            Some starting text
        </TextBox>
    );

    var diff = (string oldText, string newText) : string => {
        if (newText.Contains(' ')) return oldText;
        return newText;
    };

    HtmlNode noSpacesTextBox = (
        <TextBox diff={diff}>NoSpacesAllowed!</TextBox>
    );

    string textForSecondBox = 'This one is\nmultiline!\n(and it prints to the console)';
    var callBack = (string text) => {
        textForSecondBox = text;
        Console.WriteLine($'The textbox now contains {text.Length} characters!');
    };
    HtmlNode mutllineTextBoxWithCallbacks = (
        <TextBox multiline={true}
                text={string: textForSecondBox} setText={callBack}
        ></TextBox>
    );

    HtmlNode fancyTextBox = (
        <TextBox class='StylishTextBox' multiline={true} selectionColor={new Color(1F, 0.3F, 0F, 0.3F)}>
            Fancy...
        </TextBox>
    );


    return (
        <body>
            <html></html>
            {basicTextBox}
            {noSpacesTextBox}
            {mutllineTextBoxWithCallbacks}
            {fancyTextBox}
        </body>
    );
}" };
		}

		protected override HtmlNode cachedNode() {
			/*IMPORTS_DONE*/
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

HtmlNode CreateApp(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, Func<HtmlNode[]> childrenFunc = null) {
	
	HtmlNode ___node = null;
		

	
HtmlNode basicTextBox = (
        CreateTextBox("TextBox", props: new Dictionary<string, object> {}, children: null, childrenFunc: null, textContent: "Some starting text")
    );
var diff = (Func<string, string, string>)((oldText, newText)=>{
        if (newText.Contains(" ")) return oldText;
        return newText;
    });
HtmlNode noSpacesTextBox = (
        CreateTextBox("TextBox", props: new Dictionary<string, object> {["diff"]=diff}, children: null, childrenFunc: null, textContent: "NoSpacesAllowed!", diff: diff)
    );
string textForSecondBox = "This one is\nmultiline!\n(and it prints to the console)";
var callBack = (Action<string>)((text)=>{
        textForSecondBox = text;
        Console.WriteLine($"The textbox now contains {text.Length} characters!");
    });
HtmlNode mutllineTextBoxWithCallbacks = (
        CreateTextBox("TextBox", props: new Dictionary<string, object> {["multiline"]=true, ["text"]=(Func<string>)(() => textForSecondBox), ["setText"]=callBack}, children: null, childrenFunc: null, textContent: "", multiline: true, text: (Func<string>)(() => textForSecondBox), setText: callBack)
    );
HtmlNode fancyTextBox = (
        CreateTextBox("TextBox", props: new Dictionary<string, object> {["class"]="StylishTextBox", ["multiline"]=true, ["selectionColor"]=new Color(1F, 0.3F, 0F, 0.3F)}, children: null, childrenFunc: null, textContent: "Fancy...", multiline: true, ____selectionColor: new Color(1F, 0.3F, 0F, 0.3F))
    );
;
	___node = newNode("body", props: new Dictionary<string, object> {}, childrenFunc: (Func<HtmlNode[]>) (() => nodeArr(newNode("html", props: new Dictionary<string, object> {}, textContent: ""), (basicTextBox), (noSpacesTextBox), (mutllineTextBoxWithCallbacks), (fancyTextBox))));
	
	return ___node;
}
HtmlNode node = CreateApp("App", props: new Dictionary<string, object> {}, children: null, childrenFunc: null, textContent: "");
setupNode(node);
return node;
		}
	}
}
