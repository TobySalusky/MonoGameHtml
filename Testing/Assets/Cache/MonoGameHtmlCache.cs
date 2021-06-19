using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGameHtml;

namespace MonoGameHtmlGeneratedCode {
	public class Cache : StatePack {
	/*CACHE_START*/
public static string[] CachedInput() {
	return new string[]{ @"
<body>
    <Move text='hello'/>
    <Move text='o/'/>
    <Move text='what is up my guy'/>
    <Move text='l:O'/>
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
}" };
}

public static HtmlNode CachedNode() {
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
HtmlNode node = newNode("body", children: nodeArr(CreateMove("Move", props: new Dictionary<string, object> {["text"]="hello"}, textContent: "", text: "hello"), CreateMove("Move", props: new Dictionary<string, object> {["text"]="o/"}, textContent: "", text: "o/"), CreateMove("Move", props: new Dictionary<string, object> {["text"]="what is up my guy"}, textContent: "", text: "what is up my guy"), CreateMove("Move", props: new Dictionary<string, object> {["text"]="l:O"}, textContent: "", text: "l:O")));
setupNode(node);
return node;
}/*CACHE_END*/
	}
}