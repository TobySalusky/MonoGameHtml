using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace MonoGameHtml {
	public class StatePack {

		public static Dictionary<string, object> ___vars;
		public static Dictionary<string, string> ___types;

		public static float timePassed, deltaTime;
		
		public static Func<float, float> sin = (rad) => (float) Math.Sin(rad);
		public static Func<float, float> cos = (rad) => (float) Math.Cos(rad);

		public static float random(float max = 1F) {
			return Util.random(max);
		}
		
		public static float random(float min, float max) {
			return Util.random(min, max);
		}
		
		public StatePack(params object[] varList) {
			___vars = new Dictionary<string, object>();
			___types = new Dictionary<string, string>();
			for (int i = 0; i < varList.Length; i += 2) {
				object obj = varList[i + 1];
				
				string name = (string) varList[i];
				string type = obj.GetType().ToString();

				type = type.Replace("]", ">"); // for functions and such
				type = Regex.Replace(type, @"`[0-9]*\[", "<");
				type = type.Replace("[>", "[]");

				___vars[name] = obj;
				___types[name] = type;
				
				Logger.log($"{name}({type}): {obj}");
			}
		}

		public static HtmlNode newNode(string tag, Dictionary<string, object> props = null, object textContent = null, 
			HtmlNode[] children = null, Func<HtmlNode[]> childrenFunc = null) { 
			
			return new HtmlNode(tag, props, textContent, children, childrenFunc);
		}

		public static HtmlNode[] nodeArr(params object[] objs) {

			List<HtmlNode> nodes = new List<HtmlNode>();
			
			foreach (var elem in objs) {
				switch (elem) {
					case HtmlNode node:
						nodes.Add(node);
						break;
					case IEnumerable<HtmlNode> nodeArr:
						nodes.AddRange(nodeArr);
						break;
				}
			}
			return nodes.ToArray();
		}
		
		public static HtmlNode[] nodeArr(params HtmlNode[] nodes) {
			return nodes;
		}

		public static int[] nStream(int n) { 
			int[] arr = new int[n];

			for (int i = 0; i < n; i++) {
				arr[i] = i;
			}

			return arr;
		}

		public static void setupNode(object node) {
			HtmlNode htmlNode = ((HtmlNode) node);
			htmlNode.topDownInit();
			htmlNode.bottomUpInit();
			htmlNode.layoutDown();
		}

		public static string StatePackAbsolutePath() {
			return FileUtil.TraceFilePath();
		}

		/*CACHE_START*/
		public static class CacheData {

			public static string[] CachedInput() {
				return new string[]{ @"
<div flexDirection='row' dimens='100%' alignX='center' alignY='flexStart'>

    <div dimens={500} backgroundColor='red'>Hello</div>
    
    {$array.map((name, i) => 
        <Test name={name} number={i}/>
    )}
</div>
", @"
const Test = (string name, int number) => {

    return (
        <h3 backgroundColor='green' borderRadius='50%' borderColor='#FFFFFF'  borderWidth={5}>
            {name}: {number}
        </h3>
    );
}
" };
			}

			public static HtmlNode CachedNode() {
				/*IMPORTS_DONE*/

HtmlNode CreateTest(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, string? name = null, int? number = null) {
	HtmlNode ___node = null;
	
	___node = newNode("h3", props: new Dictionary<string, object> {["backgroundColor"]="green", ["borderRadius"]="50%", ["borderColor"]="#FFFFFF", ["borderWidth"]=(5)}, textContent: (Func<string>)(()=> ""+(name)+": "+(number)+""));
	return ___node;
}
HtmlNode node = newNode("div", props: new Dictionary<string, object> {["flexDirection"]="row", ["dimens"]="100%", ["alignX"]="center", ["alignY"]="flexStart"}, childrenFunc: (Func<HtmlNode[]>) (() => nodeArr(newNode("div", props: new Dictionary<string, object> {["dimens"]=(500), ["backgroundColor"]="red"}, textContent: "Hello"), (((System.String[])___vars["array"]).Select((name, i) => 
        CreateTest("Test", props: new Dictionary<string, object> {["name"]=(name), ["number"]=(i)}, textContent: "", name: (name), number: (i))
    ).ToArray()))));
setupNode(node);
return node;
			}
		}
/*CACHE_END*/
	}
}