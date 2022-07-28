using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Range = MonoGameHtml.ColorConsole.Range;

namespace MonoGameHtml {
	public static class MonoGameHtmlParser {

		public static string ExpandSelfClosedHtmlTags(string code) {
			var braceDict = DelimPair.genPairDict(code, DelimPair.CurlyBrackets);
			// TODO: DO NOT MAKE DELIM PAIRS WITH QUOTES INSIDE OF STRINGS!
			var singleQuoteDict = DelimPair.genPairDict(code, DelimPair.SingleQuotes);
			var doubleQuoteDict = DelimPair.genPairDict(code, DelimPair.Quotes);
			var parenDict = DelimPair.genPairDict(code, DelimPair.Parens);

			string FindTag(int afterOpenCarrot) {
				string tag = "";

				for (int i = afterOpenCarrot; i < code.Length; i++) {
					char c = code[i];
					if (c == '/' || c.IsWhiteSpace()) {
						break;
					}

					tag += c;
				}

				return tag;
			}

			var selfClosingRanges = new Stack<(int, int)>();

			for (int i = 0; i < code.Length; i++) {
				char c = code[i];

				if (c == '<') {
					HtmlStartInfo startInfo = new HtmlStartInfo(code, i, true, false, braceDict, singleQuoteDict, doubleQuoteDict, parenDict);
					if (startInfo.valid) {
						
						selfClosingRanges.Push((startInfo.startIndex, startInfo.closeIndex));
						i = startInfo.closeIndex;
					}
				}
			}

			int count = selfClosingRanges.Count;
			for (int i = 0; i < count; i++) {
				var (start, end) = selfClosingRanges.Pop();
				string tag = FindTag(start + 1);
				int afterEnd = end + 2;
				code = code[..end] + $"></{tag}>" +code[afterEnd..];
			}

			return code;
		}

		public static async Task<List<List<(Color, int)>>> ColorSyntaxHighlightedCSharpHtml(string code) {
			var ranges = await ColorConsole.ConsoleMain.SyntaxHighlightCSharpHtml(code);

			
			static Color ClassificationToColor(Range range) {
				
				Color htmlBrace = new Color(255,132,101);
				Color htmlTag = new Color(255,255,108);
				Color htmlMacro = new Color(255, 101, 101);
				Color orange = new Color(242, 142, 42);
				Color number = new Color(124, 199, 255);
				
				switch (range.ClassificationType)
				{
					case "keyword":
					case "keyword - control":
					case "HtmlTagControl":
						return orange;
					case "class name":
						return Color.White;
					case "number":
						return number;
					case "string":
						return Color.LightGreen;
					case "operator":
						return htmlTag;
					case "punctuation":
						return Color.White;
					case "HtmlBrackets":
						return htmlTag;
					case "HtmlBrace":
						return htmlBrace;
					case "HtmlTag":
						return htmlTag;
					case "HtmlMacro":
						return htmlMacro;
					case "KnownHtmlProp":
						return Color.White;
					case "UnknownHtmlProp":
						return Color.Gray;
					default:
						return Color.White;
				}
			}
			

			var listList = new List<List<(Color, int)>>{new List<(Color, int)>()};

			foreach (var range in ranges) {
				if (range.ClassificationType == "LINEBREAK") {
					listList[^1].Add((ClassificationToColor(range), range.TextSpan.Length));
					listList.Add(new List<(Color, int)>());
				}
				else {
					listList[^1].Add((ClassificationToColor(range), range.TextSpan.Length));
				}
			}
			
			// yikes
			for (int i = 0; i < listList.Count; i++) {
            	var list = listList[i];
            	for (int j = list.Count - 1; j > 0; j--) {
            		var elem = list[j];
            		var prevElem = list[j - 1];
            		if (elem.Item1 == prevElem.Item1) {
            			list.RemoveAt(j);
            			list[j - 1] = (elem.Item1, elem.Item2 + prevElem.Item2);
            		}
            	}
            }
			
			return listList;
		}
		
		public static bool EndingHtml(string code, int startIndex, out int closeEndIndex) {
			closeEndIndex = -1;

			for (int i = startIndex + 1; i < code.Length; i++) {
				char c = code[i];
				if (i == startIndex + 1) {
					if (c == '/') continue;
				} else if (i == startIndex + 2) {
					if (c.IsLetter()) continue;
				} else {
					if (c.IsAlphanumeric()) continue;
					if (c == '>') {
						closeEndIndex = i;
						return true;
					}
				}
				break;
			}

			return false;
		}


