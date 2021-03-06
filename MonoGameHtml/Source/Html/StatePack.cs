using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using MonoGameHtml.MainMethod;

namespace MonoGameHtml {
	public class StatePack {
		
		public Dictionary<string, object> ___vars;
		public Dictionary<string, string> ___types; // types aren't really necessary after compilation TODO: do something about this?

		public float timePassed, deltaTime;
		
		public static Func<float, float> sin = (rad) => (float) Math.Sin(rad);
		public static Func<float, float> cos = (rad) => (float) Math.Cos(rad);

		// input
		public static Vector2 mousePos;
		public static KeyInfo keys;
		

		public static int ScreenWidth => HtmlMain.screenWidth;
		public static int ScreenHeight => HtmlMain.screenHeight;

		public static Game game => HtmlMain.game;

		// ReSharper disable once UnusedMember.Global
		public static float random(float max = 1F) {
			return Util.random(max);
		}
		
		// ReSharper disable once UnusedMember.Global
		public static float random(float min, float max) {
			return Util.random(min, max);
		}

		/* Basically an override for the constructor that makes use of the cache */
		public static StatePack Create(params object[] initialVariableNamesAndObjects) {
			// uses cached StatePack-type if it exists
			Type type = Assembly.GetEntryAssembly()!.GetType($"{HtmlCache.CacheNamespace()}.{HtmlCache.CacheClassName()}");
			if (type == null) type = typeof(StatePack);

			return (StatePack) Activator.CreateInstance(type, initialVariableNamesAndObjects);
		}

		public StatePack(params object[] initialVariableNamesAndObjects) {
			___vars = new Dictionary<string, object>();
			___types = new Dictionary<string, string>();
			
			if (initialVariableNamesAndObjects.Length < 2) return;
			
			Logger.logColor(ConsoleColor.Green, HtmlOutput.OUTPUT_GLOBALS);
			for (int i = 0; i < initialVariableNamesAndObjects.Length; i += 2) {
				object obj = initialVariableNamesAndObjects[i + 1];
				
				string name = (string) initialVariableNamesAndObjects[i];
				string type = TypeString(obj);
				___vars[name] = obj;
				___types[name] = type;
				
				Logger.log($"\t\"{name}\" ({type}): {obj}");
			}
			Logger.logColor(ConsoleColor.Green, HtmlOutput.OUTPUT_END);
		}

		protected internal virtual HtmlNode cachedNode() {
			return null;
		}

		protected internal virtual string[] cachedInput() {
			return null;
		}

		private static string TypeString(object obj) {
			string type = obj.GetType().ToString();
			
			type = type.Replace("]", ">"); // for functions and such
			type = Regex.Replace(type, @"`[0-9]*\[", "<");
			type = type.Replace("[>", "[]");

			return type;
		}

		public void ClearVars() { 
			___vars = new Dictionary<string, object>();
			___types = new Dictionary<string, string>();
		}
		
		public void SetVar(string varName, object obj) { 
			SetVars(varName, obj);
		}

		public void SetVars(params object[] namesAndObjects) {
			for (int i = 0; i < namesAndObjects.Length; i += 2) {
				object obj = namesAndObjects[i + 1];
				
				string name = (string) namesAndObjects[i];
				string type = TypeString(obj);
				
				___vars[name] = obj;
				___types[name] = type;
			}
		}

		/// <summary>
		/// Updates a single variable.
		/// PreReq: Variable used in Create or SetVar(s).
		/// </summary>
		/// <param name="varName">name of variable</param>
		/// <param name="obj">variable of any type</param>
		public void UpdateVar(string varName, object obj) { 
			UpdateVars(varName, obj);
		}
		
		/// <summary>
		/// Updates multiple variables.
		/// PreReq: Variables used in Create or SetVar(s).
		/// </summary>
		/// <param name="namesAndObjects">Alternating name (string) and corresponding object</param>
		public void UpdateVars(params object[] namesAndObjects) { 
			for (int i = 0; i < namesAndObjects.Length; i += 2) {
				object obj = namesAndObjects[i + 1];
				string name = (string) namesAndObjects[i];
				___vars[name] = obj;
			}
		}

		public static HtmlNode newNode(string tag, Dictionary<string, object> props = null, object textContent = null, 
			HtmlNode[] children = null, Func<HtmlNode[]> childrenFunc = null) { 
			
			return new HtmlNode(tag, props, textContent, children, childrenFunc);
		}

		public static HtmlNode[] nodeArr(params object[] objs) {

			List<HtmlNode> nodes = new List<HtmlNode>();

			if (objs == null) return new HtmlNode[0];
			
			foreach (var elem in objs) {
				if (elem == null) continue;
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
		
		/*public static HtmlNode[] nodeArr(params HtmlNode[] nodes) {
			//return nodes.Where(node => node != null).ToArray();
			return nodes;
		}*/

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