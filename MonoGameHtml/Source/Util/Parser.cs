using System.Collections.Generic;

namespace MonoGameHtml {
	internal static class Parser {

		public static List<string> splitUnNestedCommas(this string str) { 
			List<string> list = new List<string>();

			int nextStart = 0;
			
			void next(int i) { 
				list.Add(str.sub(nextStart, i));
				nextStart = i+1;
			}

			var dict = DelimPair.searchAll(str,
				DelimPair.Parens, DelimPair.CurlyBrackets, DelimPair.SquareBrackets,
				DelimPair.Quotes, DelimPair.SingleQuotes, DelimPair.Carrots);

			for (int i = 0; i < str.Length; i++) {
				if (str.Substring(i, 1) == ",") {
					bool unNested = DelimPair.allNestOf(0, str.nestAmountsLen(i, 1, dict));
					if (unNested) {
						next(i);
					}
				}
			}
			next(str.Length);
			
			foreach (var s in list) {
				Logger.log("TEST:", s);
			}

			return list;
		}
		
		
	}
}