using System.Collections.Generic;

namespace MonoGameHtml {
	internal static class Parser {

		public static void CheckCarrotType(string code) {

			/***
			 * TODO: add context type (C# vs HTML vs HTML/C# (dynamic html))
			 * then, during HTML/C#, you can differentiate between an opening tag and generics
			 * by checking the last non-whitespace character before the '<'
			 * -if it is a valid TypeName ending-character, then it is not HTML!
			 */
			
			
			var braceDict = DelimPair.genPairDict(code, DelimPair.CurlyBrackets);

			string output = "";

			for (int i = 0; i < code.Length; i++) { // fill output string
				output += (code[i] == '\n') ? '\n' : 'x';
			}

			// TODO: check if there is a closing tag!!!!
			bool StartingHtml(int startIndex, out int closeStartIndex) {
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

							if (c == '{' && propEnter) {
								i = braceDict[i].closeIndex;
								propName = false;
								propEnter = false;
								continue;
							}
						}

						if (c == '>') {
							closeStartIndex = i;
							return true;
						}
					}

					break;
				}

				return false;
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

			// loop through and check
			for (int i = 0; i < code.Length; i++) {
				char c = code[i];

				if (c == '<') {
					if (StartingHtml(i, out int closeStart)) {
						InsertOutputChar(i, '0');
						InsertOutputChar(closeStart, '1');
						i = closeStart;
					}
					if (EndingHtml(i, out int closeEnd)) {
						InsertOutputChar(i, '2');
						InsertOutputChar(closeEnd, '3');
						i = closeEnd;
					}
				}
			}
			Logger.log(code);
			Logger.log(output);
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