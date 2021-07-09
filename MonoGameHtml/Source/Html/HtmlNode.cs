﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpriteBatch = Microsoft.Xna.Framework.Graphics.SpriteBatch;
// ReSharper disable MemberCanBePrivate.Global

namespace MonoGameHtml {
	public class HtmlNode {
		
		// Primary info
		public string tag;
		public string textContent;
		public bool initialized;

		public HtmlNode parent;
		public HtmlNode[] children;
		public Func<HtmlNode[]> childrenFunc;
		
		public Dictionary<string, object> props;
		//public Dictionary<string, object> funcs; // TODO:
		
		public List<Action> actionList;
		
		// Position
		public PositionType position;
		public int x, y;
		public int width, height;

		public int paddingLeft, paddingRight, paddingTop, paddingBottom;
		public int marginLeft, marginRight, marginTop, marginBottom;

		public int PaddedX => x + marginLeft + borderWidth;
		public int PaddedY => y + marginTop + borderWidth;
		public Vector2 PaddedCorner => new Vector2(PaddedX, PaddedY);

		public int UnpaddedX => PaddedX + paddingLeft;
		public int UnpaddedY => PaddedY + paddingTop;
		public Vector2 UnpaddedCorner => new Vector2(UnpaddedX, UnpaddedY);
		public Vector2 UnpaddedDimens => new Vector2(width, height);
		
		public int PaddedWidth => width + paddingLeft + paddingRight;
		public int PaddedHeight => height + paddingTop + paddingBottom;
		public Vector2 PaddedDimens => new Vector2(PaddedWidth, PaddedHeight);

		public int BorderedWidth => PaddedWidth + 2 * borderWidth;
		public int BorderedHeight => PaddedHeight + 2 * borderWidth;

		public int FullWidth => BorderedWidth + marginLeft + marginRight;
		public int FullHeight => BorderedHeight + marginTop + marginBottom;

		
		// Text
		public string fontFamily = "JetbrainsMono";
		public int fontSize = 18;
		public SpriteFont font;
		public Vector2 textDimens = Vector2.Zero;
		// TODO: font weight

		// Appearance
		public int borderRadius;
		public int borderWidth;
		public Color borderColor = Color.Black;

		public Color backgroundColor = Color.Transparent, color = Color.Black, tint = Color.White;

		// TEXTURE SPECIFIC
		public Texture2D imgTexture;
		public TextureFitMode textureFitMode = TextureFitMode.none;
		
		// Layout
		public float flex;
		public AlignType alignX = AlignType.flexStart, alignY = AlignType.flexStart;
		public DirectionType flexDirection = DirectionType.column;
		public TextAlignType textAlign = TextAlignType.topLeft;

		// FUNCTIONS
		public bool hover;
		public Action onPress, onPressRemove, onMouseEnter, onMouseExit, onMouseMove, onMouseDrag, onHover, onTick, onMouseDown, onMouseUp; // TODO: add MouseInfo as parameter
		public Action<SpriteBatch> renderAdd;
		// mouse input
		public bool clicked;
		
		// ENUMS
		public enum TextAlignType {
			topLeft, center
		}
		public enum PositionType { // TODO: implement absolute, relative, and others
			normal, absolute, relative
		}

		public enum DirectionType {
			row, column
		}

		public enum AlignType { 
			flexStart, flexEnd, start, end, center, spaceBetween, spaceAround, spaceEvenly
		}

		public enum TextureFitMode { 
			none, cover, contain, fill
		}

		public HtmlNode(string tag, Dictionary<string, object> props = null, object textContent = null, HtmlNode[] children = null, Func<HtmlNode[]> childrenFunc = null) {
			this.tag = tag;
			this.props = props;
			this.props ??= new Dictionary<string, object>();

			initTextContent(textContent);
			this.children = children;
			this.childrenFunc = childrenFunc;

			makeParentOfChildren();

			if (childrenFunc != null) { 
				generateChildren();
			}
		}

		public void initTextContent(object textContent) { 
			if (textContent != null) {
				switch (textContent) {
					case string str:
						this.textContent = str;
						break;
					case Func<string> strFunc:

						Action generateText = () => {
							string initText = this.textContent;
							this.textContent = strFunc();
							if (initText != this.textContent) onFontChange();
						};

						generateText();
						bindAction(generateText);
						break;
				}
			}
		}

		public void makeParentOfChildren() {
			if (children != null) {
				foreach (HtmlNode child in children) {
					child.parent = this;
				}
			}
		}

