﻿﻿using System;
  using System.Collections.Generic;
using System.Linq;

namespace MonoGameHtml {
	public class DelimPair { // TODO: gen ordered list
		
		public int openIndex, openLen;
		public int closeIndex, closeLen;
		public int nestCount;

		public int AfterClose => closeIndex + closeLen;

		public static readonly (string, string) Parens = ("(", ")"), GenericCarrots = ("<", ">"), // TODO: do something special with carrots!!
			SquareBrackets = ("[", "]"), CurlyBrackets = ("{", "}"), 
			Quotes = ("\"", "\""), SingleQuotes = ("'", "'");
		
		public DelimPair(int openIndex, int closeIndex, int openLen = 1, int closeLen = 1, int nestCount = 0) {
			this.openIndex = openIndex;
			this.closeIndex = closeIndex;
			this.openLen = openLen;
			this.closeLen = closeLen;
			this.nestCount = nestCount;
		}

		public bool isWithin(DelimPair other) {
			return (openIndex >= (other.openIndex + other.openLen) && closeIndex < other.closeIndex);
		}

		public bool isPartOf(DelimPair other) {
			return (openIndex >= other.openIndex && closeIndex <= other.closeIndex);
		}

		public int contentStartIndex() {
			return openIndex + openLen;
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
			return str[openIndex..(closeIndex + closeLen)];
		}

		public string contents(string str) {
			return str.Sub(openIndex + openLen, closeIndex);
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
			return toDict(genPairs(str, open, close));
		}

		public static Dictionary<int, DelimPair> toDict(List<DelimPair> list) { 
			Dictionary<int, DelimPair> dict = new Dictionary<int, DelimPair>();

			foreach (var pair in list) {
				dict[pair.openIndex] = pair;
				dict[pair.closeIndex] = pair;
			}
			return dict;
		}

		public static List<DelimPair> genPairs(string str, (string, string) openAndClose, Func<string, int, bool> req = null) {
			(string open, string close) = openAndClose;
			return genPairs(str, open, close);
		}

		/*public static List<DelimPair> genUnNestedPairs(string str, string open, string close) {
			return genPairs(str, open, close, (str, i) => {
				
				return false;
			});
		}*/

		public static List<DelimPair> genPairs(string str, string open, string close, Func<string, string, int, bool> req = null) {

			/*if ((open == "<" || close == "<") && req == null) { // parsing html is not messed up by props
				var dict = searchAll(str, CurlyBrackets);
				return genPairs(str, open, close, (str, delim, i) => 
					allNestOf(0, str.nestAmountsLen(i, delim.Length, dict)));
			}*/
			if ((open == "<" || close == ">") && req == null) {
				int openCount = 0;
				return genPairs(str, open, close, (str, delim, i) => {
					if (delim == open) {
						for (int j = i - 1; j >= 0; j--) {
							if (str[j].IsWhiteSpace()) continue;
							if (str[j].IsValidReferenceNameCharacter()) {
								openCount++;
								return true;
							}
							return false;
						}
						return false;
					}

					if (openCount > 0) {
						openCount--;
						return true;
					}
					return false;
				});
			}

			Stack<int> stack = new Stack<int>();
			List<DelimPair> pairs = new List<DelimPair>();

			bool hasReq = req != null;

			int openLen = open.Length, closeLen = close.Length;
			for (int i = 0; i < str.Length; i++) {

				if (i <= str.Length - openLen && (i > str.Length - closeLen || str.Substring(i, closeLen) != close || (open == close && stack.Count == 0)) &&
				    str.Substring(i, openLen) == open) {
					if (hasReq && (!req(str, open, i))) continue; // skips when the requirement is not met
					stack.Push(i);
					continue;
				}

				if (i <= str.Length - closeLen && str.Substring(i, closeLen) == close) {
					if (hasReq && (!req(str, close, i))) continue; // skips when the requirement is not met
					pairs.Add(new DelimPair(stack.Pop(), i, openLen, closeLen));
				}
			}

			foreach (var pair in pairs) {
				foreach (var other in pairs) {
					if (pair == other) continue;

					if (pair.isWithin(other)) pair.nestCount++;
				}
			}
			
			return pairs;
		}

		/*public static List<DelimPair> genOrderedPairs(string str, string open, string close) { 
			
		}*/
	}

}