namespace MonoGameHtml.Tokenization.Patterns {
	public class Any : TokenPattern {
		private readonly TokenPattern[] patterns;
		public Any(params TokenPattern[] patterns) {
			this.patterns = patterns;
		}

		public (bool, int) Apply(string str, int i) {
			foreach (var pattern in patterns) {
				var (success, countMoved) = pattern.Apply(str, i);
				if (success) {
					return TokenPattern.Succeed(countMoved);
				}
			}

			return TokenPattern.Fail();
		}
	}
}