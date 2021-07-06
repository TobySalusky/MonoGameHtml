using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Input;

namespace MonoGameHtml {
	public static class TextInputUtil {

		public static readonly Dictionary<Keys, (string, string)> symbolTable = new Dictionary<Keys, (string, string)> {
			[Keys.OemPeriod] = (">", "."),
			[Keys.OemComma] = ("<", ","),
			[Keys.OemQuestion] = ("?", "/"),
			[Keys.OemSemicolon] = (":", ";"),
			[Keys.OemTilde] = ("~", "`"),
			[Keys.OemPipe] = ("|", "\\"),
			[Keys.OemOpenBrackets] = ("{", "["),
			[Keys.OemCloseBrackets] = ("}", "]"),
			[Keys.OemPlus] = ("+", "="),
			[Keys.OemMinus] = ("_", "-"),
			[Keys.OemQuotes] = ("\"", "'"),
		};
		
		public static readonly char[] shiftNum = { 
			')', '!', '@', '#', '$', '%', '^', '&', '*', '('
		};

		public static void enterAction() { 
			Logger.log("Entered!");
		}

		public static string getUpdatedText(KeyInfo keys, string text, TypingState typingState) {

			string initText = text;
			
            KeyboardState oldState = keys.oldState;
            KeyboardState newState = keys.newState;

            Keys[] arr = newState.GetPressedKeys();
            //bool changed = false;
            bool shift = keys.down(Keys.LeftShift) || keys.down(Keys.RightShift);
            bool control = keys.down(Keys.LeftControl) || keys.down(Keys.RightControl);
            bool capsLock = newState.CapsLock;

            bool undo = false, forceUndo = false;

            foreach (Keys oldKey in oldState.GetPressedKeys()) {
	            if (keys.unpressed(oldKey) && typingState.keyDownTime.ContainsKey(oldKey))
		            typingState.keyDownTime.Remove(oldKey);
            }
            
            foreach (Keys key in arr) {
	            string str = key.ToString();
	            bool single = str.Length == 1;
	            char c = str[0];

	            // increment time
	            if (!typingState.keyDownTime.ContainsKey(key)) {
		            typingState.keyDownTime[key] = 0F;
	            } else { 
		            typingState.keyDownTime[key] += typingState.deltaTime;
	            }

	            // hits from holding
	            const float initWait = 0.375F, keyFreq = 0.035F;
	            if (oldState.GetPressedKeys().Contains(key)) {
		            float downTime = typingState.keyDownTime[key] - initWait;
		            if (downTime >= keyFreq) { 
			            typingState.keyDownTime[key] -= keyFreq;
		            } else { 
			            continue;
		            }
	            }

	            if (single) { // capital/lowercase alphabet
		            if (control && key == Keys.Z) {
			            if (typingState.undos.Count > 0) {
				            text = typingState.undos.Pop();
				            undo = true;
			            }
			            break;
		            }

		            if (shift || capsLock) text += c;
		            else text += (char) (c + 32);
		            
	            } else if (str.Length == 2 && c == 'D' && str[1] >= '0' && str[1] <= '9') {	// Numbers
		            text += (shift) ? (shiftNum[int.Parse("" + str[1])]) : str[1];
		            
	            } else if (key == Keys.Back) { // Back space
		            if (text.Length > 0) {
			            if (control) {
				            forceUndo = true;
				            const string deleteUntilRegex = "\\s\\S";
				            var matches = Regex.Matches(text, deleteUntilRegex);
				            int index = matches.Any() ? matches.Max(match => match.Index + 1) : 0;
				            text = text.Substring(0, index);
			            } else {
				            text = text.Remove(text.Length - 1);
			            }
		            }
	            } else if (key == Keys.Enter) { // enter
		            enterAction();
		            if (typingState.multiline) text += '\n';
	            } else if (key == Keys.Tab) {
		            text += '\t';
	            } else if (key == Keys.Space) { // space
		            text += " ";
	            } else if (symbolTable.ContainsKey(key)) { // symbols
		            text += shift ? symbolTable[key].Item1 : symbolTable[key].Item2;
	            }
            }

            if (typingState.diff != null && text != initText) text = typingState.diff(initText, text);

            if (text != initText && !undo && (forceUndo || (typingState.time - typingState.lastUndoTime) > typingState.undoFrequency)) {
	            typingState.undos.Push(initText);
	            typingState.lastUndoTime = typingState.time;
            }

            return text;
		}
    }

	public class TypingState {
		public int index;
		public bool multiline;
		public Stack<string> undos = new Stack<string>();
		public Func<string, string, string> diff;
		public float time, deltaTime, lastUndoTime, undoFrequency;
		public Dictionary<Keys, float> keyDownTime = new Dictionary<Keys, float>();
	}
	
}