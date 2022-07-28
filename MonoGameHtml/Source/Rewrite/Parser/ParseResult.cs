using System.Collections.Generic;
using MonoGameHtml.Tokenization;

namespace MonoGameHtml.Parser {
	public readonly struct ParseResult {
		public bool Succeeded { get; }
		public TokenLike Matched { get; }
		public IEnumerable<Token> Continuation { get; }

		public ParseResult(bool succeeded, TokenLike matched = null, IEnumerable<Token> continuation = null) {
			Succeeded = succeeded;
			Matched = matched;
			Continuation = continuation;
		}

		public static ParseResult Fail => new ParseResult(false);
	}
}