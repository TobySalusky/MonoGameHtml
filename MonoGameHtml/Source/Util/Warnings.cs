﻿using System;

namespace MonoGameHtml {
    internal static class Warnings {
        
        public static void log(object str) {
            Console.WriteLine(str);
        }

        public static void log(params object[] strs) {
            Console.Write(strs[0]);

            for (int i = 1; i < strs.Length; i++) {
                Console.Write(" " + strs[i]);
            }

            Console.Write("\n");
        }
    }
}