		public void generateChildren() {
			children = childrenFunc();

			if (children != null) { 
				if (children.Contains(null)) { // remove null elements
					children = children.Where(child => child != null).ToArray();
				}

				if (children.Length == 0) {
					children = null;
					if (DynamicWidth) { // TODO: this seems like an error
						width = 0;
						onWidthChange();
					}

					if (DynamicHeight) {
						height = 0;
						onHeightChange(); 
					}
				}
			}

			if (children != null) {
				makeParentOfChildren();
				
				foreach (HtmlNode child in children) {
					if (!child.initialized) child.topDownInit();
				}
				findBase().bottomUpInit();
				
				findBase().layoutDown();
			}
		}

		public void stateChangeDown() { // TODO: perhaps find out if childrenFunc has any reliance on the changed state
			if (childrenFunc != null) { 
				generateChildren();
			} else if (children != null) {
				foreach (HtmlNode child in children) {
					child.stateChangeDown();
				}
			}
		}

		public void onWidthChange() {
			onResize();
		}
		
		public void onHeightChange() {
			onResize();
		}

		public HtmlNode findBase() {
			return (parent == null) ? this : parent.findBase();
		}

		public void onResize() { // TODO: sus? do i need to do a topDownInit()  ??????
			// TODO:
			HtmlNode baseNode = findBase();
			
			//baseNode.bottomUpInit(); stack overflow D:
			baseNode.layoutDown();
			
			if (props.ContainsKey("borderRadius")) { // TODO: abstract to method
				if (props["borderRadius"] is string str) {
					float mult = NodeUtil.percentAsFloat(str);
					borderRadius = (int) Math.Min(mult * FullWidth, mult * FullHeight);
				} else { 
					borderRadius = (int) props["borderRadius"];
				}
			}
		}
		
		public void onFontChange() {
			font = Fonts.getFontSafe(fontFamily, fontSize);
			textDimens = font.MeasureString(textContent);
			if (DynamicWidth) width = (int) textDimens.X;
			if (DynamicHeight) height = (int) textDimens.Y;

			//if (DynamicWidth || DynamicHeight) onResize();// TODO: FIGURE OUT WHAT THE HECK IS GOING ON
			onResize();
		}

		public bool propHasAny(string propName) {
			return (props.ContainsKey("-" + propName) || props.ContainsKey(propName));
		}

		public T prop<T>(string propIdentifier) {
			return (T) props[propIdentifier];
		}

		public Func<Color> toColorFunc(object func) {
			return func switch {
				Func<Color> colorFunc => colorFunc,
				Func<string> strFunc => () => NodeUtil.colorFromProp(strFunc()),
				Func<object> objFunc => () => NodeUtil.colorFromProp(objFunc()),
			};
		}

		public bool DynamicWidth => !propHasAny("dimens") && !propHasAny("width") && !(flexDirection == DirectionType.row && propHasAny("flex"));
		public bool DynamicHeight => !propHasAny("dimens") && !propHasAny("height") && !(flexDirection == DirectionType.column && propHasAny("flex"));

		public void bottomUpInit() {
			if (children != null) {
				foreach (HtmlNode child in children) {
					child.bottomUpInit();
				}
			}

			if (children != null && textContent == null) { 
				if (DynamicWidth) {
					width = flexDirection == DirectionType.row ? children.Select(child => child.FullWidth).Sum() : 
						children.Select(child => child.FullWidth).Max();
					onWidthChange();
				}
				if (DynamicHeight) {
					height = flexDirection == DirectionType.column ? children.Select(child => child.FullHeight).Sum() : 
						children.Select(child => child.FullHeight).Max();
					onHeightChange();
				}
			}
		}

		public void addCSSUnder(CSSDefinition cssDefinition) {
			foreach (string key in cssDefinition.styleProps.Keys) {
				if (!props.ContainsKey(key)) {
					props[key] = cssDefinition.styleProps[key];
				}
			}
		}

		public void addPropsOver(Dictionary<string, object> propDict) {
			if (propDict == null) return;
			foreach (string key in propDict.Keys) {
				props[key] = propDict[key];
			}
		}
		
		public void addPropsUnder(Dictionary<string, object> propDict) {
			if (propDict == null) return;
			foreach (string key in propDict.Keys.Where(key => !props.ContainsKey(key))) {
				props[key] = propDict[key];
			}
		}

