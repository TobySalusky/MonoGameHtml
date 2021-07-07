﻿using System.Collections.Generic;
using System.Linq;

namespace MonoGameHtml {
	public static class Macros {

		public static string[] defaults = {
			"time", "timePassed",
			"t", "timePassed",
			"deltaTime", "deltaTime",
			"dt", "deltaTime",
			"mp", "mousePos",
			"r", "random()",
			"setRef(varName)", "(HtmlNode ___refNode)=^$$varName=___refNode",
			"set(type, varName)", "($$type ___setTemp)=^$$varName=___setTemp",
		};
		
		public static Dictionary<string, string> create(params string[] macroList) {
			macroList = macroList.Concat(defaults).ToArray();
			Dictionary<string, string> macros = new Dictionary<string, string>();
			
			for (int i = 0; i < macroList.Length; i += 2) {
				string macroID = macroList[i];
				string value = macroList[i + 1];

				macros[macroID] = value;
			}

			return macros;
		}
	}
}