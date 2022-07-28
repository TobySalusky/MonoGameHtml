// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Xna.Framework;

namespace MonoGameHtml.ColorConsole {
    internal static class ConsoleMain {
        
        public static void PrintCS(string code) { // TODO: this doesn't seem right :/
            Task task = AsyncPrintCS(code);
            task.Wait();
        }

        public static async Task<List<Range>> SyntaxHighlightCSharpHtml(string code) {

            AdhocWorkspace workspace = new AdhocWorkspace();
            Solution solution = workspace.CurrentSolution;
            Project project = solution.AddProject("projectName", "assemblyName", LanguageNames.CSharp);
            Document document = project.AddDocument("name.cs", code);
            SourceText text = await document.GetTextAsync();
            
            var classifiedSpans = await Classifier.GetClassifiedSpansAsync(document, TextSpan.FromBounds(0, text.Length));

            var ranges = classifiedSpans.Select(classifiedSpan =>
                new Range(classifiedSpan, text.GetSubText(classifiedSpan.TextSpan).ToString()));

            ranges = FillGaps(text, ranges).ToArray();
            
            var arr = new string[code.Length];

            void SetRange(int start, int len, string classification) {
                for (int j = 0; j < len; j++) {
                    arr[start + j] = classification;
                }
            }

            foreach (var range in ranges) {
                SetRange(range.TextSpan.Start, range.TextSpan.Length, range.ClassificationType);
            }

            // HTML
            
            
            // TODO: convert back to ranges
            var rangeList = new List<Range>();
            /*
            string currClassification = arr.Length > 0 ? arr[0] : null;
            int currLen = 0;

            for (int i = 0; i < arr.Length; i++) {
                string val = arr[i];
                if (val == currClassification) {
                    currLen++;
                }
                if (val != currClassification) {
                    Logger.log("?",val, currClassification);
                    rangeList.Add(new Range(new ClassifiedSpan(currClassification, new TextSpan(i, currLen)), code.Substring(i, currLen)));
                    currClassification = val;
                    currLen = 1;
                }

                /*if (i == arr.Length - 1) {
                    rangeList.Add(new Range(new ClassifiedSpan(currClassification, new TextSpan(i, currLen)), code.Substring(i, currLen)));
                }
            }
            rangeList.Add(new Range(new ClassifiedSpan(currClassification, new TextSpan(code.Length - currLen, currLen)), code.Substring(code.Length - currLen, currLen)));
            
            Logger.log("count",rangeList.Count);*/

            
            var braceDict = DelimPair.genPairDict(code, DelimPair.CurlyBrackets);
            // TODO: DO NOT MAKE DELIM PAIRS WITH QUOTES INSIDE OF STRINGS!
            var singleQuoteDict = DelimPair.genPairDict(code, DelimPair.SingleQuotes);
            var doubleQuoteDict = DelimPair.genPairDict(code, DelimPair.Quotes);
            var parenDict = DelimPair.genPairDict(code, DelimPair.Parens);

            static string GetTagType(string tag) {
                if (tag == "Try" || tag == "Catch" ||
                    tag == "If" || tag == "Elif" || tag == "Else" ||
                    tag == "Switch" || tag == "Case" || tag == "Default") return "HtmlTagControl";
                return "HtmlTag";
            }
            
            for (int i = 0; i < arr.Length; i++) {
                if (code[i] == '<') {
                    
                    HtmlStartInfo startInfo = new HtmlStartInfo(code, i, false, false, braceDict, singleQuoteDict,
                        doubleQuoteDict, parenDict);
                    if (!startInfo.valid) {
                        startInfo = new HtmlStartInfo(code, i, true, false, braceDict, singleQuoteDict,
                            doubleQuoteDict, parenDict);
                    }

                    if (startInfo.valid) {
                        SetRange(i, 1, "HtmlBrackets");
                        SetRange(startInfo.closeIndex, 1, "HtmlBrackets");

                        int tagLen = 0;
                        for (int j = i + 1; j < startInfo.closeIndex; j++) {
                            char c = code[j];
                            if (c.IsAlphanumeric()) {
                                tagLen++;
                                continue;
                            }
                            break;
                        }
                        

                        SetRange(i + 1, tagLen, GetTagType(code.Substring(i+1, tagLen)));

                        int propNameStart = i + 1 + tagLen;

                        for (int j = i + tagLen; j < startInfo.closeIndex; j++) {
                            char c = code[j];

                            if (c == '{') {

                                int equalsAt = code[..j].lastIndexOf("=");

                                string propName = code[propNameStart..equalsAt].Trim();
                                string propClassification = HtmlNode.KnownPropNames.Contains(propName)
                                    ? "KnownHtmlProp"
                                    : "UnknownHtmlProp";
                                SetRange(propNameStart, equalsAt - propNameStart, propClassification);

                                SetRange(j, 1, "HtmlBrace");
                                j = braceDict[j].closeIndex - 1;
                                SetRange(j + 1, 1, "HtmlBrace");
                                propNameStart = j + 2;
                            } else if (c == '\'') {

                                int equalsAt = code[..j].lastIndexOf("=");

                                string propName = code[propNameStart..equalsAt].Trim();
                                string propClassification = HtmlNode.KnownPropNames.Contains(propName)
                                    ? "KnownHtmlProp"
                                    : "UnknownHtmlProp";
                                SetRange(propNameStart, equalsAt - propNameStart, propClassification);
                                j = singleQuoteDict[j].closeIndex;
                                propNameStart = j + 1;
                            }
                        }

                    }
                    
                    if (MonoGameHtmlParser.EndingHtml(code, i, out int endIndex)) {
                        SetRange(i, 2, "HtmlBrackets");
                        SetRange(i + 2, endIndex - i - 2, GetTagType(code.Substring(i + 2, endIndex - i - 2)));
                        SetRange(endIndex, 1, "HtmlBrackets");
                    }
                }
            }



            for (int i = 0; i < arr.Length; i++) {
                rangeList.Add(new Range(new ClassifiedSpan(code[i] == '\n' ? "LINEBREAK" : arr[i], new TextSpan(i, 1)), code.Substring(i, 1)));
            }

            return rangeList;
        }

        

