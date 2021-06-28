using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoGameHtml {
	public class HtmlRunner {
		
		internal HtmlNode node;
		internal StatePack statePack; // TODO: make StatePack not fully static

		internal MouseState lastMouseState;
		internal Vector2 lastMousePos;

		private float delta(GameTime gameTime) {
			return (float) gameTime.ElapsedGameTime.TotalSeconds;
		}
		
		public void Update(GameTime gameTime, MouseState mouseState) { 
			MouseInfo mouse = new MouseInfo(mouseState, lastMouseState);
			float deltaTime = delta(gameTime);

			statePack.timePassed += deltaTime;
			statePack.deltaTime = deltaTime;
			StatePack.mousePos = mouse.pos;
			
			node.update(deltaTime, mouse);
			if (mouse.leftPressed) {
				node.clickRecurse(mouse.pos);
			}
			if (mouse.leftUnpressed) node.recurse(htmlNode => {
				htmlNode.mouseUp();
			});
			if (mouse.pos != lastMousePos) {
				// moving
				node.recurse(htmlNode => { htmlNode.onMouseMove?.Invoke(); });
				// dragging
				if (mouse.leftDown) node.recurse(htmlNode => { htmlNode.onMouseDrag?.Invoke(); });
			}

			lastMouseState = mouseState;
			lastMousePos = mouse.pos;
		}

		public void Render(SpriteBatch spriteBatch) { 
			
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