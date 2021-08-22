﻿using System;
 using System.IO;
 using System.Reflection;

 namespace MonoGameHtml {
	internal static class HtmlCache {

		public static bool IsCached(string[] input, StatePack pack) { 
			if (input == null || input.Length == 0) return false;
			
			string[] cachedInput = pack.cachedInput();
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

		public static void CacheHtml(string[] input, string outputCode, StatePack pack) {
			string[] cachedInput = pack.cachedInput();

			if (input == null || input.Length == 0) return;
			
			if (cachedInput == null || cachedInput.Length != input.Length) {
				try {
					UpdateCache(input, outputCode);
				}
				catch (Exception e) {
					Logger.log("FAILED TO CACHE!", e);
				}
			} else {
				for (int i = 0; i < input.Length; i++) {
					if (input[i] != cachedInput[i]) {
						try {
							UpdateCache(input, outputCode);
						}
						catch (Exception e) {
							Logger.log("FAILED TO CACHE!", e);
						}
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
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using MonoGameHtml;

namespace MonoGameHtmlGeneratedCode {{
	public class Cache : StatePack {{
	public Cache(params object[] initialVariableNamesAndObjects) : base(initialVariableNamesAndObjects) {{}}
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
protected override string[] cachedInput() {{
	{inputArrString}
}}

protected override HtmlNode cachedNode() {{
	{outputCode}
}}" + text.Substring(text.indexOf(endComment));

			try { 
				await File.WriteAllTextAsync(path, text.Trim());
			} catch (Exception e) {
				Warnings.log("FAILED TO WRITE CACHE FILE --skipping...");
				Console.WriteLine(e);
			}
		}
	}
 }