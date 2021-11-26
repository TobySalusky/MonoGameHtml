using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using FontStashSharp;

namespace MonoGameHtml.Fonts {
    public static class MonoGameHtmlFonts {

        public static Dictionary<string, FontSystem> existingFonts;

        static MonoGameHtmlFonts() {
            Initialize();
        }

        public static void Initialize() {
            if (existingFonts != null) {
                foreach (var pair in existingFonts) {
                    pair.Value.Dispose();
                }
            }

            existingFonts = new Dictionary<string, FontSystem> {["___fallback"] = new FontSystem()};

            existingFonts["___fallback"].AddFont(File.ReadAllBytes(HtmlMain.defaultFontPath));
        }

        public static void Reset() {
            Initialize();
        }

        public static SpriteFontBase getFontSafe(string fontName, int size) {
            size = Math.Max(size, 1);
            if (existingFonts.ContainsKey(fontName)) {
                return existingFonts[fontName].GetFont(size);
            }

            try {
                Console.WriteLine($"EFEFEFEFE {fontName}");

                existingFonts[fontName] = new FontSystem();
                existingFonts[fontName].AddFont(File.ReadAllBytes(Path.Join(HtmlMain.fontPath, $"{fontName}.ttf")));
                
                return existingFonts[fontName].GetFont(size);
            }
            catch (Exception e) {
                Logger.log(e, e.StackTrace, e.Message);
                Logger.log($"Failed to load font {fontName} size {size}");
                return getFontSafe("___fallback", size);
            }
        }
        

    }
}