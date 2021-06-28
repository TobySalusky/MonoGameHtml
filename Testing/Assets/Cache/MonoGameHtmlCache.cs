using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGameHtml;

namespace MonoGameHtmlGeneratedCode {
	public class Cache : StatePack {
	public Cache(params object[] initialVariableNamesAndObjects) : base(initialVariableNamesAndObjects) {}
	/*CACHE_START*/
protected override string[] cachedInput() {
	return new string[]{ @"
<body>
    <Move text='helo'/>
    <Move text='henlo'/> 
    <Move text={''+$h}/>
    <Move text='henlo'/>
    <Move text='helo'/>
    <Slider back='red' front='green' width={700} height='10%' init={0.5F}/>
    <Slider onChange={(float amount) =^Logger.log(amount)}/>
    <Toggle/>
    <Toggle back='white' front='green' onChange={(bool val)=^Logger.log($'flipped to {val}!')}/>
</body>
", @"
const Move = (string text) => {
    
    const float c = 150;
    float lastX = -1;
    float width = 400;

    return (
        <div -width={int~: width} -backgroundColor={Color: Color.Lerp(Color.Cyan, Color.Orange, (width - c) / (ScreenWidth-2*c))} onMouseExit={()=^lastX = -1} onHover={()=^ {
             float x = @mp.X;
             if (lastX != -1) width = Math.Clamp(width + (x - lastX), c, ScreenWidth-c);
             lastX = x;
        }}>
            {text}
        </div>
    );
}", @"

const Container = (List<string> init) => {

    List^^string^ [rows, setRows] = useState(init);

    return (
        <div alignX='center' alignY='flexStart' width='100%'>
            {rows.map((str, i) =^
                <Row rows={rows} setRows={setRows} i={i}/>
            )}
            <div onPress={() =^ {
                rows.Add($'new {random()}');
                setRows(rows);
            }} textAlign='center' width='50%' height={$h} backgroundColor='white' borderColor='#888888' borderWidth={2}>+</div>
        </div>
    );
}", @"

const Row = (List<string> rows, Action<List<string>> setRows, int i = -1) => {
    return (
        <div flexDirection='row' alignItems='center' width='50%' height={$h}>
            <div flex={5} alignY='center' borderColor='#888888' borderWidth={2} backgroundColor='white' textAlign='center'>
                {rows[i]}
            </div> 
            <div onPress={() =^ {
                rows.RemoveAt(i);
                setRows(rows); 
            }} flex={1} align='center' borderColor='#888888' borderWidth={2} backgroundColor='white' textAlign='center'>
                -
            </div>
        </div>
    );
}", @"
const Slider = (
	Action^^float^ onChange,
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
}", @"
const Toggle = (
    Action^^bool^ onChange,
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
}" };
}

protected override HtmlNode cachedNode() {
	/*IMPORTS_DONE*/

HtmlNode CreateMove(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, string? text = null) {
	
	HtmlNode ___node = null;
	
const float c = 150;
float lastX = -1;
float width = 400;
	___node = newNode("div", props: new Dictionary<string, object> {["-width"]=((Func<int>)(() => ((int)(width)))), ["-backgroundColor"]=((Func<Color>)(() => (Color.Lerp(Color.Cyan, Color.Orange, (width - c) / (ScreenWidth-2*c))))), ["onMouseExit"]=((Action)(()=>lastX = -1)), ["onHover"]=((Action)(()=>{
             float x = mousePos.X;
             if (lastX != -1) width = Math.Clamp(width + (x - lastX), c, ScreenWidth-c);
             lastX = x;
        }))}, textContent: (Func<string>)(()=> ""+(text)+""));
	return ___node;
}

HtmlNode CreateContainer(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, List<string>? init = null) {
	
	HtmlNode ___node = null;
	
List<string> rows = init;
Action<List<string>> setRows = (___val) => {
	rows = ___val;
	___node.stateChangeDown();
};

	___node = newNode("div", props: new Dictionary<string, object> {["alignX"]="center", ["alignY"]="flexStart", ["width"]="100%"}, childrenFunc: (Func<HtmlNode[]>) (() => nodeArr((rows.Select((str, i) =>
                CreateRow("Row", props: new Dictionary<string, object> {["rows"]=(rows), ["setRows"]=(setRows), ["i"]=(i)}, textContent: "", rows: (rows), setRows: (setRows), i: (i))
            ).ToArray()), newNode("div", props: new Dictionary<string, object> {["onPress"]=((Action)(()=>{
                rows.Add($"new {random()}");
                setRows(rows);
            })), ["textAlign"]="center", ["width"]="50%", ["height"]=(((System.Int32)___vars["h"])), ["backgroundColor"]="white", ["borderColor"]="#888888", ["borderWidth"]=(2)}, textContent: "+"))));
	return ___node;
}

HtmlNode CreateRow(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, List<string>? rows = null, Action<List<string>>? setRows = null, int i = -1) {
	
	HtmlNode ___node = null;
	
	___node = newNode("div", props: new Dictionary<string, object> {["flexDirection"]="row", ["alignItems"]="center", ["width"]="50%", ["height"]=(((System.Int32)___vars["h"]))}, children: nodeArr(newNode("div", props: new Dictionary<string, object> {["flex"]=(5), ["alignY"]="center", ["borderColor"]="#888888", ["borderWidth"]=(2), ["backgroundColor"]="white", ["textAlign"]="center"}, textContent: (Func<string>)(()=> ""+(rows[i])+"")), newNode("div", props: new Dictionary<string, object> {["onPress"]=((Action)(()=>{
                rows.RemoveAt(i);
                setRows(rows); 
            })), ["flex"]=(1), ["align"]="center", ["borderColor"]="#888888", ["borderWidth"]=(2), ["backgroundColor"]="white", ["textAlign"]="center"}, textContent: "-")));
	return ___node;
}

HtmlNode CreateSlider(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, Action<float>? onChange = null, object? ____back = null, object? ____front = null, object? ____width = null, object? ____height = null, float init = 0F) {
	object back = ____back ?? "darkgray";
object front = ____front ?? "lightgray";
object width = ____width ?? 100;
object height = ____height ?? 30;

	HtmlNode ___node = null;
	
HtmlNode node = null;
float amount = init, lastAmount = amount;
void toMouse() {
amount = Math.Clamp((mousePos.X-node.PaddedX)/node.PaddedWidth, 0F, 1F);
if (amount != lastAmount) onChange?.Invoke(amount);
lastAmount = amount;
}
	___node = newNode("div", props: new Dictionary<string, object> {["ref"]=((Action<HtmlNode>)((HtmlNode el)=>node=el)), ["onPress"]=((Action)(()=>{
			toMouse();
		})), ["onMouseDrag"]=((Action)(()=>{
			if (node.clicked) toMouse();
		})), ["width"]=(width), ["height"]=(height), ["backgroundColor"]=(back)}, children: nodeArr(newNode("div", props: new Dictionary<string, object> {["backgroundColor"]=(front), ["-width"]=((Func<int>)(() => ((int)(node.PaddedWidth * amount)))), ["height"]="100%"}, textContent: "")));
	return ___node;
}

HtmlNode CreateToggle(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, Action<bool>? onChange = null, object? ____back = null, object? ____front = null, object? ____width = null, object? ____height = null) {
	object back = ____back ?? "darkgray";
object front = ____front ?? "lightgray";
object width = ____width ?? 100;
object height = ____height ?? 50;

	HtmlNode ___node = null;
	
bool val = false;
Action<bool> setVal = (___val) => {
	val = ___val;
	___node.stateChangeDown();
};

	___node = newNode("span", props: new Dictionary<string, object> {["onPress"]=((Action)(()=>{
                setVal(!val);
                onChange?.Invoke(val);
              })), ["backgroundColor"]=(back), ["width"]=(width), ["height"]=(height), ["borderRadius"]="50%", ["padding"]=(10)}, childrenFunc: (Func<HtmlNode[]>) (() => nodeArr((val ? newNode("div", props: new Dictionary<string, object> {["flex"]=(1)}, textContent: "") : null), newNode("div", props: new Dictionary<string, object> {["dimens"]=(height), ["backgroundColor"]=(front), ["borderRadius"]="50%"}, textContent: ""), (val ? null : newNode("div", props: new Dictionary<string, object> {["flex"]=(1)}, textContent: "")))));
	return ___node;
}
HtmlNode node = newNode("body", children: nodeArr(CreateMove("Move", props: new Dictionary<string, object> {["text"]="helo"}, textContent: "", text: "helo"), CreateMove("Move", props: new Dictionary<string, object> {["text"]="henlo"}, textContent: "", text: "henlo"), CreateMove("Move", props: new Dictionary<string, object> {["text"]=(""+((System.Int32)___vars["h"]))}, textContent: "", text: (""+((System.Int32)___vars["h"]))), CreateMove("Move", props: new Dictionary<string, object> {["text"]="henlo"}, textContent: "", text: "henlo"), CreateMove("Move", props: new Dictionary<string, object> {["text"]="helo"}, textContent: "", text: "helo"), CreateSlider("Slider", props: new Dictionary<string, object> {["back"]="red", ["front"]="green", ["width"]=(700), ["height"]="10%", ["init"]=(0.5F)}, textContent: "", ____back: "red", ____front: "green", ____width: (700), ____height: "10%", init: (0.5F)), CreateSlider("Slider", props: new Dictionary<string, object> {["onChange"]=((Action<float>)((float amount) =>Logger.log(amount)))}, textContent: "", onChange: ((Action<float>)((float amount) =>Logger.log(amount)))), CreateToggle("Toggle", textContent: ""), CreateToggle("Toggle", props: new Dictionary<string, object> {["back"]="white", ["front"]="green", ["onChange"]=((Action<bool>)((bool val)=>Logger.log($"flipped to {val}!")))}, textContent: "", ____back: "white", ____front: "green", onChange: ((Action<bool>)((bool val)=>Logger.log($"flipped to {val}!"))))));
setupNode(node);
return node;
}/*CACHE_END*/
	}
}