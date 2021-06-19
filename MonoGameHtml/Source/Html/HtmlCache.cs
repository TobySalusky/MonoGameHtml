﻿using System;
 using System.IO;
 using System.Reflection;

 namespace MonoGameHtml {
	internal static class HtmlCache {
		
		public static string[] fetchInput() {
			Type type = Assembly.GetEntryAssembly()!.GetType("MonoGameHtmlGeneratedCode.Cache");
			var res = type!.GetMethod("CachedInput", BindingFlags.Public | BindingFlags.Static)!.Invoke(null, null);
			return (string[]) res;
		}
		
		public static HtmlNode fetchNode() {
			Type type = Assembly.GetEntryAssembly()!.GetType("MonoGameHtmlGeneratedCode.Cache");
			var res = type!.GetMethod("CachedNode", BindingFlags.Public | BindingFlags.Static)!.Invoke(null, null);
			return (HtmlNode) res;
		}

		public static bool IsCached(string[] input) { 
			if (input == null || input.Length == 0) return false;
			
			string[] cachedInput = fetchInput();
			if (cachedInput == null || cachedInput.Length != input.Length) { 
				return false;
			}
			
			for (int i = 0; i < input.Length; i++) {
				if (input[i] != cachedInput[i]) { 
					return false;
				}
			}

			return true;
		}

		public static HtmlNode UseCache() {
			return fetchNode();
		}

		public static void CacheHtml(string[] input, string outputCode) {
			string[] cachedInput = fetchInput();

			if (input == null || input.Length == 0) return;
			
			if (cachedInput == null || cachedInput.Length != input.Length) { 
				UpdateCache(input, outputCode);
			} else {
				for (int i = 0; i < input.Length; i++) {
					if (input[i] != cachedInput[i]) { 
						UpdateCache(input, outputCode);
					}
				}
			}
		}

		public static async void UpdateCache(string[] input, string outputCode) {
			string path = Path.Join(HtmlMain.cachePath, "MonoGameHtmlCache.cs");
			const string startComment =  "/*CACHE_START*/", endComment = "/*CACHE_END*/";
			
			if (!File.Exists(path)) {

				string initFileText = @$"
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGameHtml;

namespace MonoGameHtmlGeneratedCode {{
	public class Cache : StatePack {{
	{startComment}
	{endComment}
	}}
}}
";
				await File.WriteAllTextAsync(path, initFileText);
			}

			string text = await File.ReadAllTextAsync(path);

			string inputArrString = "";
			for (int i = 0; i < input.Length; i++) {
				inputArrString += ((i != 0) ? ", " : "") + $"@\"{input[i]}\"";
			}

			inputArrString = $"return new string[]{{ {inputArrString} }};";
			
			text = text.Substring(0, text.indexOf(startComment) + startComment.Length) + @$"
public static string[] CachedInput() {{
	{inputArrString}
}}

public static HtmlNode CachedNode() {{
	{outputCode}
}}" + text.Substring(text.indexOf(endComment));
			
			await File.WriteAllTextAsync(path, text.Trim());
		}
	}
}