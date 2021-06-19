﻿using System.Collections.Generic;
 using System.Linq;

 namespace MonoGameHtml {
	public static class HtmlComponents {
		public static string[] Create(params string[] componentFileContents) {
			List<string> componentStrings = new List<string>();
			
			// extract multiple components from single strings
			foreach (string componentFile in componentFileContents) {
				
				var bracketPairs = DelimPair.genPairs(componentFile, DelimPair.CurlyBrackets).Where(pair => pair.nestCount == 0).ToArray();

				for (int i = 0; i < bracketPairs.Length; i++) {
					string component = componentFile.sub((i == 0) ? 0 : bracketPairs[i - 1].AfterClose, bracketPairs[i].AfterClose);
					componentStrings.Add(component);
				}
			}
			
			return componentStrings.ToArray();
		}
	}
}