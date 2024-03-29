﻿﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
  using FontStashSharp;
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

		public HtmlNode[] Contents => (childrenFunc != null) ? childrenFunc() : children;
		
		public Dictionary<string, object> props;
		//public Dictionary<string, object> funcs; // TODO:
		
		public List<Action> actionList;
		
		// Position
		public PositionType position;
		public int x, y;
		public int width, height;

		public int? left, right, top, bottom;

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
		public string fontFamily = "___fallback";
		public int fontSize = 18;
		public SpriteFontBase font;
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
		public PositionType positionType = PositionType.STATIC;

		// FUNCTIONS
		public bool hover;
		public Action onPress, onPressRemove, onMouseEnter,
			onMouseExit, onMouseMove, onMouseDrag, onHover,
			onTick, onMouseDown, onMouseUp; // TODO: add MouseInfo as parameter

		public Action<HtmlNode> onResize;
		public Action<SpriteBatch> renderAdd;
		// mouse input
		public bool clicked;
		
		// ENUMS
		public enum TextAlignType {
			topLeft, center
		}
		public enum PositionType { // TODO: implement absolute, relative, and others
			STATIC, RELATIVE, FIXED, ABSOLUTE
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
			if (this.children != null && !this.children.Any()) this.children = null;
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
							try {
								string initText = this.textContent;
								this.textContent = strFunc();
								if (initText != this.textContent) onFontChange();

							} catch (Exception e) { 
								Logger.logColor(ConsoleColor.Cyan, "NO!!", e, e.Message, e.StackTrace);
							}
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
				
				triggerOnResize();

				foreach (var child in children) {
					//Logger.log(tag, child.tag, child.width, child.height);
				}
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
			triggerOnResize();
		}
		
		public void onHeightChange() {
			triggerOnResize();
		}

		public HtmlNode findBase() {
			return (parent == null) ? this : parent.findBase();
		}

		public void triggerOnResize() { // TODO: sus? do i need to do a topDownInit()  ??????
			// TODO:
			HtmlNode baseNode = findBase();

			if (children != null) {
				foreach (HtmlNode child in children) {
					bool resizeFlag = false;
					
					if (child.props.ContainsKey("dimens") && child.props["dimens"] is string percentDimensStr) {
						child.width = NodeUtil.widthFromProp(percentDimensStr, this);
						child.height = NodeUtil.heightFromProp(percentDimensStr, this);
						resizeFlag = true;
					}
					
					if (child.props.ContainsKey("width") && child.props["width"] is string percentWidthStr) {
						child.width = NodeUtil.widthFromProp(percentWidthStr, this);
						resizeFlag = true;
					}
				
					if (child.props.ContainsKey("height") && child.props["height"] is string percentHeightStr) {
						child.height = NodeUtil.heightFromProp(percentHeightStr, this);
						resizeFlag = true;
					}
				
					if (resizeFlag) child.triggerOnResize();
				}
			}

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
			
			onResize?.Invoke(this);
		}
		
		public void onFontChange() {
			font = Fonts.MonoGameHtmlFonts.getFontSafe(fontFamily, fontSize);
			textDimens = font.MeasureStringCorrectly(textContent);
			if (DynamicWidth) width = (int) textDimens.X;
			if (DynamicHeight) height = (int) textDimens.Y;

			//if (DynamicWidth || DynamicHeight) onResize();// TODO: FIGURE OUT WHAT THE HECK IS GOING ON
			triggerOnResize();
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
					width = flexDirection == DirectionType.row ? children.Where(child => child.contributesParentSize()).Select(child => child.FullWidth).Sum() : 
						children.Select(child => child.FullWidth).Max();
					onWidthChange();
				}
				if (DynamicHeight) {
					height = flexDirection == DirectionType.column ? children.Where(child => child.contributesParentSize()).Select(child => child.FullHeight).Sum() : 
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
			if (props.ContainsKey("class")) {
				// currently no support for dynamic class // TODO: ADD THIS
				object classObj = props["class"];
				switch (classObj) {
					case string className:
						if (CSSHandler.classes.ContainsKey(className)) {
							addCSSUnder(CSSHandler.classes[className]);
						}
						break;
					case IEnumerable<string> classNames: { // classes later in list are given higher precedence
						foreach (var className in classNames.Reverse()) {
							if (className != null && CSSHandler.classes.ContainsKey(className)) {
								addCSSUnder(CSSHandler.classes[className]);
							}
						}
						break;
					}
				}
			}

			if (CSSHandler.tags.ContainsKey(tag)) {
				CSSDefinition tagDefinition = CSSHandler.tags[tag];
				addCSSUnder(tagDefinition);
			}

			if (CSSHandler.tags.ContainsKey("*")) addCSSUnder(CSSHandler.tags["*"]);
			

			void BindDiffFunc<T>(Func<T> get, Action<T> set, Func<T> newFunc, Action onDiff) {
				bindAction(() => {
					T newVal = newFunc();
					if (!Equals(get(), newVal)) {
						set(newVal);
						onDiff();
					}
				});
			}

			bool TryBindDiffFunc<T>(Func<T> get, Action<T> set, object obj, Action onDiff) {
				if (obj is Func<T> newFunc) {
					BindDiffFunc(get, set, newFunc, onDiff);
					return true;
				}

				return false;
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
				
				if (textContent != null) font = Fonts.MonoGameHtmlFonts.getFontSafe(fontFamily, fontSize); // default

				if (props.Keys.Count == 0) {
					if (textContent != null) onFontChange();
					goto finishProps;
				}
				
				if (props.ContainsKey("left")) left = prop<int>("left");
				if (props.ContainsKey("right")) right = prop<int>("right");
				if (props.ContainsKey("top")) top = prop<int>("top");
				if (props.ContainsKey("bottom")) bottom = prop<int>("bottom");

				void DynPosition (string propName, Action<int> set, Func<int?> get) {
					if (props.ContainsKey(propName)) {
						object funcProp = props[propName];
						if (funcProp is Func<int> intFunc) {
							set(intFunc()); // removes jitter on initialization
							bindAction(() => {
								int? init = get();
								set(intFunc());
								if (get() != init) layoutDown(); 
							});
						}
					}
				}

				if (props.ContainsKey("pos")) {
					var (startLeft, startTop) = prop<Vector2>("pos");
					left = (int) startLeft;
					top = (int) startTop;
				}

				if (props.ContainsKey("-pos")) {
					object funcProp = props["-pos"];
					if (funcProp is Func<Vector2> vecFunc) {
						// removes jitter on initialization
						var (startLeft, startTop) = vecFunc();
						left = (int) startLeft;
						top = (int) startTop;
						bindAction(() => {
							var (newLeft, newTop) = vecFunc();
							if ((int) newLeft != left || (int) newTop != top) {
								left = (int) newLeft;
								top = (int) newTop;
								layoutDown();
							} 
						});
					}
				}
				
				DynPosition("-left", val => left = val, () => left);
				DynPosition("-right", val => right = val, () => right);
				DynPosition("-top", val => top = val, () => top);
				DynPosition("-right", val => right = val, () => right);

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

				if (props.ContainsKey("-padding")) { // TODO: percentages
					object funcProp = props["-padding"];
					if (funcProp is Func<int> intFunc) { 
						bindAction(() => {
							int initPadding = paddingLeft;
							int newPadding = intFunc();
							paddingLeft = newPadding;
							paddingRight = newPadding;
							paddingTop = newPadding;
							paddingBottom = newPadding;
							if (initPadding != paddingLeft) triggerOnResize();
						});
					}
				}
				
				if (props.ContainsKey("-margin")) { // TODO: percentages
					object funcProp = props["-margin"];
					if (funcProp is Func<int> intFunc) { 
						bindAction(() => {
							int initMargin = marginLeft;
							int newMargin = intFunc();
							marginLeft = newMargin;
							marginRight = newMargin;
							marginTop = newMargin;
							marginBottom = newMargin;
							if (initMargin != marginLeft) triggerOnResize();
						});
					}
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
				
				tryWidthProp(ref marginLeft, 
					"marginLeft");
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
					TryBindDiffFunc(() => fontSize, val => fontSize = val, props["-fontSize"], onFontChange);
				}
				if (props.ContainsKey("-fontFamily")) {
					TryBindDiffFunc(() => fontFamily, val => fontFamily = val, props["-fontFamily"], onFontChange);
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
								if (width != initWidth || height != initHeight) triggerOnResize();
							});
							break;
						case Func<int> intFunc:
							bindAction(() => {
								int initWidth = width;
								int initHeight = height;
								int val = intFunc();
								width = val;
								height = val;
								if (width != initWidth || height != initHeight) triggerOnResize();
							});
							break;
					}
				}

				if (props.ContainsKey("-flex")) {
					TryBindDiffFunc(() => flex, val => flex = val, props["-flex"], () => {
						if (parent.flexDirection == DirectionType.column) {
							if (!propHasAny("width") && !propHasAny("dimens")) {
								width = NodeUtil.widthFromProp("100%", parent);
							}
						} else if (parent.flexDirection == DirectionType.row) {
							if (!propHasAny("height") && !propHasAny("dimens")) {
								height = NodeUtil.heightFromProp("100%", parent);
							}
						}
						triggerOnResize();
					});
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
					var func = funcProp switch {
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

				Action<HtmlNode> CreateOptionalNodeAction(string propName) {
					return props[propName] switch {
						Action<HtmlNode> nodeAction => nodeAction,
						Action action => (node) => action(),
					};
				}

				void TrySetOptionalNodeAction(string propName, ref Action<HtmlNode> actionRef) {
					if (props.ContainsKey(propName)) {
						actionRef = CreateOptionalNodeAction(propName);
					}
				}

				void TrySet<T>(string propName, ref T valRef) {
					if (props.ContainsKey(propName)) valRef = prop<T>(propName);
				}

				if (props.ContainsKey("renderAdd")) renderAdd = prop<Action<SpriteBatch>>("renderAdd");
				
				TrySetOptionalNodeAction(nameof(onResize), ref onResize);

				TrySet(nameof(onPress), ref onPress);
				TrySet(nameof(onPressRemove), ref onPressRemove);
				TrySet(nameof(onMouseMove), ref onMouseMove);
				TrySet(nameof(onMouseDrag), ref onMouseDrag);
				TrySet(nameof(onMouseEnter), ref onMouseEnter);
				TrySet(nameof(onMouseExit), ref onMouseExit);
				TrySet(nameof(onHover), ref onHover);
				TrySet(nameof(onTick), ref onTick);
				// if (props.ContainsKey("onTick")) {
				// 	bindActionAsInitial(prop<Action>("onTick"));
				// }

				TrySet(nameof(onMouseDown), ref onMouseDown);
				TrySet(nameof(onMouseUp), ref onMouseUp);

				TrySet(nameof(borderWidth), ref borderWidth);
				if (props.ContainsKey("-borderWidth")) { 
					object funcProp = props["-borderWidth"];
					if (funcProp is Func<int> func) { 
						bindAction(() => {
							int initBorderWidth = borderWidth;
							borderWidth = func();
							if (initBorderWidth != borderWidth) triggerOnResize();
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

				if (props.ContainsKey("position")) { 
					positionType = Enum.Parse<PositionType>(prop<string>("position").ToUpper());
				}
				// TODO: dynamic position
				
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
							triggerOnResize();
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
								triggerOnResize();
							}
						});
					}
				}

				
				// REF function
				if (props.ContainsKey("ref")) {
					CreateOptionalNodeAction("ref")(this);
				}
			} finishProps: { }

			initialized = true;
			
			if (children != null) {
				foreach (HtmlNode child in children) {
					child.topDownInit();
				}
			}
		}

		public bool contributesParentSize() {
			return positionType == PositionType.STATIC || positionType == PositionType.RELATIVE;
		}

		public void layoutDown() {

			if (positionType != PositionType.STATIC) {
				applyPosition();
			}

			if (this.children == null) return;

			// TODO: rename variable
			var children = this.children.Where(node => node.contributesParentSize()).ToArray();

			if (children.Length != 0) {
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
			}

			if (this.children != null) {
				foreach (HtmlNode child in this.children) { child?.layoutDown(); }
			}

		}

		public void applyPosition() {
			int storeX = x;
			int storeY = y;

			// x-axis
			// TODO: find anchor element!
			if (left.HasValue && right.HasValue) {
				// TODO:
			} if (left.HasValue) {
				x = left.Value;
			} else if (right.HasValue) {
				if (parent != null) { // TODO: FIXME
					x = (parent.x + parent.FullWidth) - FullWidth - right.Value;
				}
			}
			
			// y-axis
			if (top.HasValue && bottom.HasValue) {
				// TODO:
			} if (top.HasValue) {
				y = top.Value;
			} else if (bottom.HasValue) {
				if (parent != null) { // TODO: FIXME
					y = (parent.y + parent.FullHeight) - FullHeight - bottom.Value;
				}
			}

			if (positionType == PositionType.RELATIVE) {
				x += storeX;
				y += storeY;
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
		
		public void bindActionAsInitial(Action action) {
			if (actionList == null) actionList = new List<Action>();
			
			actionList.Insert(0, action);
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
		
		// ReSharper disable once UnusedMember.Global
		public HtmlNode GetPrevious() {
			if (parent == null) {
				throw new Exception("can not get previous node when no parent exists");
			}

			var childArr = parent.children;
			for (int i = 0; i < childArr.Length; i++) {
				if (childArr[i] == this) {
					return i == 0 ? null : childArr[i - 1];
				}
			}

			throw new Exception("did not find this node in its parent's child array");
		}
		
		// ReSharper disable once UnusedMember.Global
		public HtmlNode GetNext() {
			if (parent == null) {
				throw new Exception("can not get next node when no parent exists");
			}

			var childArr = parent.children;
			for (int i = 0; i < childArr.Length; i++) {
				if (childArr[i] == this) {
					return i == childArr.Length - 1 ? null : childArr[i + 1];
				}
			}

			throw new Exception("did not find this node in its parent's child array");
		}

		public static HashSet<string> KnownPropNames = new HashSet<string> {
			"dimens","width","height","backgroundColor","color","tint","src","objectFit",
			"left","top","bottom","right","position","props","propsUnder",
			"renderAdd","onPress","onPressRemove","onHover","onTick","onMouseEnter","onMouseExit","onMouseMove","onMouseDrag","ref",
			"textContent","class","flex","flexDirection","justifyContent","alignItems","align","alignX","alignY",
			"padding","paddingLeft","paddingRight","paddingTop","paddingBottom","paddingInline","paddingBlock",
			"margin","marginLeft","marginRight","marginTop","marginBottom","marginInline","marginBlock",
			"textAlign","fontSize","fontFamily","borderWidth","borderColor","borderRadius"
		};
	}
}