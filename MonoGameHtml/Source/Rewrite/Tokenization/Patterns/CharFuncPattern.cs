using System;

namespace MonoGameHtml.Tokenization.Patterns {
	public abstract class CharFuncPattern {
		protected Func<char, bool> charFunc;
		protected CharFuncPattern(Func<char, bool> func) {
			charFunc = func;
		}
		protected CharFuncPattern(char exact) {
			charFunc = c => c == exact;
		}
	}
}