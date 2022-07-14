namespace MonoGameHtml {
	public static class StrUtil {
		public static bool OutOfBounds(this string str, int index) {
			return index >= str.Length;
		}
	}
}