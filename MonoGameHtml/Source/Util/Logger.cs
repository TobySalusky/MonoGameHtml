﻿using System;

namespace MonoGameHtml {
    public static class Logger {
        
        public static void log(object str) {
            if (!HtmlMain.logOutput) return;

            Console.WriteLine(str);
        }

        public static void log(params object[] strs) {
            if (!HtmlMain.logOutput) return;
            
            Console.Write(strs[0]);

            for (int i = 1; i < strs.Length; i++) {
                Console.Write(" " + strs[i]);
            }

            Console.Write("\n");
        }
    }
}