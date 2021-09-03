using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpriteFontPlus;

namespace MonoGameHtml {
    internal static class Fonts {

        public static Dictionary<string, Dictionary<int, SpriteFont>> fontDict =
            new Dictionary<string, Dictionary<int, SpriteFont>>();

        public static SpriteFont getFontSafe(string fontName, int size) {
            size = Math.Max(size, 1);
            if (fontDict.ContainsKey(fontName) && fontDict[fontName].ContainsKey(size)) {
                return fontDict[fontName][size];
            }

            try {
                addFont(fontName, size);
                return fontDict[fontName][size];
            }
            catch (Exception e) {
                Logger.log(e, e.StackTrace, e.Message);
                Logger.log($"Failed to load font {fontName} size {size}");
                return getFontSafe("___fallback", size);
            }
        }
        
        // public static SpriteFont getFontRaw(string fontName, int size) {
        //     return fontDict[fontName][size];
        // }

        public static void addFont(string fontName, int size) {

            if (fontDict.ContainsKey(fontName) && fontDict[fontName].ContainsKey(size)) return;
            string fullPath = fontName == "___fallback" ? HtmlMain.defaultFontPath : Path.Join(HtmlMain.fontPath, $"{fontName}.ttf");
            var fontBakeResult = TtfFontBaker.Bake(File.ReadAllBytes(fullPath),
                size,
                1024,
                1024,
                new[]
                {
                    CharacterRange.BasicLatin,
                    CharacterRange.Latin1Supplement,
                    CharacterRange.LatinExtendedA,
                }
            );

            SpriteFont font = fontBakeResult.CreateSpriteFont(HtmlMain.graphicsDevice);

            if (!fontDict.ContainsKey(fontName)) { 
                fontDict[fontName] = new Dictionary<int, SpriteFont>();
            }

            fontDict[fontName][size] = font;
        }
    }
}