		public void topDownInit() { // INITIALIZE USING PROPS (and such) ================= // TODO: convert to switch statement

			if (props.ContainsKey("props")) addPropsOver(prop<Dictionary<string, object>>("props"));
			if (props.ContainsKey("propsUnder")) addPropsUnder(prop<Dictionary<string, object>>("propsUnder"));
			
			// Load tag/class CSS (class has precedence over tags)
			if (props.ContainsKey("class") && CSSHandler.classes.ContainsKey(prop<string>("class"))) { // currently no support for dynamic class // TODO: ADD THIS
				CSSDefinition classDefinition = CSSHandler.classes[prop<string>("class")];
				addCSSUnder(classDefinition);
			}

			if (CSSHandler.tags.ContainsKey(tag)) {
				CSSDefinition tagDefinition = CSSHandler.tags[tag];
				addCSSUnder(tagDefinition);
			}

			processProps: {
				if (textContent == "") textContent = null;

				if (textContent == null && propHasAny("textContent")) {
					object textContentProp = null;
					if (props.ContainsKey("-textContent")) textContentProp = props["-textContent"];
					else if (props.ContainsKey("textContent")) textContentProp = props["textContent"];
					if (textContentProp != null) textContent = null;
					initTextContent(textContentProp);
					if (textContent == "") textContent = null;
				}

				if (textContent != null) font = Fonts.getFontSafe(fontFamily, fontSize); // default

				if (props.Keys.Count == 0) goto finishProps;
				
				if (props.ContainsKey("position")) position = Enum.Parse<PositionType>(prop<string>("position"));
				
				if (props.ContainsKey("x")) x = prop<int>("x");
				if (props.ContainsKey("y")) y = prop<int>("y");

				if (props.ContainsKey("dimens")) { 
					width = NodeUtil.widthFromProp(props["dimens"], parent);
					height = NodeUtil.heightFromProp(props["dimens"], parent);
				}

				// static dimensions
				void tryWidthProp(ref int widthVar, string propName) { 
					if (props.ContainsKey(propName)) widthVar = NodeUtil.widthFromProp(props[propName], parent);
				}
				void tryHeightProp(ref int heightVar, string propName) { 
					if (props.ContainsKey(propName)) heightVar = NodeUtil.heightFromProp(props[propName], parent);
				}

				tryWidthProp(ref width, "width");
				tryHeightProp(ref height, "height");
				
				if (props.ContainsKey("padding")) {
					object val = props["padding"];
					paddingLeft = NodeUtil.widthFromProp(val, parent);
					paddingRight = NodeUtil.widthFromProp(val, parent);
					paddingTop = NodeUtil.heightFromProp(val, parent);
					paddingBottom = NodeUtil.heightFromProp(val, parent);
				}
				
				if (props.ContainsKey("paddingBlock")) {
					object val = props["paddingBlock"];
					paddingTop = NodeUtil.heightFromProp(val, parent);
					paddingBottom = NodeUtil.heightFromProp(val, parent);
				}
				
				if (props.ContainsKey("paddingInline")) {
					object val = props["paddingInline"];
					paddingLeft = NodeUtil.widthFromProp(val, parent);
					paddingRight = NodeUtil.widthFromProp(val, parent);
				}
				
				tryWidthProp(ref paddingLeft, "paddingLeft");
				tryWidthProp(ref paddingRight, "paddingRight");
				tryHeightProp(ref paddingTop, "paddingTop");
				tryHeightProp(ref paddingBottom, "paddingBottom");
				
				if (props.ContainsKey("margin")) {
					object val = props["margin"];
					marginLeft = NodeUtil.widthFromProp(val, parent);
					marginRight = NodeUtil.widthFromProp(val, parent);
					marginTop = NodeUtil.heightFromProp(val, parent);
					marginBottom = NodeUtil.heightFromProp(val, parent);
				}
				
				if (props.ContainsKey("marginBlock")) {
					object val = props["marginBlock"];
					marginTop = NodeUtil.heightFromProp(val, parent);
					marginBottom = NodeUtil.heightFromProp(val, parent);
				}
				
				if (props.ContainsKey("marginInline")) {
					object val = props["marginInline"];
					marginLeft = NodeUtil.widthFromProp(val, parent);
					marginRight = NodeUtil.widthFromProp(val, parent);
				}
				
				tryWidthProp(ref marginLeft, "marginLeft");
				tryWidthProp(ref marginRight, "marginRight");
				tryHeightProp(ref marginTop, "marginTop");
				tryHeightProp(ref marginBottom, "marginBottom");

				if (props.ContainsKey("flex")) {
					if (parent.flexDirection == DirectionType.column) {
						if (!propHasAny("width") && !propHasAny("dimens")) {
							width = NodeUtil.widthFromProp("100%", parent);
						}
					} else if (parent.flexDirection == DirectionType.row) {
						if (!propHasAny("height") && !propHasAny("dimens")) {
							height = NodeUtil.heightFromProp("100%", parent);
						}
					}
					flex = Convert.ToSingle(props["flex"]);
				}

				if (props.ContainsKey("fontFamily")) fontFamily = (string) props["fontFamily"];
				if (props.ContainsKey("fontSize")) fontSize = (int) props["fontSize"];
				if (props.ContainsKey("textAlign")) textAlign = Enum.Parse<TextAlignType>((string) props["textAlign"]);
				
				if (textContent != null) onFontChange();
				
				if (props.ContainsKey("-fontSize")) {
					object funcProp = props["-fontSize"];
					if (funcProp is Func<int> intFunc) { 
						bindAction(() => {
							int initFontSize = fontSize;
							fontSize = intFunc();
							if (initFontSize != fontSize) onFontChange();
						});
					}
				}
				if (props.ContainsKey("-fontFamily")) {
					object funcProp = props["-fontFamily"];
					if (funcProp is Func<string> strFunc) { 
						bindAction(() => {
							string initFontFamily = fontFamily;
							fontFamily = strFunc();
							if (initFontFamily != fontFamily) onFontChange();
						});
					}
				}

				if (props.ContainsKey("-dimens")) { 
					object funcProp = props["-dimens"];
					switch (funcProp) {
						case Func<string> strFunc:
							bindAction(() => {
								int initWidth = width;
								int initHeight = height;
								string val = strFunc();
								width = NodeUtil.widthFromProp(val, parent);
								height = NodeUtil.heightFromProp(val, parent);
								if (width != initWidth || height != initHeight) onResize();
							});
							break;
						case Func<int> intFunc:
							bindAction(() => {
								int initWidth = width;
								int initHeight = height;
								int val = intFunc();
								width = val;
								height = val;
								if (width != initWidth || height != initHeight) onResize();
							});
							break;
					}
				}

				if (props.ContainsKey("-flex")) {
					object flexFuncProp = props["-flex"];
					if (flexFuncProp is Func<float> floatFunc) { 
						bindAction(() => {
							float initFlex = flex;
							flex = floatFunc();
							if (initFlex != flex) {
								if (parent.flexDirection == DirectionType.column) {
									if (!propHasAny("width") && !propHasAny("dimens")) {
										width = NodeUtil.widthFromProp("100%", parent);
									}
								} else if (parent.flexDirection == DirectionType.row) {
									if (!propHasAny("height") && !propHasAny("dimens")) {
										height = NodeUtil.heightFromProp("100%", parent);
									}
								}
								onResize();
							}
						});
					}
				}

				
				
				// dynamic width/heights
				if (props.ContainsKey("-width")) {
					object widthFuncProp = props["-width"];
					if (widthFuncProp is Func<string> strFunc) {
						bindAction(() => {
							int initWidth = width;
							width = NodeUtil.widthFromProp(strFunc(), parent);
							if (initWidth != width) onWidthChange();
						});
					} else if (widthFuncProp is Func<int> intFunc) { 
						bindAction(() => {
							int initWidth = width;
							width = intFunc();
							if (initWidth != width) onWidthChange();
						});
					}
				}
				
				
				if (props.ContainsKey("-height")) {
					object funcProp = props["-height"];
					Func<int> func = funcProp switch {
						Func<int> intFunc => intFunc,
						Func<string> strFunc => () => NodeUtil.heightFromProp(strFunc(), parent),
						Func<object> objFunc => () => NodeUtil.heightFromProp(objFunc(), parent),
					};
					bindAction(() => {
						int initHeight = height;
						height = func();
						if (initHeight != height) onHeightChange();
					});
				}


				if (props.ContainsKey("borderRadius")) {
					if (props["borderRadius"] is string str) {
						float mult = NodeUtil.percentAsFloat(str);
						borderRadius = (int) Math.Min(mult * FullWidth, mult * FullHeight);
					} else { 
						borderRadius = (int) props["borderRadius"];
					}
				}

				if (props.ContainsKey("renderAdd")) renderAdd = prop<Action<SpriteBatch>>("renderAdd");
				if (props.ContainsKey("onPress")) onPress = prop<Action>("onPress");
				if (props.ContainsKey("onPressRemove")) onPressRemove = prop<Action>("onPressRemove");
				if (props.ContainsKey("onMouseMove")) onMouseMove = prop<Action>("onMouseMove");
				if (props.ContainsKey("onMouseDrag")) onMouseDrag = prop<Action>("onMouseDrag");
				if (props.ContainsKey("onMouseEnter")) onMouseEnter = prop<Action>("onMouseEnter");
				if (props.ContainsKey("onMouseExit")) onMouseExit = prop<Action>("onMouseExit");
				if (props.ContainsKey("onHover")) onHover = prop<Action>("onHover");
				if (props.ContainsKey("onTick")) onTick = prop<Action>("onTick");
				if (props.ContainsKey("onMouseDown")) onMouseDown = prop<Action>("onMouseDown");
				if (props.ContainsKey("onMouseUp")) onMouseUp = prop<Action>("onMouseUp");


				if (props.ContainsKey("borderWidth")) borderWidth = prop<int>("borderWidth");
				if (props.ContainsKey("-borderWidth")) { 
					object funcProp = props["-borderWidth"];
					if (funcProp is Func<int> func) { 
						bindAction(() => {
							int initBorderWidth = borderWidth;
							borderWidth = func();
							if (initBorderWidth != borderWidth) onResize();
						});
					}
				}

				if (props.ContainsKey("borderColor")) borderColor = NodeUtil.colorFromProp(props["borderColor"]);
				if (props.ContainsKey("backgroundColor")) backgroundColor = NodeUtil.colorFromProp(props["backgroundColor"]);
				if (props.ContainsKey("color")) color = NodeUtil.colorFromProp(props["color"]);
				if (props.ContainsKey("tint")) tint = NodeUtil.colorFromProp(props["tint"]);

				if (props.ContainsKey("-backgroundColor")) {
					var func = toColorFunc(props["-backgroundColor"]);
					bindAction(() => backgroundColor = func());
				}
				if (props.ContainsKey("-color")) {
					var func = toColorFunc(props["-color"]);
					bindAction(() => color = func());
				}
				if (props.ContainsKey("-borderColor")) {
					var func = toColorFunc(props["-borderColor"]);
					bindAction(() => borderColor = func());
				}
				
				if (props.ContainsKey("-tint")) {
					var func = toColorFunc(props["-tint"]);
					bindAction(() => tint = func());
				}

				align: { 
					if (props.ContainsKey("flexDirection")) flexDirection = Enum.Parse<DirectionType>((string) props["flexDirection"]);

					AlignType alignMain = AlignType.flexStart, alignSub = AlignType.start;

					if (props.ContainsKey("justifyContent")) alignMain = Enum.Parse<AlignType>((string) props["justifyContent"]);
					if (props.ContainsKey("alignItems")) alignSub = Enum.Parse<AlignType>((string) props["alignItems"]);
					
					if (flexDirection == DirectionType.column) {
						alignX = alignSub;
						alignY = alignMain;
					} else { 
						alignX = alignMain;
						alignY = alignSub;
					}

					if (props.ContainsKey("align")) { 
						var val = Enum.Parse<AlignType>(prop<string>("align"));
						alignX = val;
						alignY = val;
					}
					
					if (props.ContainsKey("alignX")) { 
						alignX = Enum.Parse<AlignType>(prop<string>("alignX"));
						if (alignX != AlignType.start && alignX != AlignType.center && alignX != AlignType.end) { 
							flexDirection = DirectionType.row;
						}
					}
					
					if (props.ContainsKey("alignY")) { 
						alignY = Enum.Parse<AlignType>(prop<string>("alignY"));
						if (alignY != AlignType.start && alignY != AlignType.center && alignY != AlignType.end) {
							flexDirection = DirectionType.column;
						}
					}
				}

				
				// TEXTURE SPECIFIC PROPS
				if (tag == "texture" || tag == "img") {
					if (props.ContainsKey("objectFit")) {
						textureFitMode = Enum.Parse<TextureFitMode>(prop<string>("objectFit"));
					} else if (props.ContainsKey("-objectFit")) { 
						var strFunc = prop<Func<string>>("-objectFit");
						bindAction(() => textureFitMode = Enum.Parse<TextureFitMode>(prop<string>(strFunc())));
					}

					if (props.ContainsKey("src")) {
						imgTexture = prop<Texture2D>("src");
						if (DynamicWidth && DynamicHeight) { 
							width = imgTexture.Width;
							height = imgTexture.Height;
							onResize();
						}
					} else if (props.ContainsKey("-src")) {
						var imgFunc = prop<Func<Texture2D>>("-src");
						bindAction(() => { 
							Texture2D initTexture = imgTexture;
							imgTexture = imgFunc();
							// check if changing width/height is necessary
							if (initTexture != imgTexture && (DynamicWidth || DynamicHeight) 
							                              && (initTexture.Width != imgTexture.Width || initTexture.Height != imgTexture.Height)) {
								width = imgTexture.Width;
								height = imgTexture.Height;
								onResize();
							}
						});
					}
				}

				
				// REF function
				if (props.ContainsKey("ref")) {
					prop<Action<HtmlNode>>("ref")(this);
				}
			} finishProps: { }

			initialized = true;
			
			if (children != null) {
				foreach (HtmlNode child in children) {
					child.topDownInit();
				}
			}
		}

