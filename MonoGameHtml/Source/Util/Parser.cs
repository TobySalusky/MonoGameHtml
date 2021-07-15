using System;
using System.Collections.Generic;

namespace MonoGameHtml {
	internal static class Parser {

		public static string ExpandSelfClosedHtmlTags(string code) {
			var braceDict = DelimPair.genPairDict(code, DelimPair.CurlyBrackets);
			// TODO: DO NOT MAKE DELIM PAIRS WITH QUOTES INSIDE OF STRINGS!
			var singleQuoteDict = DelimPair.genPairDict(code, DelimPair.SingleQuotes);
			var doubleQuoteDict = DelimPair.genPairDict(code, DelimPair.Quotes);

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
					if (StartingHtml(code, braceDict, singleQuoteDict, doubleQuoteDict, i, out int closeStart, true)) {
						
						selfClosingRanges.Push((i, closeStart));
						Logger.log(i, closeStart);
						
						i = closeStart;
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

		private static bool StartingHtml(string code, IReadOnlyDictionary<int, DelimPair> braceDict, IReadOnlyDictionary<int, DelimPair> singleQuoteDict, 
			IReadOnlyDictionary<int, DelimPair> doubleQuoteDict, int startIndex, out int closeStartIndex, bool selfClosing = false) {
        	closeStartIndex = -1;

        	// TODO: when bold/annotations come in, this will have to be case-based
        	// looks backwards to check that the preceding character is not part of a valid variable name
        	// avoids capturing generics as HTML
        	for (int i = startIndex - 1; i >= 0; i--) {
        		if (code[i].IsWhiteSpace()) continue;
        		if (code[i].IsValidReferenceNameCharacter()) {
        			return false;
        		}
        		break;
        	}

        	bool tagDone = false, propName = false, propEnter = false;
        	
        	for (int i = startIndex + 1; i < code.Length; i++) {
        		char c = code[i];
        		if (i == startIndex + 1) {
        			if (c.IsLetter()) continue;
        		} else {
	                if (c.IsAlphanumeric()) {
        				if (tagDone) propName = true;
        				continue;
        			}

	                if (tagDone && c == '-') {// TODO: temporary- remove dash
		                propName = true;
		                continue;
	                }

	                if (c.IsWhiteSpace()) {
        				tagDone = true;
        				continue;
        			}

        			if (propName) {
        				if (c == '=') {
        					if (propEnter) break;
        					propEnter = true;
        					continue;
        				}

        				if (propEnter) {
        					switch (c) {
        						case '{':
        							i = braceDict[i].closeIndex;
        							propName = false;
        							propEnter = false;
        							break;
        						case '\'':
        							i = singleQuoteDict[i].closeIndex;
        							propName = false;
        							propEnter = false;
        							break;
        						case '"':
        							i = doubleQuoteDict[i].closeIndex;
        							propName = false;
        							propEnter = false;
        							break;
        					}

        					continue;
        				}
        			}

                    if (selfClosing) {
	                    if (c == '/' && (i + 1) < code.Length && code[i + 1] == '>') {
		                    closeStartIndex = i;
		                    return true;
	                    }
                    } else {
	                    if (c == '>') {
		                    closeStartIndex = i;
		                    return true;
	                    }
                    }
                }

        		break;
        	}

        	return false;
        }

		
		public static List<HtmlPair> FindHtmlPairs(string code) {

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

			string output = "";

			for (int i = 0; i < code.Length; i++) { // fill output string
				output += (code[i] == '\n') ? '\n' : 'x';
			}

			
			bool EndingHtml(int startIndex, out int closeEndIndex) {
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

			void InsertOutputChar(int i, char c) {
				output = output[..i] + c + output[(i + 1)..];
			}

			var openRanges = new Stack<(int, int)>();
			var pairs = new List<HtmlPair>();
			
			// loop through and check
			for (int i = 0; i < code.Length; i++) {
				char c = code[i];

				if (c == '<') {
					{if (StartingHtml(code, braceDict, singleQuoteDict, doubleQuoteDict, i, out int closeStart)) {
						InsertOutputChar(i, '0');
						InsertOutputChar(closeStart, '1');

						openRanges.Push((i, closeStart));

						i = closeStart;
					}}
					{if (EndingHtml(i, out int closeEnd)) {
						InsertOutputChar(i, '2');
						InsertOutputChar(closeEnd, '3');

						var (openStart, openEnd) = openRanges.Pop();
						pairs.Add(new HtmlPair(openStart, i, openEnd - openStart + 1, closeEnd - i + 1));
						
						i = closeEnd;
					}}
				}
			}

			foreach (var pair in pairs) {
				/*Logger.log("test1",pair.whole(code));
				Logger.log("test2",pair.contents(code));
				Logger.log("test3",pair.openContents(code));*/
				
				foreach (var other in pairs) {
					if (pair == other) continue;

					if (pair.isWithin(other)) pair.nestCount++;
				}
			}
			
			//Logger.log(code);
			//Logger.log(output);
			return pairs;
		}

		public static List<string> SplitUnNestedCommas(this string str) { 
			var list = new List<string>();

			int nextStart = 0;
			
			void Next(int i) { 
				list.Add(str.Sub(nextStart, i));
				nextStart = i+1;
			}

			var dict = DelimPair.searchAll(str,
				DelimPair.Parens, DelimPair.CurlyBrackets, DelimPair.SquareBrackets,
				DelimPair.Quotes, DelimPair.SingleQuotes, DelimPair.Carrots);

			for (int i = 0; i < str.Length; i++) {
				if (str.Substring(i, 1) == ",") {
					bool unNested = DelimPair.allNestOf(0, str.nestAmountsLen(i, 1, dict));
					if (unNested) {
						Next(i);
					}
				}
			}
			Next(str.Length);

			return list;
		}
	}
}