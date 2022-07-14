namespace MonoGameHtml.Tokenization.Patterns {
	public class Optional : TokenPattern {
		private readonly TokenPattern proxy;

		public Optional(TokenPattern proxy) {
			this.proxy = proxy;
		}

		public (bool, int) Apply(string str, int i) {
			return (true, proxy.Apply(str, i).Item2);
		}
	}
}