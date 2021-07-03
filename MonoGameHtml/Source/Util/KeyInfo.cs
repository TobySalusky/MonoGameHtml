﻿﻿using Microsoft.Xna.Framework.Input;

namespace MonoGameHtml {
    public class KeyInfo {

        public KeyboardState newState, oldState;

        public bool shift, control;
        
        public KeyInfo(KeyboardState newState, KeyboardState oldState) {
            this.newState = newState;
            this.oldState = oldState;
            shift = down(Keys.LeftShift); // TODO: right also?
            control = down(Keys.LeftControl);
        }

        public bool down(Keys key) {
            return newState.IsKeyDown(key);
        }
        
        public bool up(Keys key) {
            return newState.IsKeyUp(key);
        }

        public bool pressed(Keys key) {
            return newState.IsKeyDown(key) && !oldState.IsKeyDown(key);
        }
        
        public bool unpressed(Keys key) {
            return !newState.IsKeyDown(key) && oldState.IsKeyDown(key);
        }
    }
}