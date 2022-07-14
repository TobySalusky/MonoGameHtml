using System;
using System.Collections.Generic;
using System.Linq;
using MonoGameHtml.Tokenization.Patterns;

namespace MonoGameHtml.Tokenization {
	public static class Tokenizer {

		public static TokenType[] patternOrder = Enum.GetValues(typeof(TokenType)).Cast<TokenType>().ToArray();

		public static IEnumerable<Token> Tokenize(string fileStr) {
			int i = 0;

			while (i < fileStr.Length) {
				foreach (var type in patternOrder) {
					var pattern = TokenTypePatterns.GetPatternFor(type);
					
					var (success, countMoved) = pattern.Apply(fileStr, i);
					if (success) {
						yield return new Token { 
							type = type, 
							index = i, 
							length = countMoved, 
							value = fileStr.Substring(i, countMoved)
						};
						i += countMoved;
						goto continueWhile;
					}
				}

				throw new Exception("Failed to fully tokenize string.");
				continueWhile: { }
			}

			if (i > fileStr.Length) { 
				throw new Exception("Over-tokenized string.");
			}
		}

		public static IEnumerable<Token> RemoveSuperfluous(this IEnumerable<Token> tokens) { 
			return tokens
				.Where(token => token.type != TokenType.WhiteSpace && token.type != TokenType.Comment);
		}
	}
}