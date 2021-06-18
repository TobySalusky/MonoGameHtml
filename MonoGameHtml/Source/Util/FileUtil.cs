using System.Runtime.CompilerServices;

namespace MonoGameHtml {
	internal static class FileUtil {
		public static string TraceFilePath([CallerFilePath] string sourceFilePath = "") {
			return sourceFilePath;
		}
	}
}