using System.Runtime.CompilerServices;

namespace MonoGameHtml {
	internal static class FileUtil {

		public static string correctDirPath(string dirPath) {
			if (dirPath == null) return null;
				
			dirPath = dirPath.Trim().Replace("\\", "/");
			if (!dirPath.EndsWith("/")) dirPath += "/";
			return dirPath;
		}
	}
}