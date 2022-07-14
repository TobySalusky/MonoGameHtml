namespace MonoGameHtml.Tokenization.Patterns {
	public class All : TokenPattern {
		private readonly TokenPattern[] patterns;
		public All(params TokenPattern[] patterns) {
			this.patterns = patterns;
		}

		public (bool, int) Apply(string str, int i) {
			int startingIndex = i;
			foreach (var pattern in patterns) {
				var (success, countMoved) = pattern.Apply(str, i);
				if (!success) {
					return TokenPattern.Fail();
				}

				i += countMoved;
			}

			return (true, i - startingIndex);
		}
	}
}