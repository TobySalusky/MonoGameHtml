using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoGameHtml {
	public static class TextInputUtil {

		public static string spacesPerTab = "  ";
		
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

		public static string getUpdatedText(KeyInfo keys, string text, TypingState typingState) {

			string initText = text;
			int initCursorIndex = typingState.cursorIndex;
			
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
            
            void insert(string str) {
	            text = beforeCursor() + str + afterCursor();
	            typingState.cursorIndex += str.Length;
            }

            void insertChar(char c) {
	            insert(""+c);
            }

            string afterCursor() {
	            return text[typingState.cursorIndex..];
            }

            string beforeCursor() {
	            return text[..typingState.cursorIndex];
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
				            var typingUndo = typingState.undos.Pop();
				            text = typingUndo.text;
				            typingState.cursorIndex = typingUndo.cursorIndex;
				            undo = true;
			            }
			            break;
		            }
		            
		            if (shift || capsLock) insertChar(c);
		            else insertChar((char) (c + 32));
		            
	            } else if (str.Length == 2 && c == 'D' && str[1] >= '0' && str[1] <= '9') {	// Numbers
		            insert("" + ((shift) ? (shiftNum[int.Parse("" + str[1])]) : str[1]));
	            } else if (key == Keys.Back) { // Back space
		            if (text.Length > 0) {
			            if (control) {
				            const string deleteUntilRegex = "[\n\\s]\\S";
				            var matches = Regex.Matches(beforeCursor(), deleteUntilRegex);
				            int index = matches.Any() ? matches.Max(match => match.Index + 1) : 0;
				            text = text[..index] + afterCursor();
				            forceUndo = typingState.cursorIndex != index;
				            
				            typingState.cursorIndex = index;
			            } else if (typingState.cursorIndex != 0) {
				            text = text[..(typingState.cursorIndex - 1)] + afterCursor();
				            typingState.cursorIndex--;
			            }
		            }
	            } else if (key == Keys.Enter) { // enter
		            if (typingState.multiline) insertChar('\n');
	            } else if (key == Keys.Tab) {
		            insertChar('\t');
	            } else if (key == Keys.Space) { // space
		            insertChar(' ');
	            } else if (symbolTable.ContainsKey(key)) { // symbols
		            insert(shift ? symbolTable[key].Item1 : symbolTable[key].Item2);
	            } else if (key == Keys.Left) {
		            typingState.cursorIndex = Math.Max(0, typingState.cursorIndex - 1);
	            } else if (key == Keys.Right) {
		            typingState.cursorIndex = Math.Min(text.Length, typingState.cursorIndex + 1);
	            } else if (key == Keys.Up) {
		            setCursorFromPos(
			            cursorPositionAtIndex(typingState.node, typingState, text, typingState.cursorIndex) 
			            - Vector2.UnitY * typingState.node.font.FindHeight(), 
			            typingState.node, typingState, text);
	            } else if (key == Keys.Down) {
		            setCursorFromPos(
			            cursorPositionAtIndex(typingState.node, typingState, text, typingState.cursorIndex) 
			            + Vector2.UnitY * typingState.node.font.FindHeight(), 
			            typingState.node, typingState, text);
	            }
            }

            if (typingState.diff != null && text != initText) text = typingState.diff(initText, text);

            if (text != initText && !undo && (forceUndo || (typingState.time - typingState.lastUndoTime) > typingState.undoFrequency)) {
	            typingState.undos.Push(new TypingUndo{text = initText, cursorIndex = initCursorIndex});
	            typingState.lastUndoTime = typingState.time;
            }

            if (text != initText || typingState.cursorIndex != initCursorIndex) {
	            typingState.lastEditOrMove = typingState.time;
            }

            return text;
		}

		public static Vector2 cursorPositionAtIndex(HtmlNode node, TypingState typingState, string realText, int cursorIndex) {
			SpriteFont font = node.font;
			float height = font.MeasureString("TEST").Y;
			string text = node.textContent[..Math.Min(node.textContent.Length, typingState.findDisplayCursorIndex(realText, cursorIndex))];
			float x = node.UnpaddedX + font.MeasureString(text[Math.Max(0, text.lastIndexOf("\n"))..]).X;
			float y = node.UnpaddedY + Math.Max(0, font.MeasureString(text).Y - height);
			return new Vector2(x, y);
		}

		public static void drawCursor(SpriteBatch spriteBatch, HtmlNode node, TypingState typingState, string realText) {
			Vector2 cursorPos = cursorPositionAtIndex(node, typingState, realText, typingState.cursorIndex);
			spriteBatch.Draw(Textures.rect, new Rectangle((int) cursorPos.X, (int) cursorPos.Y, 2,(int) node.font.FindHeight()), node.color);
		}

		public static void setCursorFromPos(Vector2 pos, HtmlNode node, TypingState typingState, string realText) {
			string text = node.textContent;
			SpriteFont font = node.font;

			Vector2 start = node.UnpaddedCorner;
			
			// find which line the position corresponds to
			float height = font.FindHeight();
			int lastLine = text.CountOf("\n");
			int line = Math.Clamp((int)((pos.Y-start.Y)/height), 0, lastLine);
			var enters = text.allIndices("\n");
			int lineStart = (line == 0) ? 0 : enters[line - 1] + 1;
			int lineUntil = (enters.Count > line) ? enters[line] : text.Length;
			string lineStr = text.Sub(lineStart, lineUntil);
			string beforeLine = text[..lineStart];
			
			// find nearest index on line
			float minDist = float.MaxValue;
			int minLineIndex = 0;
			for (int i = 0; i <= lineStr.Length; i++) {
				float x = start.X + font.MeasureString(lineStr[..i]).X;
				float dist = Math.Abs(x - pos.X);
				if (dist < minDist) {
					minDist = dist;
					minLineIndex = i;
				}
			}
			
			// calculate overall index
			int displayIndex = beforeLine.Length + minLineIndex;
			int index = typingState.findRealCursorIndex(realText, displayIndex);

			if (typingState.cursorIndex != index) {
				typingState.cursorIndex = index;
				typingState.lastEditOrMove = typingState.time;
			}
		}
	}

	public class TypingState {
		public HtmlNode node;
		public int cursorIndex;
		public bool multiline;
		public readonly Stack<TypingUndo> undos = new Stack<TypingUndo>();
		public Func<string, string, string> diff;
		public float time, deltaTime, lastUndoTime, undoFrequency, lastEditOrMove;
		public readonly Dictionary<Keys, float> keyDownTime = new Dictionary<Keys, float>();
		
		public int findDisplayCursorIndex(string text, int index) {
			return index + (TextInputUtil.spacesPerTab.Length - 1) * text[..index].CountOf("\t");
		}

		public int findDisplayCursorIndex(string text) {
			return findDisplayCursorIndex(text, cursorIndex);
		}

		public int findRealCursorIndex(string text, int displayIndex) { // TODO:
			for (int i = displayIndex; i >= 0; i--) {
				if (i > text.Length) continue;
				if (findDisplayCursorIndex(text, i) <= displayIndex) return i;
			}

			return -1;
		}
	}

	public struct TypingUndo {
		public string text;
		public int cursorIndex;
	}

}