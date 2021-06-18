﻿using System.Collections.Generic;
 using System.IO;
 using Microsoft.Xna.Framework;
 using Microsoft.Xna.Framework.Graphics;
 using Color = Microsoft.Xna.Framework.Color;

 namespace MonoGameHtml {
     internal class Textures {

         public static Texture2D rect, invis, circle;

         public static void loadTextures() {

             rect = genRect(Color.White);
             invis = genRect(new Color(1F, 1F, 1F, 0F));
             circle = genCircle(500); // TODO: optimise, circle is slow and also kind of small/big
         }


         public static Texture2D genCircle(int size) {
             Texture2D texture = new Texture2D(HtmlMain.graphicsDevice, size, size);

             var arr = new Color[size * size];
             for (int i = 0; i < size; i++) {
                 for (int j = 0; j < size; j++) {
                     if (Util.mag(new Vector2(size / 2F - i, size / 2F - j)) <= size / 2F) {
                         arr[i + j * size] = Color.White;
                     }
                 }
             }

             texture.SetData(arr);

             return texture;
         }

         public static Texture2D genRect(Color rectColor) {
             Texture2D rect = new Texture2D(HtmlMain.graphicsDevice, 1, 1);
             rect.SetData(new[] {rectColor});
             return rect;
         }

         public static Texture2D genRect(Color rectColor, int x, int y) {
             Texture2D rect = new Texture2D(HtmlMain.graphicsDevice, x, y);

             var arr = new Color[x * y];

             for (int r = 0; r < x; r++) {
                 for (int c = 0; c < y; c++) {
                     arr[r + c * x] = rectColor;
                 }
             }

             rect.SetData(arr);
             return rect;
         }
     }
 }