		public void layoutDown() {
			if (children == null) return;

			int sumWidth = 0, sumHeight = 0;
			float sumFlex = 0;
			foreach (HtmlNode child in children) {
				sumWidth += child.FullWidth;
				sumHeight += child.FullHeight;
				sumFlex += child.flex;
			}

			int x = UnpaddedX;
			int y = UnpaddedY;

			if (flexDirection == DirectionType.column || sumFlex < 0.0001F) {
				switch (alignX) {
					case AlignType.spaceBetween: {

						if (children.Length == 1) {
							children[0].x = x;
							break;
						}

						int gap = (width - sumWidth) / (children.Length - 1);
						int thisX = x;
						foreach (HtmlNode child in children) {
							child.x = thisX;
							thisX += child.FullWidth + gap;
						}
						break;
					}
					case AlignType.spaceAround: {

						int gap = (width - sumWidth) / (children.Length);
						int thisX = x + gap / 2;
						foreach (HtmlNode child in children) {
							child.x = thisX;
							thisX += child.FullWidth + gap;
						}
						break;
					}
					case AlignType.spaceEvenly: {

						int gap = (width - sumWidth) / (children.Length + 1);
						int thisX = x + gap;
						foreach (HtmlNode child in children) {
							child.x = thisX;
							thisX += child.FullWidth + gap;
						}
						break;
					}
					case AlignType.center: {
						if (flexDirection == DirectionType.column) {
							foreach (HtmlNode child in children) {
								child.x = (x + width / 2) - child.FullWidth / 2;
							}
							break;
						}

						int thisX = (x + width / 2) - sumWidth / 2;
						foreach (HtmlNode child in children) {
							child.x = thisX;
							thisX += child.FullWidth;
						}
						break;
					}
					case AlignType.start: { 
						foreach (HtmlNode child in children) {
							child.x = x;
						}
						break;
					}
					case AlignType.end: { 
						foreach (HtmlNode child in children) {
							child.x = x + width - child.FullWidth;
						}
						break;
					}
					case AlignType.flexStart: {
						if (flexDirection == DirectionType.row) { 
							int thisX = x;
							foreach (HtmlNode child in children) {
								child.x = thisX;
								thisX += child.FullWidth;
							}
						} else { 
							foreach (HtmlNode child in children) {
								child.x = x;
							}
						}
						break;
					}
					case AlignType.flexEnd: { 
						if (flexDirection == DirectionType.row) { 
							int thisX = x + width;
							for (int i = children.Length - 1; i >= 0; i--) {
								thisX -= children[i].FullWidth;
								children[i].x = thisX;
							}
						} else { 
							foreach (HtmlNode child in children) {
								child.x = x + width - child.FullWidth;
							}
						}
						
						break;
					}
				}
			} else { // TODO: FIX THIS WHAT IS THIS WHY Are the ifs laid out like this its horrible

				int nonFlexWidth = 0;
				foreach (HtmlNode child in children) {
					nonFlexWidth += (child.flex <= 0) ? child.FullWidth : (child.FullWidth - child.width);
				}
				
				float perFlex = (width - nonFlexWidth) / sumFlex;

				int thisX = x;
				foreach (HtmlNode child in children) {
					child.x = thisX;
					if (child.flex > 0) child.width = (int) (perFlex * child.flex);
					thisX += child.FullWidth;
				}
			}

			if (flexDirection == DirectionType.row || sumFlex < 0.0001F) { 
				switch (alignY) {
					case AlignType.spaceBetween: {

						if (children.Length == 1) {
							children[0].y = y;
							break;
						}

						int gap = (height - sumHeight) / (children.Length - 1);
						int thisY = y;
						foreach (HtmlNode child in children) {
							child.y = thisY;
							thisY += child.FullHeight + gap;
						}
						break;
					}
					case AlignType.spaceAround: {

						int gap = (height - sumHeight) / (children.Length);
						int thisY = y + gap / 2;
						foreach (HtmlNode child in children) {
							child.y = thisY;
							thisY += child.FullHeight + gap;
						}
						break;
					}
					case AlignType.spaceEvenly: {

						int gap = (height - sumHeight) / (children.Length + 1);
						int thisY = y + gap;
						foreach (HtmlNode child in children) {
							child.y = thisY;
							thisY += child.FullHeight + gap;
						}
						break;
					}
					case AlignType.center: {
						if (flexDirection == DirectionType.row) {
							foreach (HtmlNode child in children) {
								child.y = (y + height / 2) - child.FullHeight / 2;
							}
							break;
						}

						int thisY = (y + height / 2) - sumHeight / 2;
						foreach (HtmlNode child in children) {
							child.y = thisY;
							thisY += child.FullHeight;
						}
						break;
					}
					case AlignType.start: { 
						foreach (HtmlNode child in children) {
							child.y = y;
						}
						break;
					}
					case AlignType.end: { 
						foreach (HtmlNode child in children) {
							child.y = y + height - child.FullHeight;
						}
						break;
					}
					case AlignType.flexStart: {
						if (flexDirection == DirectionType.column) { 
							int thisY = y;
							foreach (HtmlNode child in children) {
								child.y = thisY;
								thisY += child.FullHeight;
							}
						} else { 
							foreach (HtmlNode child in children) {
								child.y = y;
							}
						}
						break;
					}
					case AlignType.flexEnd: { 
						if (flexDirection == DirectionType.column) { 
							int thisY = y + height;
							for (int i = children.Length - 1; i >= 0; i--) {
								thisY -= children[i].FullHeight;
								children[i].y = thisY;
							}
						} else { 
							foreach (HtmlNode child in children) {
								child.y = y + height - child.FullHeight;
							}
						}
						break;
					}
				}
			} else {
				int noneFlexHeight = 0;
				foreach (HtmlNode child in children) {
					noneFlexHeight += (child.flex <= 0) ? child.FullHeight : (child.FullHeight - child.height);
				}
				
				float perFlex = (height - noneFlexHeight) / sumFlex;

				int thisY = x;
				foreach (HtmlNode child in children) {
					child.y = thisY;
					if (child.flex > 0) child.height = (int) (perFlex * child.flex);
					thisY += child.FullHeight;
				}
			}

			foreach (HtmlNode child in children) {
				child.layoutDown();
			}
		}

