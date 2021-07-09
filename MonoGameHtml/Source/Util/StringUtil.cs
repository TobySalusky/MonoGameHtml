﻿﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace MonoGameHtml {
    public static class StringUtil {

        public static int CountOf(this string str, string searchFor) {
            int count = 0;
            for (int i = 0; i < str.Length + 1 - searchFor.Length; i++) {
                if (str.Substring(i, searchFor.Length) == searchFor) count++;
            }

            return count;
        }

        public static string Sub(this string str, int startInclusive, int endExclusive) {
            return str.Substring(startInclusive, endExclusive - startInclusive);
        }
		
        public static string Sub(this string str, int startInclusive) {
            return str.Substring(startInclusive);
        }

        public static int indexOf(this string str, string value) {
            return str.IndexOf(value, StringComparison.Ordinal);
        }
        
        public static int lastIndexOf(this string str, string value) {
            return str.LastIndexOf(value, StringComparison.Ordinal);
        }

        public static string ReplaceFirst(this string str, string oldStr, string newStr) { // NOTE: should crash if no instance exists
            int index = str.IndexOf(oldStr);
            return str.Substring(0, index) + newStr + str.Substring(index + oldStr.Length);
        }

        public static (string, string) splitWithout(this string str, int index) {
            return (str.Substring(0, index), str.Substring(index + 1));
        }

        public static string[] splitWithout(this string str, IEnumerable<int> splitIndices) {
            string[] arr = new string[splitIndices.Count() + 1];

            int i = 0;
            foreach (int index in splitIndices) {

                (string before, string after) = splitWithout(str, index);
                arr[i] = before;
                str = after;
                i++;
            }
            
            arr[^1] = str;
            return arr;
        }

        public static List<int> allIndices(this string str, string search) {
            return Util.allIndices(str, search);
        }
        
        public static DelimPair searchPairs(this string str, string open, string close, int searchIndex) {
            return DelimPair.genPairDict(str, open, close)[searchIndex];
        }
        
        public static DelimPair searchPairs(this string str, (string, string) openAndClose, int searchIndex) {
            return DelimPair.genPairDict(str, openAndClose.Item1, openAndClose.Item2)[searchIndex];
        }

        public static Dictionary<(string, string), int> nestAmountsLen(this string str, int start, int len, params (string, string)[] delimTypes) {
            return nestAmountsRange(str, (start, start + len - 1), delimTypes);
        }
        
        public static Dictionary<(string, string), int> nestAmountsLen(this string str, int start, int len, Dictionary<(string, string), List<DelimPair>> dict) {
            return nestAmountsRange(str, (start, start + len - 1), dict);
        }

        public static Dictionary<(string, string), int> nestAmountsRange(this string str, (int, int) rangeInclusive, params (string, string)[] delimTypes) {
            Dictionary<(string, string), List<DelimPair>> dict = DelimPair.searchAll(str, delimTypes);
            return nestAmountsRange(str, rangeInclusive, dict);
        }

        
        public static Dictionary<(string, string), int> nestAmountsRange(this string str, (int, int) rangeInclusive, Dictionary<(string, string), List<DelimPair>> dict) {
            (int start, int end) = rangeInclusive;
            
            var nestDict = new Dictionary<(string, string), int>();
            foreach (var key in dict.Keys) {
                nestDict[key] = 0;
                foreach (var pair in dict[key]) {
                    if (pair.openIndex + pair.openLen < start && pair.closeIndex > end) nestDict[key]++;
                }
            }
            return nestDict;
        }

        public static string beforePair(this string str, DelimPair pair) {
            return str.Substring(0, pair.openIndex);
        }

        public static string afterPair(this string str, DelimPair pair) {
            return str.Substring(pair.AfterClose);
        }
    }
}