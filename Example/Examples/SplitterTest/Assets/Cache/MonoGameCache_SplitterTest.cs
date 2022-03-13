

using System;
using System.Collections.Generic;
using System.Linq;
using MonoGameHtml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace MonoGameHtmlGeneratedCode.SplitterTest {
	public class Cache_SplitterTest : StatePack {
	public Cache_SplitterTest(params object[] initialVariableNamesAndObjects) : base(initialVariableNamesAndObjects) {}
		protected override string[] cachedInput() {
			return new string[]{ @"<App></App>", @"

const PanelView = () => {

	HtmlNode[] contents = (childrenFunc != null ? childrenFunc() : children);
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
			<html></html>
		</span>
	);
}", @"


const Panel = (int minWidth = 20, float initFlex = 1F) => {

	return (
		<panel minWidth={minWidth} props={props} initFlex={initFlex} height='100%' class='BasePanel' getWidth={int: ___node.width} setWidth={(int newWidth) => ___node.width=newWidth}>
			{(childrenFunc != null ? childrenFunc() : children)}
			<html></html>
		</panel>
	);
}", @"


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
				dragStartX = mousePos.X;
			}}
			onMouseUp={() => dragging=false}
			
			onTick={()=> {
				if (!dragging) return;
				
				int fullDragX = (int) (mousePos.X - dragStartX);
				if (fullDragX != 0) {
					if (shiftFunc != null) {
						dragStartX += shiftFunc.Invoke(fullDragX);
					}
				}
			}}
		></splitter>
	);
}", @"const App = () => {
    return (
        <body>
            <PanelView>
                <Panel class='SidePanel'>
                    <p>hi</p>
                </Panel>
                <Splitter class='Splitter'></Splitter>
                <Panel class='MainPanel' initFlex={3F}>
                    <h3>yo</h3>
                </Panel>
                <Splitter class='Splitter'></Splitter>
                <Panel class='SidePanel'>
                    <h3>yo</h3>
                </Panel>
            </PanelView>
        </body>
    );
}" };
		}

		protected override HtmlNode cachedNode() {
			/*IMPORTS_DONE*/
HtmlNode CreatePanelView(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, Func<HtmlNode[]> childrenFunc = null) {
	
	HtmlNode ___node = null;
		

	
HtmlNode[] contents = (childrenFunc != null ? childrenFunc() : children);
float totalFlex = 0F;
Func<HtmlNode, int, int> adjustDiff = (panel, diff) => {
		if (diff >= 0) return diff;
		if (panel.prop<Func<int>>("getWidth").Invoke() + diff < panel.prop<int>("minWidth")) {
			return panel.prop<int>("minWidth") - panel.prop<Func<int>>("getWidth").Invoke();
		}
		return diff;
	};
	
	Action<HtmlNode, int> diffPanelWidth = (panel, diff) => {
		panel.prop<Action<int>>("setWidth").Invoke(panel.prop<Func<int>>("getWidth").Invoke() + diff);
	};
for (int i = 0; i < contents.Length; i++) {
		HtmlNode node = contents[i];
		if (i % 2 == 0) {
			if (node.tag != "panel") {
				throw new Exception($"element {i} of PanelView must be a panel");
			}
			totalFlex += node.prop<float>("initFlex");
			
		} else {
		
			if (node.tag != "splitter") {
				throw new Exception($"element {i} of PanelView must be a splitter");
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
			
			node.prop<Action<Func<int, int>>>("setShiftFunc")(shiftFunc);
		}
	}

	;
	___node = newNode("span", props: new Dictionary<string, object> {["dimens"]="100%", ["props"]=props, ["ref"]=(Action<HtmlNode>)((HtmlNode thisNode) => {
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
							panel.props["width"] = availSpace - spaceUsed;
						} else {
							int thisPanelWidth = (int) ((float) availSpace * (panel.prop<float>("initFlex") / totalFlex));
							panel.props["width"] = thisPanelWidth;
							spaceUsed += thisPanelWidth;
						}
					}
				}
				thisNode.triggerOnResize();
			})}, childrenFunc: (Func<HtmlNode[]>) (() => nodeArr((contents), newNode("html", props: new Dictionary<string, object> {}, textContent: ""))));
	
	return ___node;
}

