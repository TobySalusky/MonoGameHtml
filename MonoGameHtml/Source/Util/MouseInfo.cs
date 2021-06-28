﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MonoGameHtml {
    public struct MouseInfo {
        
        public bool leftDown, middleDown, rightDown;
        public bool leftPressed, middlePressed, rightPressed;
        public bool leftUnpressed, middleUnpressed, rightUnpressed;
        public Vector2 pos;
        public int scroll;
        
        public MouseInfo(MouseState state, MouseState lastState) {
            leftDown = state.LeftButton == ButtonState.Pressed;
            middleDown = state.MiddleButton == ButtonState.Pressed;
            rightDown = state.RightButton == ButtonState.Pressed;

            leftPressed = leftDown && lastState.LeftButton != ButtonState.Pressed;
            middlePressed = middleDown && lastState.MiddleButton != ButtonState.Pressed;
            rightPressed = rightDown && lastState.RightButton != ButtonState.Pressed;
            
            leftUnpressed = !leftDown && lastState.LeftButton == ButtonState.Pressed;
            middleUnpressed = !middleDown && lastState.MiddleButton == ButtonState.Pressed;
            rightUnpressed = !rightDown && lastState.RightButton == ButtonState.Pressed;

            scroll = -Math.Sign(state.ScrollWheelValue - lastState.ScrollWheelValue);

            pos = new Vector2(state.X, state.Y);
        }
    }
}