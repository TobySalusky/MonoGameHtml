using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoGameHtml {
	public class HtmlRunner {
		
		internal HtmlNode node;
		internal StatePack statePack; // TODO: make StatePack not fully static

		internal MouseState lastMouseState;

		private float delta(GameTime gameTime) {
			return (float) gameTime.ElapsedGameTime.TotalSeconds;
		}
		
		public void Update(GameTime gameTime, MouseState mouseState) { 
			MouseInfo mouse = new MouseInfo(mouseState, lastMouseState);
			float deltaTime = delta(gameTime);

			StatePack.timePassed += deltaTime;
			StatePack.deltaTime = deltaTime;
			
			node.update(deltaTime, mouse);
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
	}
}