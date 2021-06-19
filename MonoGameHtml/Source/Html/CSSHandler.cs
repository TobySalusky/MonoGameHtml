﻿﻿using System.Collections.Generic;
  using System.IO;
  using System.Linq;

namespace MonoGameHtml {
	public static class CSSHandler {

		public static Dictionary<string, CSSDefinition> classes = new Dictionary<string, CSSDefinition>();
		public static Dictionary<string, CSSDefinition> tags = new Dictionary<string, CSSDefinition>();

		internal static string defaultCssPath;
		
		static CSSHandler() {
			defaultCssPath = FileUtil.TraceFilePath();
			defaultCssPath = defaultCssPath.Substring(0, defaultCssPath.indexOf("Source"));
			defaultCssPath = Path.Join(defaultCssPath, "Assets", "CSS", "Default.css");
			SetCSS();
		}

		public static void SetCSS(params string[] filePaths) {
			SetCSS((IEnumerable<string>)filePaths);
		}

		public static void SetCSS(IEnumerable<string> filePathEnumerable) {

			Dictionary<string, string> data = new Dictionary<string, string>();

			classes = new Dictionary<string, CSSDefinition>();
			tags = new Dictionary<string, CSSDefinition>();

			
			var allFilePaths = new[]{defaultCssPath}.Concat(filePathEnumerable);
			
			foreach (string filePath in allFilePaths) {
				readInCSSFile(filePath, data);
			}

			// use strings to generate definitions
			foreach (string identifier in data.Keys) {
				if (identifier.StartsWith(".")) { // classes
					string realID = identifier.Substring(1);
					classes[realID] = new CSSDefinition(data[identifier]);
				} else { // tags
					tags[identifier] = new CSSDefinition(data[identifier]);
				}
			}
		}

		internal static void readInCSSFile(string filePath, Dictionary<string, string> aggregateCSS) { 
			// read lines from CSS file
			string[] lines = File.ReadAllLines(filePath);
				
			// condense into one line (remove tabs and empty lines)
			string fileContents = "";
			foreach (string line in lines.Select(str => str.Trim())) {
				if (line != "") fileContents += line;
			}
			
			// only continue if there is at least one valid CSS definitions
			if (!fileContents.Contains("{")) return;

			// find bracket pairs and extract CSS code + class/tag identifiers
			var bracketPairs = DelimPair.genPairs(fileContents, DelimPair.CurlyBrackets);

			for (int i = 0; i < bracketPairs.Count; i++) {
				string identifiersStr = fileContents.sub((i == 0) ? 0 : bracketPairs[i - 1].AfterClose, bracketPairs[i].openIndex);
				string css = bracketPairs[i].contents(fileContents).Trim();

				var identifiers = identifiersStr.Split(",").Select(str => str.Trim());

				// combines css if already defined, otherwise, creates it
				foreach (string identifier in identifiers) {
					if (aggregateCSS.ContainsKey(identifier)) {
						aggregateCSS[identifier] += css;
					} else {
						aggregateCSS[identifier] = css;
					}
				}
			}
		}
	}
	
	public class CSSDefinition {
		public Dictionary<string, object> styleProps = new Dictionary<string, object>();

		public CSSDefinition(string css) {
			var statements = css.Split(";").Where(str => str.Contains(":"));
			foreach (string statement in statements) {
				int colon = statement.indexOf(":");
				string camelID = camelCase(statement.Substring(0, colon).Trim());
				object value = parseValue(statement.Substring(colon + 1).Trim());
				styleProps[camelID] = value;
			}
		}

		internal string camelCase(string identifier) {
			static string upperFirstChar(string str) {
				return str.Substring(0, 1).ToUpper() + str.Substring(1).ToLower();
			}

			// lowerCases the first segment while upperCasing the rest
			var segments = identifier.Split("-").
				Select((segment, i) => (i == 0) ? segment.ToLower() : upperFirstChar(segment));
			return string.Join("", segments);
		}

		internal object parseValue(string value) {
			
			if (int.TryParse(value, out int intVal)) {
				return intVal;
			}
			if (value.EndsWith("px")) { // processes pixel values ending with px as integers
				bool isInt = int.TryParse(value.Substring(0, value.indexOf("px")), out int pixelValue);
				if (isInt) return pixelValue;
			}

			if (value.Contains("-")) return camelCase(value);

			return value;
		}
	}
}