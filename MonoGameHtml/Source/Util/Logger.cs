﻿using System;

namespace MonoGameHtml {
    public static class Logger {

        public static void logColor(ConsoleColor color, params object[] strs) {
            ConsoleColor storeColor = Console.ForegroundColor;
            if (HtmlMain.loggerSettings.allowColor) Console.ForegroundColor = color;
            Log(strs);
            Console.ForegroundColor = storeColor;
        }

        public static void Log(object str) {
            // if (!HtmlMain.loggerSettings.logOutput) return;

            Console.WriteLine(str);
        }

        public static void Log(params object[] strs) {
            if (!HtmlMain.loggerSettings.logOutput) return;
            
            Console.Write(strs[0]);

            for (int i = 1; i < strs.Length; i++) {
                Console.Write(" " + strs[i]);
            }

            Console.Write("\n");
        }
    }
}