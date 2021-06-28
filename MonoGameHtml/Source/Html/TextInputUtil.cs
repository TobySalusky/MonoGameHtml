using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace MonoGameHtml {
	internal static class TextInputUtil {
		
		public static readonly Dictionary<Keys, (string, string)> symbolTable = new Dictionary<Keys, (string, string)> {
			[Keys.OemPeriod] = (">", "."),
			[Keys.OemComma] = ("<", ","),
			[Keys.OemQuestion] = ("?", "/"),
			[Keys.OemSemicolon] = (":", ";"),
			[Keys.OemTilde] = ("~", "`"),
			[Keys.OemPipe] = ("|", "\\"),
		};
		
		public static readonly char[] shiftNum = { 
			')', '!', '@', '#', '$', '%', '^', '&', '*', '('
		};

		public static void enterAction() { 
			Logger.log("Entered!");
		}

		public static void method(KeyInfo keys, string text) {
            
            KeyboardState oldState = keys.oldState;
            KeyboardState newState = keys.newState;

            Keys[] arr = newState.GetPressedKeys();
            //bool changed = false;
            bool shift = keys.down(Keys.LeftShift) || keys.down(Keys.RightShift);
            bool control = keys.down(Keys.LeftControl) || keys.down(Keys.RightControl);
            bool capsLock = newState.CapsLock;

            foreach (Keys key in arr) {
	            string str = key.ToString();
	            bool single = str.Length == 1;
	            char c = str[0];
	            
	            if (oldState.GetPressedKeys().Contains(key)) continue; // TODO: hold for spam

	            if (single) { // capital/lowercase alphabet
		            if (shift || newState.CapsLock) text += c;
		            else text += (char) (c + 32);
		            
	            } else if (str.Length == 2 && c == 'D' && str[1] >= '0' && str[1] <= '9') {	// Numbers
		            text += (shift) ? (shiftNum[int.Parse("" + str[1])]) : str[1];
		            
	            } else if (key == Keys.Back) { // Back space
		            if (text.Length > 0) {
			            text = control ? text.Remove(Math.Max(0, text.lastIndexOf(" "))) : text.Remove(text.Length - 1);
		            }
	            } else if (key == Keys.Enter) { // enter
		            enterAction();
	            } else if (key == Keys.Space) { // space
		            text += " ";
	            } else if (symbolTable.ContainsKey(key)) { // symbols
		            text += shift ? symbolTable[key].Item1 : symbolTable[key].Item2;
	            }
            }
		}
    }
	
}