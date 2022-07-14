namespace MonoGameHtml.Tokenization {
	public interface TokenPattern {
		public (bool, int) Apply(string str, int i);

		public static (bool, int) Fail() {
			return (false, 0);
		}

		public static (bool, int) Succeed(int length) { 
			return (true, length);
		}
	}
}