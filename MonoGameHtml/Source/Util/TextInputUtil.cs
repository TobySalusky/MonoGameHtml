using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using FontStashSharp;
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

			typingState.latestKeyInfo = keys;
			
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
	            if (typingState.HasRealSelection()) {
		            int minIndex = Math.Min(typingState.cursorIndex, typingState.selectStartIndex);
		            int maxIndex = Math.Max(typingState.cursorIndex, typingState.selectStartIndex);
		            text = text[..minIndex] + str + text[maxIndex..];
		            typingState.cursorIndex = minIndex;
	            }
	            else {
		            text = beforeCursor() + str + afterCursor();
		            typingState.cursorIndex += str.Length;
	            }
	            typingState.Deselect();
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

            void ArrowSelect() {
	            if (shift) {
		            if (!typingState.hasSelection) typingState.StartSelection();
	            }
	            else {
		            typingState.Deselect();
	            }
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
			            if (typingState.HasRealSelection()) {
				            int minIndex = Math.Min(typingState.cursorIndex, typingState.selectStartIndex);
				            int maxIndex = Math.Max(typingState.cursorIndex, typingState.selectStartIndex);
				            text = text[..minIndex] + text[maxIndex..];
				            typingState.cursorIndex = minIndex;
			            }
			            else {
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
			            typingState.Deselect();
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
		            ArrowSelect();

		            if (control) {
			            const string deleteUntilRegex = "[\n\\s]\\S";
			            var matches = Regex.Matches(beforeCursor(), deleteUntilRegex);
			            int index = matches.Any() ? matches.Max(match => match.Index + 1) : 0;
			            typingState.cursorIndex = index;
		            }
		            else {
			            typingState.cursorIndex = Math.Max(0, typingState.cursorIndex - 1);
		            }
	            } else if (key == Keys.Right) {
		            ArrowSelect();
		            
		            if (control) {
			            const string deleteUntilRegex = "\\S[\n\\s]";
			            var matches = Regex.Matches(afterCursor(), deleteUntilRegex);
			            int index = matches.Any() ? matches.Min(match => match.Index + 1) : 0;

			            int init = typingState.cursorIndex;
			            typingState.cursorIndex = index + beforeCursor().Length;
			            if (init == typingState.cursorIndex) typingState.cursorIndex = text.Length;
		            }
		            else {
			            typingState.cursorIndex = Math.Min(text.Length, typingState.cursorIndex + 1);
		            }
	            } else if (key == Keys.Up) {
		            ArrowSelect();
		            
		            setCursorFromPos(
			            cursorPositionAtIndex(typingState.node, typingState, text, typingState.cursorIndex) 
			            - Vector2.UnitY * typingState.node.font.LineHeight, 
			            typingState.node, typingState, text);
	            } else if (key == Keys.Down) {
		            ArrowSelect();
		            
		            setCursorFromPos(
			            cursorPositionAtIndex(typingState.node, typingState, text, typingState.cursorIndex) 
			            + Vector2.UnitY * typingState.node.font.LineHeight, 
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

            typingState.cursorIndex = Math.Clamp(typingState.cursorIndex, 0, text.Length);

            if (typingState.hasSelection) {
	            typingState.prevSelection = (typingState.selectStartIndex, typingState.cursorIndex);
	            typingState.hadPrevSelection = true;
            } else {
	            typingState.hadPrevSelection = false;
            }

            return text;
		}

		public static Vector2 cursorPositionAtIndex(HtmlNode node, TypingState typingState, string realText, int cursorIndex) {
			SpriteFontBase font = node.font;
			float height = font.LineHeight;
			string text = node.textContent[..Math.Min(node.textContent.Length, typingState.findDisplayCursorIndex(realText, cursorIndex))];
			float x = node.UnpaddedX + font.MeasureString(text[Math.Max(0, text.lastIndexOf("\n"))..]).X;
			float y = node.UnpaddedY + Math.Max(0, font.MeasureStringCorrectly(text).Y - height);
			
			return new Vector2(x, y);
		}

		public static void drawSelection(SpriteBatch spriteBatch, HtmlNode node, TypingState typingState, string realText, Color color) {
			void HighlightSegment(int index1, int index2) {
				Vector2 cursorPos = cursorPositionAtIndex(node, typingState, realText, index1);
				Vector2 cursorPos2 = cursorPositionAtIndex(node, typingState, realText, index2);
				spriteBatch.Draw(Textures.rect, new Rectangle((int) cursorPos.X, (int) cursorPos.Y, (int)(cursorPos2.X - cursorPos.X), node.font.LineHeight + (int)(cursorPos2.Y - cursorPos.Y)), color);
			}
			
			void HighlightSpace(int index1) {
				Vector2 cursorPos = cursorPositionAtIndex(node, typingState, realText, index1);
				Vector2 cursorPos2 = cursorPos + Vector2.UnitX * node.font.MeasureString(" ").X;
				spriteBatch.Draw(Textures.rect, new Rectangle((int) cursorPos.X, (int) cursorPos.Y, (int)(cursorPos2.X - cursorPos.X), node.font.LineHeight + (int)(cursorPos2.Y - cursorPos.Y)), color);
			}
			
			int minIndex = Math.Min(typingState.selectStartIndex, typingState.cursorIndex);
			int maxIndex = Math.Max(typingState.selectStartIndex, typingState.cursorIndex);

			string[] lines = realText[minIndex..maxIndex].SplitLines();

			int startNextIndex = 0;
			foreach (var (line, i) in lines.Enumerate()) {
				if (i == 0) {
					startNextIndex = minIndex + line.Length;
					HighlightSegment(minIndex, startNextIndex);
				}
				else {
					int tempStart = startNextIndex + 1;
					startNextIndex = tempStart + line.Length;
					if (line == "") {
						HighlightSpace(tempStart);
					}
					else {
						HighlightSegment(tempStart, startNextIndex);
					}
				}
			}
		}

		public static void drawCursor(SpriteBatch spriteBatch, HtmlNode node, TypingState typingState, string realText) {
			Vector2 cursorPos = cursorPositionAtIndex(node, typingState, realText, typingState.cursorIndex);
			spriteBatch.Draw(Textures.rect, new Rectangle((int) cursorPos.X, (int) cursorPos.Y, 2, node.font.LineHeight), node.color);
		}

		public static void Click(Vector2 pos, HtmlNode node, TypingState typingState, string realText) {
			setCursorFromPos(pos, node, typingState, realText);
			typingState.StartSelection();

			var newClick = new TypingClickInfo(typingState.time, typingState.cursorIndex, realText);
			
			if (typingState.lastClick.cursorIndex == newClick.cursorIndex && typingState.lastClick.text == newClick.text &&
			    typingState.lastClick.time >= newClick.time - 0.5F) {
				typingState.clickStreak++;
				if (typingState.clickStreak == 1) {
					
					(typingState.selectStartIndex, typingState.cursorIndex) = ExpandSelectRangeFromIndex(realText, typingState.cursorIndex);
					if (typingState.cursorIndex - typingState.selectStartIndex <= 0) {
						typingState.selectStartIndex = typingState.cursorIndex;
						typingState.cursorIndex = Math.Min(typingState.cursorIndex + 1, realText.Length);
					}
				} else if (typingState.clickStreak >= 2) {
					(typingState.selectStartIndex, typingState.cursorIndex) = LineIndexToRange(realText, IndexToLineIndex(realText, typingState.cursorIndex));
				}
			}
			else {
				typingState.clickStreak = 0;
			}

			typingState.lastClick = newClick;
		}

		public static (int, int) ExpandSelectRangeFromIndex(string realText, int index) {
			var (lineIndex, indexOnLine) = IndexToLineIndexAndIndexOnLine(realText, index);

			string line = realText.SplitLines()[lineIndex];

			int startLess = 0;
			int endMore = 0;
			for (int i = indexOnLine - 1; i >= 0; i--) {
				if (!line[i].IsWhiteSpace()) startLess++;
				else break;
			}
			for (int i = indexOnLine + 1; i < line.Length; i++) {
				if (!line[i].IsWhiteSpace()) endMore++;
				else break;
			}

			int charsBefore = LineIndexToCharactersBefore(realText, lineIndex);
			return (index - startLess, Math.Min(index + endMore + 1, realText.Length));
		}

		
		public static int IndexToLineIndex(string realText, int index) => realText[..index].CountOf("\n");
		public static (int, int) IndexToLineIndexAndIndexOnLine(string realText, int index) {
			int lineIndex = IndexToLineIndex(realText, index);

			return (lineIndex, index - LineIndexToCharactersBefore(realText, lineIndex));
		}

		public static int LineIndexToCharactersBefore(string realText, int lineIndex) => realText.SplitLines().Where((line, i) => i < lineIndex).Aggregate(0, (sum, str) => sum + str.Length + 1);



		public static (int, int) LineIndexToRange(string realText, int lineIndex) {
			var lines = realText.SplitLines();
			int start = LineIndexToCharactersBefore(realText, lineIndex);
			return (start, start + lines[lineIndex].Length);
		}

		public static void setCursorFromPos(Vector2 pos, HtmlNode node, TypingState typingState, string realText) {
			string text = node.textContent;
			SpriteFontBase font = node.font;

			Vector2 start = node.UnpaddedCorner;
			
			// find which line the position corresponds to
			float height = font.LineHeight;
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
		public bool hasSelection = false;
		public int selectStartIndex;
		public int cursorIndex;
		public bool multiline;
		public readonly Stack<TypingUndo> undos = new Stack<TypingUndo>();
		public Func<string, string, string> diff;
		public float time, deltaTime, lastUndoTime, undoFrequency, lastEditOrMove;
		public readonly Dictionary<Keys, float> keyDownTime = new Dictionary<Keys, float>();
		public TypingClickInfo lastClick;
		public int clickStreak = 0;
		public bool hadPrevSelection;
		public (int start, int end) prevSelection;
		public KeyInfo latestKeyInfo;

		public void Deselect() {
			hasSelection = false;
		}

		public void StartSelection() {
			hasSelection = true;
			selectStartIndex = cursorIndex;
		}

		public bool HadRealPrevSelection() {
			return hadPrevSelection && prevSelection.start != prevSelection.end;
		}
		
		public bool HasRealSelection() {
			return hasSelection && selectStartIndex != cursorIndex;
		}

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

	public struct TypingClickInfo {
		public float time;
		public int cursorIndex;
		public string text;

		public TypingClickInfo(float time, int cursorIndex, string text) {
			this.time = time;
			this.cursorIndex = cursorIndex;
			this.text = text;
		}
	}
}