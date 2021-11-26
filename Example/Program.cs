using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGameHtml.Fonts;

namespace Example {
    public static class Program {
        private static Game currentGame;
        
        [STAThread]
        public static void Main() {
            try {
                currentGame = new ExamplePickerGame();
                currentGame.Run();
            }
            catch {
                // suppress errors from replacing Game instance
            }
        }

        public static void ChangeGame(Game game) {
            MonoGameHtmlFonts.Reset();

            currentGame.Exit();
            //currentGame.Dispose();
            currentGame = game;
            game.Run();
        }

        public static void GoBackIfPressedCtrlB(KeyboardState keyboardState) {
            if ((keyboardState.IsKeyDown(Keys.LeftControl) || keyboardState.IsKeyDown(Keys.RightControl)) &&
                keyboardState.IsKeyDown(Keys.B)) ChangeGame(new ExamplePickerGame());
        }
    }
}