HtmlNode CreatePanel(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, Func<HtmlNode[]> childrenFunc = null, int minWidth = 20, float initFlex = 1F) {
	
	HtmlNode ___node = null;
		

	
;
	___node = newNode("panel", props: new Dictionary<string, object> {["minWidth"]=minWidth, ["props"]=props, ["initFlex"]=initFlex, ["height"]="100%", ["class"]="BasePanel", ["getWidth"]=(Func<int>)(() => ___node.width), ["setWidth"]=(Action<int>)((int newWidth) => ___node.width=newWidth)}, childrenFunc: (Func<HtmlNode[]>) (() => nodeArr(((childrenFunc != null ? childrenFunc() : children)), newNode("html", props: new Dictionary<string, object> {}, textContent: ""))));
	
	return ___node;
}

HtmlNode CreateSplitter(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, Func<HtmlNode[]> childrenFunc = null) {
	
	HtmlNode ___node = null;
		

	
Func<int, int> shiftFunc = null;
bool mouseOver = false;
bool dragging = false;
float dragStartX = 0f;
;
	___node = newNode("splitter", props: new Dictionary<string, object> {["height"]="100%", ["class"]="BaseSplitter", ["props"]=props, ["setShiftFunc"]=(Action<Func<int,int>>)((Func<int,int> newShiftFunc) => shiftFunc = newShiftFunc), ["onMouseEnter"]=(Action)(()=>mouseOver=true), ["onMouseExit"]=(Action)(()=>mouseOver=false), ["onMouseDown"]=(Action)(()=>{
				if (!mouseOver) return;
				dragging = true;
				dragStartX = mousePos.X;
			}), ["onMouseUp"]=(Action)(()=>dragging=false), ["onTick"]=(Action)(()=>{
				if (!dragging) return;
				
				int fullDragX = (int) (mousePos.X - dragStartX);
				if (fullDragX != 0) {
					if (shiftFunc != null) {
						dragStartX += shiftFunc.Invoke(fullDragX);
					}
				}
			})}, textContent: "");
	
	return ___node;
}

HtmlNode CreateApp(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, Func<HtmlNode[]> childrenFunc = null) {
	
	HtmlNode ___node = null;
		

	
;
	___node = newNode("body", props: new Dictionary<string, object> {}, children: nodeArr(CreatePanelView("PanelView", props: new Dictionary<string, object> {}, children: nodeArr(CreatePanel("Panel", props: new Dictionary<string, object> {["class"]="SidePanel"}, children: nodeArr(newNode("p", props: new Dictionary<string, object> {}, textContent: "hi")), childrenFunc: null, textContent: null), CreateSplitter("Splitter", props: new Dictionary<string, object> {["class"]="Splitter"}, children: null, childrenFunc: null, textContent: ""), CreatePanel("Panel", props: new Dictionary<string, object> {["class"]="MainPanel", ["initFlex"]=3F}, children: nodeArr(newNode("h3", props: new Dictionary<string, object> {}, textContent: "yo")), childrenFunc: null, textContent: null, initFlex: 3F), CreateSplitter("Splitter", props: new Dictionary<string, object> {["class"]="Splitter"}, children: null, childrenFunc: null, textContent: ""), CreatePanel("Panel", props: new Dictionary<string, object> {["class"]="SidePanel"}, children: nodeArr(newNode("h3", props: new Dictionary<string, object> {}, textContent: "yo")), childrenFunc: null, textContent: null)), childrenFunc: null, textContent: null)));
	
	return ___node;
}
HtmlNode node = CreateApp("App", props: new Dictionary<string, object> {}, children: null, childrenFunc: null, textContent: "");
setupNode(node);
return node;
		}
	}
}
