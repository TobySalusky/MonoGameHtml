﻿﻿using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using StbImageSharp;

  namespace MonoGameHtml {
	public static class CSSHandler {

		public static Dictionary<string, CSSDefinition> classes = new Dictionary<string, CSSDefinition>();
		public static Dictionary<string, CSSDefinition> tags = new Dictionary<string, CSSDefinition>();
		
		static CSSHandler() {
			SetCSS();
		}

		public static void SetCSSFiles(params string[] filePaths) { 
			SetCSS(filePaths.Select(path => (CSSResource) new CSSFile(path)).ToArray());
		}

		public static void SetCSS(params CSSResource[] resources) {

			Dictionary<string, string> data = new Dictionary<string, string>();

			classes = new Dictionary<string, CSSDefinition>();
			tags = new Dictionary<string, CSSDefinition>();

			var allResources = new[]{new CSSString(BakedDefaultAssets.DEFAULT_CSS)}.Concat(resources);
			
			foreach (CSSResource resource in allResources) {
				readInCSSFile(resource, data);
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

		internal static void readInCSSFile(CSSResource resource, Dictionary<string, string> aggregateCSS) { 
			// read lines from CSS file
			string[] lines = resource.AsLines();
				
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
				string identifiersStr = fileContents.Sub((i == 0) ? 0 : bracketPairs[i - 1].AfterClose, bracketPairs[i].openIndex);
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

		private CSSDefinition() { }

		public CSSDefinition(string css) {
			var statements = css.Split(";").Where(str => str.Contains(":"));
			foreach (string statement in statements) {
				int colon = statement.indexOf(":");
				string camelID = camelCase(statement.Substring(0, colon).Trim());
				object value = parseValue(statement.Substring(colon + 1).Trim());
				styleProps[camelID] = value;
			}
		}

		protected static (string, CSSDefinition) Combine(params (string, CSSDefinition)[] namedDefinitions) {
			CSSDefinition newDef = new CSSDefinition();
			
			foreach (var (name, def) in namedDefinitions) {
				foreach (var (key, value) in def.styleProps) {
					newDef.styleProps[key] = value;
				}
			}
			return (namedDefinitions.Select(namedDef => namedDef.Item1).Aggregate((a, b) => a + "&&" + b), newDef);
		}

		private string camelCase(string identifier) {
			static string upperFirstChar(string str) {
				return str.Substring(0, 1).ToUpper() + str.Substring(1).ToLower();
			}

			// lowerCases the first segment while upperCasing the rest
			var segments = identifier.Split("-").
				Select((segment, i) => (i == 0) ? segment.ToLower() : upperFirstChar(segment));
			return string.Join("", segments);
		}

		private object parseValue(string value) {
			
			if (int.TryParse(value, out int intVal)) {
				return intVal;
			}

			if (bool.TryParse(value, out bool boolVal)) {
				return boolVal;
			}

			if (value.EndsWith("px")) { // processes pixel values ending with px as integers
				bool isInt = int.TryParse(value.Substring(0, value.indexOf("px")), out int pixelValue);
				if (isInt) return pixelValue;
			}

			if (value.Contains("-")) return camelCase(value);

			return value;
		}
	}

	public interface CSSResource {
		string[] AsLines();
	}

	public class CSSFile : CSSResource {
		private readonly string _filePath;
		public CSSFile(string filePath) {
			_filePath = filePath;
		}

		public string[] AsLines() {
			return File.ReadAllLines(_filePath);
		}
	}

	public class CSSString : CSSResource {
		private readonly string _css;
		public CSSString(string css) {
			_css = css;
		}

		public string[] AsLines() {
			return _css.SplitLines();
		}
	}
  }