		public override string ToString() {
			return $"{tag}Node{{{(props != null ? "props: "+Util.stringifyDict(props) : "")}{(textContent != null ? " textContent: " + textContent : "")}{(children != null ? " children: " + stringifyChildren() : "")}}}";
		}

		public string stringifyChildren() {
			string str = "[";
			for (int i = 0; i < children.Length; i++) {
				str += children[i].ToString() + ((i + 1 < children.Length) ? ", " : "");
			}
			
			return str + "]";
		}

		public void bindAction(Action action) {
			if (actionList == null) actionList = new List<Action>();
			
			actionList.Add(action);
		}

		internal void update(float deltaTime, MouseInfo mouse) {
			
			onTick?.Invoke();
			
			if (actionList != null) {
				foreach (Action action in actionList) {
					action.Invoke();
				}
			}

			if (posInside(mouse.pos)) {
				bool diff = !hover;
				hover = true;
				if (diff) onMouseEnter?.Invoke();
				
				onHover?.Invoke();
			} else if (hover) {
				hover = false;
				onMouseExit?.Invoke();
			}

			if (children != null) { 
				foreach (HtmlNode child in children) {
					child.update(deltaTime, mouse);
				}
			}
		}

		public bool posInside(Vector2 vec) {
			int x = PaddedX, y = PaddedY, width = PaddedWidth, height = PaddedHeight;
			return vec.X > x && vec.Y > y && vec.X < x + width && vec.Y < y + height;
		}

