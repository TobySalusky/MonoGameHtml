// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;

namespace MonoGameHtml.ColorConsole {
    internal static class ConsoleMain {
        
        public static void PrintCS(string code) { // TODO: this doesn't seem right :/
            Task task = AsyncPrintCS(code);
            task.Wait();
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
