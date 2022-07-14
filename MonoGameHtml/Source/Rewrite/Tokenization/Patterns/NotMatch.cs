namespace MonoGameHtml.Tokenization.Patterns {
	public class NotMatch : TokenPattern {
		private readonly string match;

		public NotMatch(string match) {
			this.match = match;
		}

		public (bool, int) Apply(string str, int i) {
			if (!str.OutOfBounds(i + match.Length - 1) && str.Substring(i, match.Length) != match) { 
				return (true, match.Length);
			}
			return TokenPattern.Fail();
		}
	}
}