		public void topClicked(Vector2 vec) { 
			// TODO:
		}
		
		public void onClick(Vector2 vec) {
			clicked = true;
			onPress?.Invoke();
		}

		public void mouseUp() {
			bool wasClicked = clicked;
			clicked = false;
			if (wasClicked) onPressRemove?.Invoke();
		}

		public bool clickRecurse(Vector2 vec) {

			if (posInside(vec)) { // TODO: redo? def not how this works
				bool final = true;
				if (children != null) { 
					foreach (HtmlNode child in children) {
						if (child.clickRecurse(vec)) {
							final = false;
						}
					}
				}
				if (final) topClicked(vec);

				onClick(vec);
				return true;
			}

			return false;
		}

		public void recurse(Action<HtmlNode> nodeAction) {
			nodeAction.Invoke(this);
			
			if (children == null) return;
			foreach (HtmlNode node in children) {
				node.recurse(nodeAction);
			}
		}

		public void tryRenderText(SpriteBatch spriteBatch) {
			
			if (textContent == null) return;
			if (color == Color.Transparent) return;

			Vector2 pos = UnpaddedCorner;
			
			if (font == null) {
				Warnings.log("NULL FONT:", ToString());
				return;
			}
			spriteBatch.DrawString(font, textContent, 
				(textAlign == TextAlignType.topLeft) ? pos : pos + UnpaddedDimens/2F - textDimens/2F, color);
		}
		
