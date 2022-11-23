using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MonoGameHtml.Tokenization.Patterns;

namespace MonoGameHtml.Tokenization {
	public static class Tokenizer {

		public static TokenType[] patternOrder = Util.GetValues<TokenType>().ToArray();

		public static Token[] Tokenize(string fileStr) {
			int i = 0;
			var output = new List<Token>();
			while (i < fileStr.Length) {
				
				foreach (var type in patternOrder) {
					var pattern = TokenTypePatterns.GetPatternFor(type);
					
					var (success, countMoved) = pattern.Apply(fileStr, i);
					if (success) {
						output.Add(new Token { 
							type = type, 
							index = i, 
							length = countMoved, 
							value = fileStr.Substring(i, countMoved)
						});
						i += countMoved;
						goto continueWhile;
					}
				}
				
				throw new Exception($"Failed to fully tokenize string. (at: i={i} w/ { string.Join(", ", output.Select(token => token.type)) }) & failed here =>\"{fileStr[i..]}\"");
				continueWhile: { }
			}

			if (i > fileStr.Length) { 
				throw new Exception("Over-tokenized string.");
			}
			
			return output.ToArray();
		}

		public static Token[] RemoveSuperfluous(this IEnumerable<Token> tokens) { 
			return tokens
				.Where(token => token.type != TokenType.WhiteSpace && token.type != TokenType.Comment).ToArray();
		}
	}
}