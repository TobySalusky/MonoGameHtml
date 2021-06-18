﻿﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;

namespace MonoGameHtml {
	public static class NodeUtil {

		public static readonly Dictionary<string, Color> colorDict = genColorDict();

		public static Dictionary<string, Color> genColorDict() {
			
			Dictionary<string, Color> dict = new Dictionary<string, Color>();
			Type colorType = typeof(Color);

			PropertyInfo[] fields = colorType.GetProperties(BindingFlags.Public | BindingFlags.Static);

			foreach (var field in fields) {
				if (field.PropertyType == colorType) {
					dict[field.Name.ToLower()] = (Color) field.GetValue(null);
				}
			}

			return dict;
		}

		public static Color strToColor(string str) {

			string lower = str.ToLower();
			if (colorDict.ContainsKey(lower)) return colorDict[lower];

			if (str.StartsWith("#")) {
				byte r = Convert.ToByte(str.Substring(1, 2), 16);
				byte g = Convert.ToByte(str.Substring(3, 2), 16);
				byte b = Convert.ToByte(str.Substring(5, 2), 16);
				
				return new Color(r,g,b);
			}
			
			return Color.Red;
		}


		public static int widthFromProp(object prop, HtmlNode parent) {

			if (prop is string str) {
				if (str.Substring(str.Length - 1) == "%") {
					int maxWidth = parent?.width ?? HtmlMain.screenWidth;

					return (int) (float.Parse(str.Substring(0, str.Length - 1))/100F * maxWidth);
				}
				
				return int.Parse(str);
			}

			return (int) prop;
		}

		public static int heightFromProp(object prop, HtmlNode parent) {

			if (prop is string str) {
				if (str.Substring(str.Length - 1) == "%") {
					int maxHeight = parent?.height ?? HtmlMain.screenHeight;

					return (int) (float.Parse(str.Substring(0, str.Length - 1))/100F * maxHeight);
				}

				return int.Parse(str);
			}

			return (int) prop;
		}
		
		public static Color colorFromProp(object prop) {

			if (prop is string str) {
				return strToColor(str);
			}

			return (Color) prop;
		}

		public static float percentAsFloat(string percent) {
			return float.Parse(percent.Substring(0, percent.Length - 1)) / 100F;
		}
	}
}