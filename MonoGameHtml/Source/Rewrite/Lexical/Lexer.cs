using System;
using System.Collections.Generic;
using MonoGameHtml.Tokenization;

namespace MonoGameHtml.Lexical {
	public static class Lexer {
		static Lexer() {
			LexerRule.namedRules = LexerRuleParser.ParseLexerRules();
		}

		public static LexResult Lex(IEnumerable<Token> tokens, string lexAs = "file") {
			return new LexerRule.Named { Name = lexAs }.Apply(tokens);
		}
	}
}