		public void tryRenderImage(SpriteBatch spriteBatch) {
			
			if (imgTexture == null) return;
			
			switch (textureFitMode) {
				case TextureFitMode.none:
				case TextureFitMode.fill: { 
					Rectangle rect = new Rectangle(UnpaddedX, UnpaddedY, width, height);
					spriteBatch.Draw(imgTexture, rect, tint);
					break;
				}
				case TextureFitMode.cover: {
					// TODO:
					break;
				}
				case TextureFitMode.contain: {
					// TODO:
					break;
				}
			}
		}

		public void renderSelf(SpriteBatch spriteBatch) {

			int x = PaddedX;
			int y = PaddedY;
			int width = PaddedWidth;
			int height = PaddedHeight;
			
			// draw border
			if (borderWidth != 0 && borderColor != Color.Transparent) {
				if (borderRadius == 0) { 
					int doubleBorder = 2 * borderWidth;
					spriteBatch.Draw(Textures.rect, new Rectangle(x - borderWidth, y - borderWidth, width + doubleBorder, borderWidth), borderColor);
					spriteBatch.Draw(Textures.rect, new Rectangle(x - borderWidth, y, borderWidth, height), borderColor);
					spriteBatch.Draw(Textures.rect, new Rectangle(x + width, y, borderWidth, height), borderColor);
					spriteBatch.Draw(Textures.rect, new Rectangle(x - borderWidth, y + height, width + doubleBorder, borderWidth), borderColor);
				} else {
					
					int diameter = borderRadius * 2;
					spriteBatch.Draw(Textures.rect, new Rectangle(x + borderRadius, y - borderWidth, width - diameter, borderWidth), borderColor);
					spriteBatch.Draw(Textures.rect, new Rectangle(x - borderWidth, y+borderRadius, borderWidth, height-diameter), borderColor);
					spriteBatch.Draw(Textures.rect, new Rectangle(x + width, y + borderRadius, borderWidth, height - diameter), borderColor);
					spriteBatch.Draw(Textures.rect, new Rectangle(x + borderRadius, y + height, width - diameter, borderWidth), borderColor);

					if (backgroundColor == Color.Transparent) { 
						void drawArc(Vector2 center, float radius, float startAngle, float endAngle, int points) {
						
							float diff = (endAngle - startAngle) / (points - 1);
							for (int i = 1; i < points; i++) {
								Vector2 p1 = center + Util.polar(radius, startAngle + diff * (i - 1));
								Vector2 p2 = center + Util.polar(radius, startAngle + diff * i);
							
								Util.drawLineScreen(p1, p2, spriteBatch, borderColor, borderWidth);
							}
						}
						const int points = 25;
						float rad = borderRadius + borderWidth / 2F;
						drawArc(new Vector2(x+borderRadius, y+borderRadius), rad, 
							-Maths.halfPI, -Maths.PI, points);
						drawArc(new Vector2(x+width-borderRadius, y+borderRadius), rad, 
							0, -Maths.halfPI, points);
						drawArc(new Vector2(x+borderRadius, y+height-borderRadius), rad, 
							-Maths.PI, -Maths.PI * 3/2F, points);
						drawArc(new Vector2(x+width-borderRadius, y+height-borderRadius), rad, 
							-Maths.PI * 3/2F, -Maths.twoPI, points);
					} else {
						int doubleBorder = borderWidth * 2;
						spriteBatch.Draw(Textures.circle, new Rectangle(x-borderWidth, y-borderWidth,diameter, diameter+doubleBorder), borderColor);
						spriteBatch.Draw(Textures.circle, new Rectangle(x + width - diameter-borderWidth, y-borderWidth,diameter+doubleBorder, diameter+doubleBorder), borderColor);
						spriteBatch.Draw(Textures.circle, new Rectangle(x-borderWidth, y + height - diameter-borderWidth,diameter+doubleBorder, diameter+doubleBorder), borderColor);
						spriteBatch.Draw(Textures.circle, new Rectangle(x + width - diameter-borderWidth, y + height - diameter-borderWidth,diameter+doubleBorder, diameter+doubleBorder), borderColor);
					}
				}
			}
			
			//spriteBatch.Draw(Textures.rect, new Rectangle(x + width, y + height, 10, 10), Color.Red);

			
			// Draw background color
			if (backgroundColor != Color.Transparent) {
				if (borderRadius == 0) {
					Rectangle renderRect = new Rectangle(x, y, width, height);
					spriteBatch.Draw(Textures.rect, renderRect, backgroundColor);
				} else { 
					int diameter = borderRadius * 2;
			
					spriteBatch.Draw(Textures.rect, new Rectangle(x + borderRadius, y, width - diameter, borderRadius), backgroundColor);
					spriteBatch.Draw(Textures.rect, new Rectangle(x, y + borderRadius, width, height - diameter), backgroundColor);
					spriteBatch.Draw(Textures.rect, new Rectangle(x + borderRadius, y + height - borderRadius, width - diameter, borderRadius), backgroundColor);
			
					spriteBatch.Draw(Textures.circle, new Rectangle(x, y,diameter, diameter), backgroundColor);
					spriteBatch.Draw(Textures.circle, new Rectangle(x + width - diameter, y,diameter, diameter), backgroundColor);
					spriteBatch.Draw(Textures.circle, new Rectangle(x, y + height - diameter,diameter, diameter), backgroundColor);
					spriteBatch.Draw(Textures.circle, new Rectangle(x + width - diameter, y + height - diameter,diameter, diameter), backgroundColor);
				}
			}
			
			tryRenderText(spriteBatch);

			tryRenderImage(spriteBatch);

			renderAdd?.Invoke(spriteBatch);
		}

		public void render(SpriteBatch spriteBatch) { 
			
			renderSelf(spriteBatch);
			
			if (children != null) { 
				foreach (HtmlNode child in children) {
					child.render(spriteBatch);
				}
			}
		}
	}
}