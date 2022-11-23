using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoGameHtml {
	public class HtmlCatchingRunner : HtmlRunner {
		private readonly HtmlRunner wrappedInstance;

		public HtmlCatchingRunner(HtmlRunner wrappedInstance) {
			this.wrappedInstance = wrappedInstance;
		}

		public override void Update(GameTime gameTime, MouseState mouseState, KeyboardState keyState) {
			try {
				wrappedInstance.Update(gameTime, mouseState, keyState);
			} catch (Exception e) { 
				Logger.Log(e);
			}
		}

		public override void Render(SpriteBatch spriteBatch) {
			try {
				wrappedInstance.Render(spriteBatch);
			} catch (Exception e) { 
				Logger.Log(e);
			}
		}
	}
}