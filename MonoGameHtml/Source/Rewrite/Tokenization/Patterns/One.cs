using System;

namespace MonoGameHtml.Tokenization.Patterns {
	public class One : CharFuncPattern, TokenPattern {
		public One(Func<char, bool> func) : base(func) { }
		public One(char exact) : base(exact) { }
		
		public (bool, int) Apply(string str, int i) {
			if (str.OutOfBounds(i) || !charFunc(str[i])) return TokenPattern.Fail();
			return (true, 1);
		}
	}
}