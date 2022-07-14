namespace MonoGameHtml.Lexical {
	public class TokenGroup : TokenLike {
		public string Name { get; }
		public TokenLike[] TokenLikes { get; }

		public TokenGroup(string name, TokenLike[] tokenLikes) {
			Name = name;
			TokenLikes = tokenLikes;
		}
	}
}