

using System;
using System.Collections.Generic;
using System.Linq;
using MonoGameHtml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Example;

namespace MonoGameHtmlGeneratedCode.Inventory {
	public class Cache_Inventory : StatePack {
	public Cache_Inventory(params object[] initialVariableNamesAndObjects) : base(initialVariableNamesAndObjects) {}
		protected override string[] cachedInput() {
			return new string[]{ @"<App></App>", @"const App = () => {

    RerenderDiff(() => $inventory.Open);
    
    return (
        <body>
            {$inventory.Open ?
                <div>
                    <SlotSet slots={$inventory.Slots} ></SlotSet>
                    <KeyHint key='i'>to close inventory</KeyHint>
                </div> :
                <div class='BottomBar'>
                    <div>
                        <KeyHint key='i'>to open inventory</KeyHint>
                        <SlotSet slots={new []{$inventory.Slots[0]}} interactive={false} ></SlotSet>
                    </div>
                </div>
            }

            {$inventory.Open ? null : <h2>[Insert game here]</h2>}
        </body>
    );
}", @"

const SlotSet = (InventorySlot[][] slots, bool interactive = true) => {

    Vector2 offsetOnClick = Vector2.Zero;

    var handSlot = new InventorySlot(null, 0);

    var clickSlot = (InventorySlot slot, Vector2 slotPos) => {
        offsetOnClick = mousePos - slotPos;
        handSlot.SwapWith(slot);
        ___node?.stateChangeDown();
    };

    return (
        <div props={props}>
            <div class='Inventory'>
                {slots.map(slotRow =>
                    <span>
                        {slotRow.map(slot =>
                            <Slot slot={slot} clicked={interactive ? clickSlot : null}></Slot>
                        )}
                    </span>
                )}
            </div>
            {
                <Slot slot={handSlot} bg={false} position='fixed'  -pos={Vector2: mousePos - offsetOnClick} ></Slot>
            }
        </div>
    );
}", @"

const Slot = (InventorySlot slot, Action<InventorySlot,Vector2> clicked = null, bool bg = true) => {

    HtmlNode node = null;
    node = (
        <div class={new[] {'Slot', bg ? 'SlotBackground' : null}} props={props} onPress={()=>clicked?.Invoke(slot, new Vector2(node.x, node.y))}>
            { slot.ItemName == null ? null :
                <div dimens='100%'>
                    <img dimens='100%' src={$textures[slot.ItemName]}></img>
                    <h6 class='ItemNum'>{(slot.ItemCount != 1) ? slot.ItemCount : ''}</h6>
                </div>
            }
        </div>
    );

    return (node);
}", @"

const KeyHint = (string key) => {
    return (
        <span alignY='center'>
            <p class='Key' -fontSize={int~: 24 + 3 * sin(timePassed * 5F)}>{key}</p>
            <h6 class='KeyMargin'>{textContent}</h6>
        </span>
    );
}" };
		}

