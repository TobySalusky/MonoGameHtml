using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameHtml {
	public static class HtmlMain {
		internal static GraphicsDevice graphicsDevice;
		internal static Vector2 screenDimen, screenCenter;
		internal static int screenWidth, screenHeight;

		internal static string fontPath;

		internal static bool logOutput;

		public static void Initialize(Game game, string fontPath = "NoPathSpecified", bool logOutput = false) {

			HtmlMain.logOutput = logOutput;
			graphicsDevice = game.GraphicsDevice;
			
			HtmlMain.fontPath = fontPath.Trim();
			HtmlMain.fontPath = HtmlMain.fontPath.Replace("/", "\\");
			if (!HtmlMain.fontPath.EndsWith("\\")) HtmlMain.fontPath += "\\";
			HtmlMain.fontPath = HtmlMain.fontPath.Replace("\\", "/");

			// TODO: handle fullscreen
			screenDimen = new Vector2(game.GraphicsDevice.PresentationParameters.Bounds.Width, game.GraphicsDevice.PresentationParameters.Bounds.Height);
			screenCenter = screenDimen / 2;
			screenWidth = (int) screenDimen.X;
			screenHeight = (int) screenDimen.Y;
			
			Logger.log("Dimensions:", screenDimen);
			Textures.loadTextures();
		}
	}
}