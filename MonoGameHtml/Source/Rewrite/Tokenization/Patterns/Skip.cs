namespace MonoGameHtml.Tokenization.Patterns {
	public class Skip : TokenPattern {
		private readonly int skipCount;

		public Skip(int skipCount) {
			this.skipCount = skipCount;
		}

		public (bool, int) Apply(string str, int i) {
			return (true, skipCount);
		}
	}
}