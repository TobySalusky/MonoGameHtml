using System;
using System.Collections.Generic;
using System.IO;

namespace MonoGameHtml {
    public class HtmlPair : DelimPair {
        
        public Dictionary<string, string> jsxFrags;

        public HtmlPair(int openIndex, int closeIndex, int openLen = 1, int closeLen = 1, int nestCount = 0) : base(openIndex, closeIndex, openLen, closeLen, nestCount) {}

        public string headerContents(string str) {
            return str.Substring(openIndex + 1, openLen - 2).Trim();
        }

        public string openingTag(string str) {
            string header = headerContents(str) + '>';
            for (int i = openIndex + 1; i < header.Length; i++) {
                if (header[i] == '>' || header[i].IsWhiteSpace()) return header[..i];
            }
            return "";
        }

        public string closingTag(string str) {
            return str[(closeIndex + 2)..(closeIndex + closeLen - 1)];
        }

        public string propString(string str) {
            string header = headerContents(str);
            for (int i = openIndex + 1; i < header.Length; i++) {
                if (header[i].IsWhiteSpace()) return header[i..].Trim();
            }
            return "";
        }
    }
}