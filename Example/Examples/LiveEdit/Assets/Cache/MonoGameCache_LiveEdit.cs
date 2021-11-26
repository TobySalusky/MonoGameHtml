

using System;
using System.Collections.Generic;
using System.Linq;
using MonoGameHtml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace MonoGameHtmlGeneratedCode.LiveEdit {
	public class Cache_LiveEdit : StatePack {
	public Cache_LiveEdit(params object[] initialVariableNamesAndObjects) : base(initialVariableNamesAndObjects) {}
		protected override string[] cachedInput() {
			return new string[]{ @"<App></App>", @"const App = () => {
    int count = 11;

    return (
        <body>
            <h3 class='Counter' onPress={()=>count++}>{count}</h3>
        </body>
    );
}" };
		}

		protected override HtmlNode cachedNode() {
			/*IMPORTS_DONE*/
HtmlNode CreateApp(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, Func<HtmlNode[]> childrenFunc = null) {
	
	HtmlNode ___node = null;
		

	
int count = 11;
;
	___node = newNode("body", props: new Dictionary<string, object> {}, children: nodeArr(newNode("h3", props: new Dictionary<string, object> {["class"]="Counter", ["onPress"]=(Action)(()=>count++)}, textContent: (Func<string>)(()=> ""+(count)+""))));
	
	return ___node;
}
HtmlNode node = CreateApp("App", props: new Dictionary<string, object> {}, children: null, childrenFunc: null, textContent: "");
setupNode(node);
return node;
		}
	}
}
