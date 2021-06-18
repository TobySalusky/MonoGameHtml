﻿namespace MonoGameHtml {
	public static class HtmlCache {

		public static bool IsCached(string[] input) { 
			if (input == null || input.Length == 0) return false;
			
			string[] cachedInput = StatePack.CacheData.CachedInput();
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
			return StatePack.CacheData.CachedNode();
		}

		public static void CacheHtml(string[] input, string outputCode) {
			string[] cachedInput = StatePack.CacheData.CachedInput();

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
			string path = StatePack.StatePackAbsolutePath();
			string text = await System.IO.File.ReadAllTextAsync(path);

			string inputArrString = "";
			for (int i = 0; i < input.Length; i++) {
				inputArrString += ((i != 0) ? ", " : "") + $"@\"{input[i]}\"";
			}

			inputArrString = $"return new string[]{{ {inputArrString} }};";
			
			const string startComment =  "/*CACHE_START*/", endComment = "/*CACHE_END*/";
			text = text.Substring(0, text.IndexOf(startComment) + startComment.Length) +
			       @$"
		public static class CacheData {{

			public static string[] CachedInput() {{
				{inputArrString}
			}}

			public static HtmlNode CachedNode() {{
				{outputCode}
			}}
		}}
"
			       +
			       text.Substring(text.IndexOf(endComment));
			
			await System.IO.File.WriteAllTextAsync(path, text);
		}
	}
}