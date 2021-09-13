using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoGameHtml {
	public class HtmlRunner {
		
		internal HtmlNode node;
		internal StatePack statePack; // TODO: make StatePack not fully static

		internal MouseState lastMouseState;
		internal KeyboardState lastKeyState;
		internal Vector2 lastMousePos;

		private float delta(GameTime gameTime) {
			return (float) gameTime.ElapsedGameTime.TotalSeconds;
		}

		public T GetVar<T>(string varName) {
			return (T) statePack.___vars[varName];
		}

		public void UpdateVar(string varName, object value) {
			statePack.___vars[varName] = value;
		}

		public virtual void Update(GameTime gameTime, MouseState mouseState, KeyboardState keyState) {
			MouseInfo mouse = new MouseInfo(mouseState, lastMouseState);
			KeyInfo keys = new KeyInfo(keyState, lastKeyState);
			float deltaTime = delta(gameTime);

			statePack.timePassed += deltaTime;
			statePack.deltaTime = deltaTime;
			StatePack.mousePos = mouse.pos;
			StatePack.keys = keys;
			
			node.update(deltaTime, mouse);
			if (mouse.leftPressed) {
				node.clickRecurse(mouse.pos);
			}
			if (mouse.leftUnpressed) node.recurse(htmlNode => { htmlNode.mouseUp(); });
			
			if (mouse.pos != lastMousePos) {
				// moving
				node.recurse(htmlNode => { htmlNode.onMouseMove?.Invoke(); });
				// dragging
				if (mouse.leftDown) node.recurse(htmlNode => { htmlNode.onMouseDrag?.Invoke(); });
			}
			
			if (mouse.leftPressed) node.recurse(htmlNode => { htmlNode.onMouseDown?.Invoke(); });
			if (mouse.leftUnpressed) node.recurse(htmlNode => { htmlNode.onMouseUp?.Invoke(); });


			lastKeyState = keyState;
			lastMouseState = mouseState;
			lastMousePos = mouse.pos;
		}

		public virtual void Render(SpriteBatch spriteBatch) { 
			
			spriteBatch.Begin(SpriteSortMode.Deferred,
				BlendState.NonPremultiplied,
				SamplerState.PointClamp);
			
			node.render(spriteBatch);
			
			spriteBatch.End();
		}

		public HtmlNode GetNode() {
			return node;
		}

		public StatePack GetStatePack() {
			return statePack;
		}
	}
}