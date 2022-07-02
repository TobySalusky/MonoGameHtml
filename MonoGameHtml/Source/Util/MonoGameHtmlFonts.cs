using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using FontStashSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

            // credit: https://stackoverflow.com/questions/1080442/how-do-i-convert-a-stream-into-a-byte-in-c
            static byte[] ReadAllBytes(Stream inStream) {
                if (inStream is MemoryStream memStream) {
                    return memStream.ToArray();
                }

                using var memoryStream = new MemoryStream();
                inStream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }

            try { 
                using var stream = typeof(HtmlNode).Assembly.GetManifestResourceStream("MonoGameHtml.JetBrainsMono.ttf");

                if (stream == null) { 
                    throw new Exception("EmbeddedResource Stream from MonoGameHtml.Resources.JetBrainsMono.ttf is null!");
                }

                existingFonts["___fallback"].AddFont(ReadAllBytes(stream));
            } catch (Exception e) {
                Console.WriteLine(e);
                Console.WriteLine("The default font failed to load, please make sure to add your own font, using the `fontPath` argument in `HtmlMain.Initialize()`, and name at least one font file '__fallback.ttf' for a valid fallback font.");
            }
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