﻿using System;
using Microsoft.Xna.Framework;

namespace MonoGameHtml {
    internal static class Maths {

        public static readonly float PI = (float) Math.PI;
        public static readonly float halfPI = (float) Math.PI / 2;
        public static readonly float twoPI = (float) Math.PI * 2;


        public static float square(float val) {
            return val * val;
        }

        public static Vector2 squareEach(Vector2 vec) {
            return new Vector2(square(vec.X), square(vec.Y));
        }

        public static float sum(Vector2 vec) {
            return vec.X + vec.Y;
        }

        public static Vector2 signEach(Vector2 vec) {
            return new Vector2(Math.Sign(vec.X), Math.Sign(vec.Y));
        }

        public static Vector2 mags(Vector2 vec) {
            return new Vector2(Math.Abs(vec.X), Math.Abs(vec.Y));
        }

        public static float min(Vector2 vec) {
            return Math.Min(vec.X, vec.Y);
        }
        
        public static float max(Vector2 vec) {
            return Math.Max(vec.X, vec.Y);
        }

        public static Vector2 removeMin(Vector2 vec) { 
            if (vec.X < vec.Y) return new Vector2(0, vec.Y);
            return new Vector2(vec.X, 0);
        }

        public static Vector2 abs(Vector2 vec) { 
            return new Vector2(Math.Abs(vec.X), Math.Abs(vec.Y));
        }
    }
}