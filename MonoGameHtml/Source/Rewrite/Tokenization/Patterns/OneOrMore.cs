using System;

namespace MonoGameHtml.Tokenization.Patterns {
	public class OneOrMore : TokenPattern {
		private readonly TokenPattern proxy;

		public OneOrMore(TokenPattern proxy) {
			this.proxy = proxy;
		}
		public OneOrMore(Func<char, bool> func) : this(new One(func)) { }
		public OneOrMore(char exact) : this(new One(exact)) { }
		
		public (bool, int) Apply(string str, int i) {
			int startingIndex = i;
			bool hasSucceeded = false;
			while (true) {
				var (success, length) = proxy.Apply(str, i);
				if (!success) {
					break;
				}
				hasSucceeded = true;
				i += length;
			}
			return (hasSucceeded, i - startingIndex);
		}
	}
}