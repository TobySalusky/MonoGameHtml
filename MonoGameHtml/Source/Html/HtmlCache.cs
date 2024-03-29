﻿﻿using System;
 using System.IO;
 using System.Linq;

 namespace MonoGameHtml {
	internal static class HtmlCache { // TODO: hash input

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

		internal static string CacheNamespace() {
			return
				$"MonoGameHtmlGeneratedCode{(HtmlMain.cacheIdentifier != null ? $".{HtmlMain.cacheIdentifier}" : "")}";
		}
		
		internal static string CacheClassName() {
			return
				$"Cache{(HtmlMain.cacheIdentifier != null ? $"_{HtmlMain.cacheIdentifier}" : "")}";
		}

		public static async void UpdateCache(string[] input, string fullCode) {
			
			string imports = fullCode.Substring(0, fullCode.indexOf("/*IMPORTS_DONE*/"));
			string outputCode = fullCode.Substring(fullCode.indexOf("/*IMPORTS_DONE*/"));

			string path = Path.Join(HtmlMain.cachePath, $"MonoGame{CacheClassName()}.cs");

			// var allInput = input.Concat(new []{imports}).ToArray(); TODO: CACHE IMPORTS

			string inputArrString = "";
			for (int i = 0; i < input.Length; i++) {
				inputArrString += ((i != 0) ? ", " : "") + $"@\"{input[i]}\"";
			}

			inputArrString = $"return new string[]{{ {inputArrString} }};";

			string fileText = @$"
{imports}

namespace {CacheNamespace()} {{
	public class {CacheClassName()} : StatePack {{
	public {CacheClassName()}(params object[] initialVariableNamesAndObjects) : base(initialVariableNamesAndObjects) {{}}
		protected override string[] cachedInput() {{
			{inputArrString}
		}}

		protected override HtmlNode cachedNode() {{
			{outputCode}
		}}
	}}
}}
";

			try { 
				await File.WriteAllTextAsync(path, fileText);
			} catch (Exception e) {
				Warnings.log("FAILED TO WRITE CACHE FILE --skipping...");
				Console.WriteLine(e);
			}
		}
	}
 }