        public static async Task AsyncPrintCS(string code) { // TODO: recolor with SetCursorPosition
            AdhocWorkspace workspace = new AdhocWorkspace();
            Solution solution = workspace.CurrentSolution;
            Project project = solution.AddProject("projectName", "assemblyName", LanguageNames.CSharp);
            Document document = project.AddDocument("name.cs", code);
            if (HtmlMain.loggerSettings.formatColoredCS) document = await Formatter.FormatAsync(document);
            SourceText text = await document.GetTextAsync();

            IEnumerable<ClassifiedSpan> classifiedSpans = await Classifier.GetClassifiedSpansAsync(document, TextSpan.FromBounds(0, text.Length));
            //Console.BackgroundColor = ConsoleColor.Black;

            IEnumerable<Range> ranges = classifiedSpans.Select(classifiedSpan =>
                new Range(classifiedSpan, text.GetSubText(classifiedSpan.TextSpan).ToString()));

            ranges = FillGaps(text, ranges);

            void log(string str) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(str);
                Console.ResetColor();
            }

            Logger.logColor(ConsoleColor.Red, HtmlOutput.OUTPUT_CS);

            foreach (Range range in ranges)
            {
                switch (range.ClassificationType)
                {
                    case "keyword":
                    case "keyword - control":
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        break;
                    case "class name":
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        break;
                    case "number":
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        break;
                    case "string":
                        Console.ForegroundColor = ConsoleColor.DarkMagenta;
                        break;
                    case "operator":
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case "punctuation":
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    default:
                        Console.ResetColor();
                        break;
                }

                Console.Write(range.Text);
                //Console.WriteLine(range.ClassificationType);
            }

            Console.ResetColor();
            
            Logger.logColor(ConsoleColor.Red, HtmlOutput.NEW_OUTPUT_END);
        }

        private static IEnumerable<Range> FillGaps(SourceText text, IEnumerable<Range> ranges)
        {
            const string WhitespaceClassification = null;
            int current = 0;
            Range previous = null;

            foreach (Range range in ranges)
            {
                int start = range.TextSpan.Start;
                if (start > current)
                {
                    yield return new Range(WhitespaceClassification, TextSpan.FromBounds(current, start), text);
                }

                if (previous == null || range.TextSpan != previous.TextSpan)
                {
                    yield return range;
                }

                previous = range;
                current = range.TextSpan.End;
            }

            if (current < text.Length)
            {
                yield return new Range(WhitespaceClassification, TextSpan.FromBounds(current, text.Length), text);
            }
        }
    }
}
