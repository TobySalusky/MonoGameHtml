using System;

namespace MonoGameHtml.Tokenization.Patterns {
	public class Meta : TokenPattern {
		private readonly int skipCount;
		private readonly Func<string, int, bool> func;

		public Meta(Func<string, int, bool> func, int skipCount = 0) {
			this.func = func;
			this.skipCount = skipCount;
		}

		public (bool, int) Apply(string str, int i) {
			if (func(str, i)) {
				return TokenPattern.Succeed(skipCount);
			}

			return TokenPattern.Fail();
		}
	}
}