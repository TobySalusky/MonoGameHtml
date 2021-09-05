using System.IO;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameHtml {
	public static class HtmlMain {
		internal static Game game;
		internal static GraphicsDevice graphicsDevice;
		internal static Vector2 screenDimen, screenCenter;
		internal static int screenWidth, screenHeight;

		internal static string fontPath, defaultFontPath, cachePath;

		internal static LoggerSettings loggerSettings;

		static HtmlMain() {
			defaultFontPath = FileUtil.TraceFilePath();
			defaultFontPath = defaultFontPath.Substring(0, defaultFontPath.indexOf("Source"));
			defaultFontPath = Path.Join(defaultFontPath, "Assets", "Fonts", "JetBrainsMono.ttf");
		}

		public static void Initialize(Game game, string fontPath = null, 
			bool cache = true, string cachePath = null, LoggerSettings loggerSettings = null) {
			
			HtmlMain.game = game;

			HtmlMain.loggerSettings = loggerSettings ?? new LoggerSettings();
			graphicsDevice = game.GraphicsDevice;

			HtmlMain.fontPath = FileUtil.correctDirPath(fontPath);
			HtmlMain.cachePath = FileUtil.correctDirPath(cachePath);

			if (cachePath == null) cache = false;
			HtmlSettings.generateCache = false;
			HtmlSettings.useCache = false;

			// TODO: handle fullscreen
			screenDimen = new Vector2(game.GraphicsDevice.PresentationParameters.Bounds.Width, game.GraphicsDevice.PresentationParameters.Bounds.Height);
			screenCenter = screenDimen / 2;
			screenWidth = (int) screenDimen.X;
			screenHeight = (int) screenDimen.Y;
			
			Logger.log("Screen Dimensions:", screenDimen);
			Textures.loadTextures();
		}
	}
}