		// prerequisite that all self-closing nodes have been expanded!
		public static List<HtmlPair> FindHtmlPairs(string code, bool extractData = false) {

			/***
			 * TODO: add context type (C# vs HTML vs HTML/C# (dynamic html))
			 * then, during HTML/C#, you can differentiate between an opening tag and generics
			 * by checking the last non-whitespace character before the '<'
			 * -if it is a valid TypeName ending-character, then it is not HTML!
			 */
			
			
			var braceDict = DelimPair.genPairDict(code, DelimPair.CurlyBrackets);
			// TODO: DO NOT MAKE DELIM PAIRS WITH QUOTES INSIDE OF STRINGS!
			var singleQuoteDict = DelimPair.genPairDict(code, DelimPair.SingleQuotes);
			var doubleQuoteDict = DelimPair.genPairDict(code, DelimPair.Quotes);
			var parenDict = DelimPair.genPairDict(code, DelimPair.Parens);

			var startInfoStack = new Stack<HtmlStartInfo>();
			var pairs = new List<HtmlPair>();
			
			// loop through and check
			for (int i = 0; i < code.Length; i++) {
				char c = code[i];

				if (c == '<') {
					HtmlStartInfo startInfo = new HtmlStartInfo(code, i, false, extractData,
						braceDict, singleQuoteDict, doubleQuoteDict, parenDict);

					if (startInfo.valid) {

						startInfoStack.Push(startInfo);

						i = startInfo.closeIndex;
					}
					
					if (EndingHtml(code, i, out int closeEnd)) {

						HtmlStartInfo topStartInfo = startInfoStack.Pop();
						int openStart = topStartInfo.startIndex, openEnd = topStartInfo.closeIndex;
						HtmlPair htmlPair = new HtmlPair(openStart, i, openEnd - openStart + 1, closeEnd - i + 1) {
							jsxFrags = topStartInfo.jsxFrags
						};

						pairs.Add(htmlPair);
						
						i = closeEnd;
					}
				}
			}

			// add nesting
			foreach (var pair in pairs) {
				foreach (var other in pairs) {
					if (pair == other) continue;

					if (pair.isWithin(other)) pair.nestCount++;
				}
			}
			
			return pairs;
		}

		public static List<string> SplitUnNested(this string str, string target) { 
			var list = new List<string>();

			int nextStart = 0;
			
			void Next(int i) { 
				list.Add(str.Sub(nextStart, i));
				nextStart = i+1;
			}

			int targetLen = target.Length;

			var dict = DelimPair.searchAll(str,
				DelimPair.Parens, DelimPair.CurlyBrackets, DelimPair.SquareBrackets,
				DelimPair.Quotes, DelimPair.SingleQuotes, DelimPair.GenericCarrots);

			for (int i = 0; i < str.Length + 1 - targetLen; i++) {
				if (str.Substring(i, targetLen) == target) {
					bool unNested = DelimPair.allNestOf(0, str.nestAmountsLen(i, targetLen, dict));
					if (unNested) {
						Next(i);
					}
				}
			}
			Next(str.Length);

			return list;
		}
		
		[Obsolete("This method is bad, please rewrite.", false)]
		// assumes otherTarget as equal length
		public static List<string> SplitUnNested(this string str, string target, string otherTarget) { 
			var list = new List<string>();

			int nextStart = 0;
			
			void Next(int i) { 
				list.Add(str.Sub(nextStart, i));
				nextStart = i+1;
			}

			int targetLen = target.Length;

			var dict = DelimPair.searchAll(str,
				DelimPair.Parens, DelimPair.CurlyBrackets, DelimPair.SquareBrackets,
				DelimPair.Quotes, DelimPair.SingleQuotes, DelimPair.GenericCarrots);

			for (int i = 0; i < str.Length + 1 - targetLen; i++) {
				string segment = str.Substring(i, targetLen);
				if (segment == target || segment == otherTarget) {
					bool unNested = DelimPair.allNestOf(0, str.nestAmountsLen(i, targetLen, dict));
					if (unNested) {
						Next(i);
					}
				}
			}
			Next(str.Length);

			return list;
		}

		public static List<string> SplitUnNestedCommas(this string str) {
			return SplitUnNested(str, ",");
		}
	}
}