using MonoGameHtml.Parser;

namespace MonoGameHtml.Tokenization {
	public struct Token : TokenLike {
		public TokenType type;
		public int index;
		public int length;
		public string value;
	}
}