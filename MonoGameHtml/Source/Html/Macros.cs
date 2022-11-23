﻿using System.Collections.Generic;
using System.Linq;

namespace MonoGameHtml {
	public static class Macros {

		public static string[] defaults = {
			"text", "<p>{textContent}</p>",
			"time", "timePassed",
			"t", "timePassed",
			"deltaTime", "deltaTime",
			"dt", "deltaTime",
			"mp", "mousePos",
			"r", "random()",
			"setRef(varName)", "(HtmlNode ___refNode)=>$$varName=___refNode",
			"set(type, varName)", "($$type ___setTemp)=>$$varName=___setTemp",
			"fill", "dimens='100%'",
			"bg(str)", "backgroundColor='$$str'",
			"col(str)", "color='$$str'",
			"size(str)", "dimens={$$str}",
			"contents", "(childrenFunc != null ? childrenFunc() : children)",
			"1", "(childrenFunc != null ? childrenFunc() : children) != null ? (childrenFunc != null ? childrenFunc() : children) : (new HtmlNode[]{<p>{textContent}</p>})",
			"inner", "((childrenFunc != null ? childrenFunc() : children) ?? (new HtmlNode[]{<p>{textContent}</p>}))",
			"Rerender()", "___node?.stateChangeDown()"
		};
		
		public static Dictionary<string, string> create(params string[] macroList) {
			macroList = macroList.Concat(defaults).ToArray();
			Dictionary<string, string> macros = new();
			for (int i = 0; i < macroList.Length; i += 2) {
				string macroID = macroList[i];
				string value = macroList[i + 1];

				macros[macroID] = value;
			}
			
			return macros;
		}
	}
}