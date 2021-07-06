﻿using System;

namespace MonoGameHtml {
    internal static class Warnings {
        
        public static void log(object str) {
            ConsoleColor storeColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("WARNING: ");
            Console.ForegroundColor = storeColor;
            
            Console.WriteLine(str);
        }

        public static void log(params object[] strs) {
            ConsoleColor storeColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("WARNING: ");
            Console.ForegroundColor = storeColor;

            Console.Write(strs[0]);

            for (int i = 1; i < strs.Length; i++) {
                Console.Write(" " + strs[i]);
            }

            Console.Write("\n");
        }
    }
}