		protected override HtmlNode cachedNode() {
			/*IMPORTS_DONE*/
HtmlNode CreateApp(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, Func<HtmlNode[]> childrenFunc = null) {
	
	HtmlNode ___node = null;
	
var ___rerenderDiffs = new List<Action>();
void RerenderDiff<T>(Func<T> func) {{
	T init = func();
	___rerenderDiffs.Add(() => {{
		T newVal = func();
		if (!Equals(newVal, init)) {{
			init = newVal;
			___node.stateChangeDown();
		}}
	}});
}};
	

	
RerenderDiff(() => ((Example.Inventory)___vars["inventory"]).Open);
;
	___node = newNode("body", props: new Dictionary<string, object> {}, childrenFunc: (Func<HtmlNode[]>) (() => nodeArr((((Example.Inventory)___vars["inventory"]).Open ?
                newNode("div", props: new Dictionary<string, object> {}, children: nodeArr(CreateSlotSet("SlotSet", props: new Dictionary<string, object> {["slots"]=((Example.Inventory)___vars["inventory"]).Slots}, children: null, childrenFunc: null, textContent: "", slots: ((Example.Inventory)___vars["inventory"]).Slots), CreateKeyHint("KeyHint", props: new Dictionary<string, object> {["key"]="i"}, children: null, childrenFunc: null, textContent: "to close inventory", key: "i"))) :
                newNode("div", props: new Dictionary<string, object> {["class"]="BottomBar"}, children: nodeArr(newNode("div", props: new Dictionary<string, object> {}, children: nodeArr(CreateKeyHint("KeyHint", props: new Dictionary<string, object> {["key"]="i"}, children: null, childrenFunc: null, textContent: "to open inventory", key: "i"), CreateSlotSet("SlotSet", props: new Dictionary<string, object> {["slots"]=new []{((Example.Inventory)___vars["inventory"]).Slots[0]}, ["interactive"]=false}, children: null, childrenFunc: null, textContent: "", slots: new []{((Example.Inventory)___vars["inventory"]).Slots[0]}, interactive: false)))))
            ), (((Example.Inventory)___vars["inventory"]).Open ? null : newNode("h2", props: new Dictionary<string, object> {}, textContent: "[Insert game here]")))));
	
if (___node != null) {
	foreach (Action ___rerenderDiff in ___rerenderDiffs) {
		___node.bindAction(___rerenderDiff);
	}
}

	return ___node;
}

HtmlNode CreateSlotSet(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, Func<HtmlNode[]> childrenFunc = null, InventorySlot[][]? slots = null, bool interactive = true) {
	
	HtmlNode ___node = null;
		

	
Vector2 offsetOnClick = Vector2.Zero;
var handSlot = new InventorySlot(null, 0);
var clickSlot = (Action<InventorySlot, Vector2>)((slot, slotPos)=>{
        offsetOnClick = mousePos - slotPos;
        handSlot.SwapWith(slot);
        ___node?.stateChangeDown();
    });
;
	___node = newNode("div", props: new Dictionary<string, object> {["props"]=props}, childrenFunc: (Func<HtmlNode[]>) (() => nodeArr(newNode("div", props: new Dictionary<string, object> {["class"]="Inventory"}, childrenFunc: (Func<HtmlNode[]>) (() => nodeArr((slots.Select(slotRow =>
                    newNode("span", props: new Dictionary<string, object> {}, childrenFunc: (Func<HtmlNode[]>) (() => nodeArr((slotRow.Select(slot =>
                            CreateSlot("Slot", props: new Dictionary<string, object> {["slot"]=slot, ["clicked"]=interactive ? clickSlot : null}, children: null, childrenFunc: null, textContent: "", slot: slot, clicked: interactive ? clickSlot : null)
                        ).ToArray()))))
                ).ToArray())))), (
                CreateSlot("Slot", props: new Dictionary<string, object> {["slot"]=handSlot, ["bg"]=false, ["position"]="fixed", ["-pos"]=(Func<Vector2>)(() => mousePos - offsetOnClick)}, children: null, childrenFunc: null, textContent: "", slot: handSlot, bg: false)
            ))));
	
	return ___node;
}

HtmlNode CreateSlot(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, Func<HtmlNode[]> childrenFunc = null, InventorySlot? slot = null, Action<InventorySlot,Vector2> clicked = null, bool bg = true) {
	
	HtmlNode ___node = null;
		

	
HtmlNode node = null;
node = (
        newNode("div", props: new Dictionary<string, object> {["class"]=new[] {"Slot", bg ? "SlotBackground" : null}, ["props"]=props, ["onPress"]=(Action)(()=>clicked?.Invoke(slot, new Vector2(node.x, node.y)))}, childrenFunc: (Func<HtmlNode[]>) (() => nodeArr(( slot.ItemName == null ? null :
                newNode("div", props: new Dictionary<string, object> {["dimens"]="100%"}, children: nodeArr(newNode("img", props: new Dictionary<string, object> {["dimens"]="100%", ["src"]=((System.Collections.Generic.Dictionary<System.String,Microsoft.Xna.Framework.Graphics.Texture2D>)___vars["textures"])[slot.ItemName]}, textContent: ""), newNode("h6", props: new Dictionary<string, object> {["class"]="ItemNum"}, textContent: (Func<string>)(()=> ""+((slot.ItemCount != 1) ? slot.ItemCount : "")+""))))
            ))))
    );
;
	___node = node;
	
	return ___node;
}

HtmlNode CreateKeyHint(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, Func<HtmlNode[]> childrenFunc = null, string? key = null) {
	
	HtmlNode ___node = null;
		

	
;
	___node = newNode("span", props: new Dictionary<string, object> {["alignY"]="center"}, children: nodeArr(newNode("p", props: new Dictionary<string, object> {["class"]="Key", ["-fontSize"]=(Func<int>)(() => (int)(24 + 3 * sin(timePassed * 5F)))}, textContent: (Func<string>)(()=> ""+(key)+"")), newNode("h6", props: new Dictionary<string, object> {["class"]="KeyMargin"}, textContent: (Func<string>)(()=> ""+(textContent)+""))));
	
	return ___node;
}
HtmlNode node = CreateApp("App", props: new Dictionary<string, object> {}, children: null, childrenFunc: null, textContent: "");
setupNode(node);
return node;
		}
	}
}
