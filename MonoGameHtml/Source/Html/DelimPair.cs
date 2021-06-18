﻿﻿using System.Collections.Generic;
using System.Linq;

namespace MonoGameHtml {
		public class DelimPair { // TODO: gen ordered list
		
		public int openIndex, openLen;
		public int closeIndex, closeLen;
		public int nestCount;

		public int AfterClose => closeIndex + closeLen;

		public static readonly (string, string) Parens = ("(", ")"), Carrots = ("<", ">"),
			SquareBrackets = ("[", "]"), CurlyBrackets = ("{", "}"), 
			Quotes = ("\"", "\""), SingleQuotes = ("'", "'");
		
		public DelimPair(int openIndex, int closeIndex, int openLen = 1, int closeLen = 1, int nestCount = 0) {
			this.openIndex = openIndex;
			this.closeIndex = closeIndex;
			this.openLen = openLen;
			this.closeLen = closeLen;
			this.nestCount = nestCount;
		}

		public static bool nestsAll(Dictionary<(string, string), int> dict, int target) {
			foreach (var key in dict.Keys) {
				if (dict[key] != target) return false;
			}

			return true;
		}

		public static bool allNestOf(int targetNest, Dictionary<(string, string), int> dict) {
			return dict.Values.All(val => val == targetNest);
		}

		public string removeFrom(string str) {
			return str.Substring(0, openIndex) + str.Substring(closeIndex + closeLen);
		}

		public static Dictionary<(string, string), List<DelimPair>> searchAll(string str, params (string, string)[] delimTypes) {
			var output = new Dictionary<(string, string), List<DelimPair>>();

			foreach ((string open, string close) in delimTypes) {
				output[(open, close)] = genPairs(str, open, close);
			}
			
			return output;
		}

		public static DelimPair searchPairs(string str, string open, string close, int searchIndex) {
			return genPairDict(str, open, close)[searchIndex];
		}

		public string whole(string str) {
			return str.Substring(openIndex, closeIndex - openIndex + 1);
		}

		public string contents(string str) {
			return str.Substring(openIndex + 1, closeIndex - openIndex - 1);
		}

		public string htmlContents(string str) {
			string content = contents(str);
			return content.Substring(content.IndexOf(">") + 1);
		}

		public static Dictionary<int, DelimPair> genPairDict(string str, (string, string) openAndClose) {
			(string open, string close) = openAndClose;
			return genPairDict(str, open, close);
		}
		
		public static Dictionary<int, DelimPair> genPairDict(string str, string open, string close) {
			var list = genPairs(str, open, close);
			Dictionary<int, DelimPair> dict = new Dictionary<int, DelimPair>();

			foreach (var pair in list) {
				dict[pair.openIndex] = pair;
				dict[pair.closeIndex] = pair;
			}

			return dict;
		}

		public static List<DelimPair> genPairs(string str, (string, string) openAndClose) {
			(string open, string close) = openAndClose;
			return genPairs(str, open, close);
		}

		public static List<DelimPair> genPairs(string str, string open, string close) {
			
			Stack<int> stack = new Stack<int>();
			List<DelimPair> pairs = new List<DelimPair>();

			int openLen = open.Length, closeLen = close.Length;
			for (int i = 0; i < str.Length; i++) {
				if (i <= str.Length - openLen && (i > str.Length - closeLen || str.Substring(i, closeLen) != close || (open == close && stack.Count == 0)) &&
				    str.Substring(i, openLen) == open) {
					stack.Push(i);
					continue;
				}

				if (i <= str.Length - closeLen && str.Substring(i, closeLen) == close) {
					pairs.Add(new DelimPair(stack.Pop(), i, openLen, closeLen));
				}
			}

			foreach (var pair in pairs) {
				foreach (var other in pairs) {
					if (pair == other) continue;

					if (pair.openIndex > other.openIndex && pair.closeIndex < other.closeIndex) pair.nestCount++;
				}
			}

			return pairs;
		}

		/*public static List<DelimPair> genOrderedPairs(string str, string open, string close) { 
			
		}*/
	}

}