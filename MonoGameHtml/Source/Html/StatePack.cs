using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;

namespace MonoGameHtml {
	public class StatePack {

		public static Dictionary<string, object> ___vars;
		public static Dictionary<string, string> ___types;

		public static float timePassed, deltaTime;
		
		public static Func<float, float> sin = (rad) => (float) Math.Sin(rad);
		public static Func<float, float> cos = (rad) => (float) Math.Cos(rad);

		public static Vector2 mousePos;

		public static int ScreenWidth => HtmlMain.screenWidth;
		public static int ScreenHeight => HtmlMain.screenHeight;
		
		public static float random(float max = 1F) {
			return Util.random(max);
		}

		public static float random(float min, float max) {
			return Util.random(min, max);
		}
		
		public StatePack(params object[] varList) {
			___vars = new Dictionary<string, object>();
			___types = new Dictionary<string, string>();
			
			if (varList.Length < 2) return;
			
			Logger.log("Global Variables=========");
			for (int i = 0; i < varList.Length; i += 2) {
				object obj = varList[i + 1];
				
				string name = (string) varList[i];
				string type = obj.GetType().ToString();

				type = type.Replace("]", ">"); // for functions and such
				type = Regex.Replace(type, @"`[0-9]*\[", "<");
				type = type.Replace("[>", "[]");

				___vars[name] = obj;
				___types[name] = type;
				
				Logger.log($"\t\"{name}\" ({type}): {obj}");
			}
			Logger.log("===================");
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
	}
}