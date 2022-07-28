using System;
using System.Collections.Generic;
using MonoGameHtml.Tokenization;

namespace MonoGameHtml.Parser {
	public static class Parser {
		static Parser() {
			ParseRule.namedRules = ParseRuleParser.ParseLexerRules();
		}

		public static ParseResult Parse(IEnumerable<Token> tokens, string lexAs = "file") {
			return new ParseRule.Named { Name = lexAs }.Apply(tokens);
		}
	}
}