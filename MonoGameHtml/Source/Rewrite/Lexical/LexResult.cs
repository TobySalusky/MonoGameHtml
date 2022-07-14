using System.Collections.Generic;
using MonoGameHtml.Tokenization;

namespace MonoGameHtml.Lexical {
	public readonly struct LexResult {
		public bool Succeeded { get; }
		public TokenLike Matched { get; }
		public IEnumerable<Token> Continuation { get; }

		public LexResult(bool succeeded, TokenLike matched = null, IEnumerable<Token> continuation = null) {
			Succeeded = succeeded;
			Matched = matched;
			Continuation = continuation;
		}

		public static LexResult Fail => new LexResult(false);
	}
}