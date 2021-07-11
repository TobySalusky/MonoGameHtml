using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using MonoGameHtml.ColorConsole;
using MonoGameHtml.Exceptions;

namespace MonoGameHtml {
	
	public static class HtmlProcessor {

		private static HtmlProcessorExtras extras;

		private class HtmlProcessorExtras {
			// in each tuple, first string is variable name, second is declaration
			public readonly Dictionary<string, List<PropInfo>> componentProps = new Dictionary<string, List<PropInfo>>();
		}

		private static string StringifyNode(string node) {
			node = node.Trim();
			
			var htmlPairs = DelimPair.genPairs(node, "<", "</");
			var carrotDict = DelimPair.genPairDict(node, "<", ">");

			
			DelimPair mainPair = htmlPairs[^1];
			
			List<string> childNodes = new List<string>();
			List<int> childNodesIndices = new List<int>();
			string mainInnerContents = "";
			int mainStartIndex = 0;
			foreach (DelimPair pair in htmlPairs) {
				if (pair.nestCount == 1) {
					string subNode = PairToNodeStr(node, pair, carrotDict);
					childNodes.Add(subNode);
					childNodesIndices.Add(pair.openIndex);
				} else if (pair.nestCount == 0) {
					mainStartIndex = carrotDict[pair.openIndex].closeIndex + 1;
					mainInnerContents = node.Sub(mainStartIndex, pair.closeIndex);
				}
			}
			childNodesIndices = childNodesIndices.Select(i => i - mainStartIndex).ToList();
			Dictionary<int, string> childNodesIndicesDict = new Dictionary<int, string>();
			for (int i = 0; i < childNodes.Count; i++) {
				childNodesIndicesDict[childNodesIndices[i]] = childNodes[i];
			}

			string headerContent = carrotDict[mainPair.openIndex].contents(node);

			string output;
			
			int firstSpace = headerContent.indexOf(" ");
			string tag = (firstSpace == -1) ? headerContent : headerContent.Substring(0, firstSpace);
			string data = (firstSpace == -1) ? null : headerContent.Substring(firstSpace + 1).Trim();

			char firstTagLetter = tag.ToCharArray()[0];
			bool customComponent = (firstTagLetter >= 'A' && firstTagLetter <= 'Z');
			
			Dictionary<string, string> props = new Dictionary<string, string>();

			processHeader: {
				
				output = customComponent ? $"Create{tag}(" : "newNode(";
				
				output += $"'{tag}', ";
				
				processData: {
					if (data == null) goto finishData;

					var quoteDict = DelimPair.genPairDict(data, "'", "'"); // TODO: quote pairing will need to be more complex
					var bracketDict = DelimPair.genPairDict(data, "{", "}");

					int lastFin = 0;
					string currLabel = "InvalidProp";

					char[] chars = data.ToCharArray();
					for (int i = 0; i < data.Length; i++) {
						if (chars[i] == '=') {
							currLabel = data.Substring(lastFin, i - lastFin).Trim();
						}

						if (chars[i] == '\'') {
							int fin = quoteDict[i].closeIndex;
							string str = data.Substring(i + 1, fin - (i + 1));
							props[currLabel] = $"'{str}'";
							lastFin = fin + 1;
							i = fin;
						}

						if (chars[i] == '{') {
							int fin = bracketDict[i].closeIndex;
							string jsx = data.Substring(i + 1, fin - (i + 1)).Trim();

							const string funcTypeRegex = @"^[a-zA-Z0-9\[\]<>()\.@_]+~?:";
							string withoutSpaces = Util.noSpaces(jsx);
							
							if (Regex.IsMatch(withoutSpaces, funcTypeRegex)) { // dynamic value (auto generate Func)
								int sep = jsx.indexOf(":");
								bool typeless = false;
								if (sep != -1) { 
									string returnType = jsx.Substring(0, sep).Trim();

									if (new Regex("^[a-zA-z<>~]+$").IsMatch(returnType)) { 
										jsx = jsx.Substring(sep + 1).Trim();
										if (returnType.EndsWith("~")) {
											returnType = returnType.Substring(0, returnType.Length - 1);
											jsx = $"({returnType})({jsx})"; // auto-cast
										}

										jsx = $"(Func^^{returnType}^)(() =^ ({jsx}))";
									} else {
										typeless = true;
									}
								} else {
									typeless = true;
								}

								if (typeless) {
									jsx = $"(Func^^object^)(() =^ ({jsx.Trim()}))";
								}
							} else if (Util.noSpaces(jsx).StartsWith("()=^")) { // parameter-less Action
								int sep = jsx.indexOf("=^");
								jsx = jsx.Substring(sep + 2).Trim();
								jsx = $"(Action)(()=^{jsx})";
							} else if (Util.noSpaces(jsx).StartsWith("(")) { // handles possible Actions with parameters
								DelimPair parenPair = jsx.searchPairs(DelimPair.Parens, jsx.indexOf("("));

								if (Util.noSpaces(parenPair.removeFrom(jsx)).StartsWith("=^")) { // (if parens are followed by =>)
									var actionArgs = parenPair.contents(jsx).SplitUnNestedCommas();
									var actionTypes = actionArgs.Select(arg => arg.Substring(0, arg.indexOf(" ")));
									string actionTypeString = "";
									foreach (string type in actionTypes) {
										actionTypeString += (actionTypeString == "") ? type : $",{type}";
									}

									jsx = $"(Action^^{actionTypeString}^)({jsx})";
								}
							}

							props[currLabel] = $"({jsx})";
							lastFin = fin + 1;
							i = fin;
						}
					}

					string propStr = "props: new Dictionary<string, object> {";

					var keys = props.Keys;
					string startWith = "";
					foreach (string key in keys) {
						propStr += $"{startWith}['{key}']={props[key]}";
						startWith = ", ";
					}

					propStr += "}, ";
					output += propStr;

				} finishData: { }
			}

			if (childNodes.Count > 0) {

				bool staticChildren = true;

				var bracketPairs = DelimPair.genPairs(mainInnerContents, "{", "}");

				foreach (DelimPair bracketPair in bracketPairs) {
					var dict = mainInnerContents.nestAmountsRange((bracketPair.openIndex, bracketPair.closeIndex), 
						DelimPair.Carrots, DelimPair.CurlyBrackets, DelimPair.SquareBrackets, DelimPair.Parens, ("<", "</"));
					if (DelimPair.allNestOf(0, dict)) {
						staticChildren = false;
					}
				}

				if (staticChildren) { 
					output += "children: nodeArr(";
					for (int i = 0; i < childNodes.Count; i++) {
						output += StringifyNode(childNodes[i]) + ((i + 1 < childNodes.Count) ? ", " : "");
					}

					output += ")";
				} else { // TODO: don't regenerate non-jsx segments (define them above creation code)

					int elemCount = 0;
					output += "childrenFunc: (Func^^HtmlNode[]^) (() =^ nodeArr(";

					int i = 0;
					while (i < mainInnerContents.Length) {
						var chars = mainInnerContents.ToCharArray();

						char c = chars[i];
						if (c == '<' || c == '{') {
							if (elemCount > 0) output += ", ";

							if (c == '<') {
								string thisChildNode = childNodesIndicesDict[i];
								output += StringifyNode(thisChildNode);
								i += thisChildNode.Length;
							} else {
								DelimPair bracketPair = mainInnerContents.searchPairs("{", "}", i);

								string jsxChild = bracketPair.contents(mainInnerContents);

								for (int j = childNodes.Count - 1; j >= 0; j--) {
									int childIndex = childNodesIndices[j];
									string childNodeStr = childNodesIndicesDict[childIndex];
									if (childIndex > bracketPair.openIndex && childIndex < bracketPair.closeIndex) {

										int childNewIndex = childIndex - (i + 1);
										jsxChild = jsxChild.Substring(0, childNewIndex) + StringifyNode(childNodeStr) 
										                                                + jsxChild.Substring(childNewIndex + childNodeStr.Length);
									}
								}

								output += $"({jsxChild})";

								i = bracketPair.closeIndex + 1;
							}

							elemCount++;
						} else {
							i++;
						}
					}


					output += "))";
				}
				
			} else {
				string text = mainPair.htmlContents(node).Trim();
				
				if (text.Contains("{")) {
					string textExpression = "";
					while (text.Contains("{")) {
						int index = text.indexOf("{");
						DelimPair pair = text.searchPairs("{", "}", index);
						if (textExpression != "") textExpression += "+";
						textExpression += $"'{text.beforePair(pair)}'+({pair.contents(text)})";
						text = text.afterPair(pair);
					}

					textExpression += $"+'{text}'";

					string dynamicText = $"(Func<string>)(()=^ {textExpression})";
					output += $"textContent: {dynamicText}";
				} else { 
					output += $"textContent: '{text}'";
				}
			}
			
			if (customComponent && extras.componentProps.ContainsKey(tag)) {
				foreach (string key in props.Keys) {
					PropInfo? propInfo = null;
					foreach (var thisPropInfo in extras.componentProps[tag]) {
						if (thisPropInfo.varName != key) continue;
						propInfo = thisPropInfo;
						break;
					}
					
					if (propInfo != null) output += $", {propInfo.Value.paramName}: {props[key]}";
				}
			}
			
			return output + ")";
		}

		private static string PairToNodeStr(string str, DelimPair htmlPair, Dictionary<int, DelimPair> carrotDict) {
			return str.Substring(htmlPair.openIndex,
				(carrotDict[htmlPair.closeIndex].closeIndex + 1) - htmlPair.openIndex);
		}

		private static void AddComponentPropNames(string code) { 

			const string before = "const ";
			string tagEtc = code.Substring(code.indexOf(before) + before.Length);
			string tag = tagEtc.Sub(0, tagEtc.minValidIndex(" ", "=")); // TODO: allow newline

			var customPropDefinitions = code.searchPairs(DelimPair.Parens, code.indexOf("(")).contents(code).SplitUnNestedCommas()
				.Select(str => str.Trim());
			foreach (string customPropDefinition in customPropDefinitions) {
				if (customPropDefinition == "") return;
				
				int firstSpace = customPropDefinition.indexOf(" ");
				string type = customPropDefinition.Substring(0, firstSpace);
				
				string afterType = customPropDefinition.Substring(firstSpace + 1).Trim();
				
				// determine whether the variable has a compile-time or runtime default (= for compile, : for runtime)
				int defaultSeparatorIndex = afterType.minValidIndex("=", ":");
				bool compileDefault = false, runtimeDefault = false;
				string defaultValue = "";
				if (defaultSeparatorIndex != -1) {
					defaultValue = afterType.Substring(defaultSeparatorIndex + 1).Trim();
					char defaultSepChar = afterType.ToCharArray()[defaultSeparatorIndex];
					if (defaultSepChar == '=') compileDefault = true;
					else runtimeDefault = true;
				}
				bool noDefault = !runtimeDefault && !compileDefault;
				
				// find name / possible temp-name (for runtime defaulted vars)
				string varName = noDefault ? afterType : afterType.Substring(0, defaultSeparatorIndex).Trim();
				string paramName = (runtimeDefault) ? $"____{varName}" : varName;

				// makes non-defaulted vars nullable
				if (noDefault && !type.EndsWith("?")) type += "?";
				// param-type may differ from inner type (runtime can be nullable in method parameters even if not nullable inside method)
				string paramType = (runtimeDefault && !type.EndsWith("?")) ? $"{type}?" : type;
				string paramDefaultValue = (compileDefault) ? defaultValue : "null";
				
				string declaration = $"{paramType} {paramName} = {paramDefaultValue}";

				// handles runtime defaulting
				string innerCode = (runtimeDefault) ? $"{type} {varName} = {paramName} ?? {defaultValue};\n" : "";
				
				if (!extras.componentProps.ContainsKey(tag)) extras.componentProps[tag] = new List<PropInfo>();
				extras.componentProps[tag].Add(new PropInfo
				{
					varName = varName,
					paramName = paramName,
					declaration = declaration,
					innerCode = innerCode
				});
			}
		}

		private static string DefineComponent(string code) {
			const string before = "const "; // TODO: EXTRACT TO FUNCTION (done twice!)
			string tagEtc = code.Substring(code.indexOf(before) + before.Length).Trim();
			string tag = tagEtc.Sub(0, tagEtc.minValidIndex(" ", "="));

			if (!Regex.IsMatch(tag, "^[A-Z][A-Za-z0-9]*$")) {
				throw new InvalidHtmlComponentException(
					$"The component name \"{tag}\" is invalid. The name must begin with a capital letter and contain only alphanumeric characters.");
			}
			
			// find contents of component definition
			int afterParamsIndex = tagEtc.searchPairs(DelimPair.Parens, tagEtc.indexOf("(")).AfterClose;
			string afterParams = tagEtc[afterParamsIndex..];
			string componentContents = afterParams.searchPairs(
				DelimPair.CurlyBrackets, afterParams.indexOf("{")).contents(afterParams);

			// contents of first parenthesis (define the props you want to grab)
			string extraPropsString = "";
			string innerPropDefaults = "";
			
			if (extras.componentProps.ContainsKey(tag)) {
				foreach (var propInfo in extras.componentProps[tag]) {
					extraPropsString += $", {propInfo.declaration}";
					innerPropDefaults += propInfo.innerCode;
				}
			}

			// find index of main return (this is where html is autogenerated)
			const string returnStr = "return"; 
			var indices = componentContents.allIndices(returnStr);
			int mainReturnIndex;
			try {
				mainReturnIndex = indices.First(i => DelimPair.allNestOf(0, 
					componentContents.nestAmountsLen(i, returnStr.Length, 
						DelimPair.Parens, DelimPair.CurlyBrackets, DelimPair.SquareBrackets,
						DelimPair.Quotes, DelimPair.SingleQuotes, DelimPair.Carrots)));
			} catch (InvalidOperationException) {
				throw new InvalidHtmlComponentException($"Found no main return statement. {(indices.Any() ? "(Must be un-nested)" : "")}");
			}

			string afterReturn = componentContents[(mainReturnIndex + returnStr.Length)..].Trim();
			DelimPair pair = DelimPair.genPairDict(afterReturn, "(", ")")[0];
			string returnContents = pair.contents(afterReturn).Trim();
			returnContents = RemoveOpenClosed(returnContents);
			
			string stateStr = "";
			state: {
				string stateDefinitions = componentContents[..mainReturnIndex];
				string[] lines = stateDefinitions.Split(new [] { '\r', '\n' });
				foreach (string str in lines) {
					string line = str.Trim();
					const string stateOpen = "useState(";
					int stateIndex = line.indexOf(stateOpen);
					
					if (stateIndex != -1) {
						string type = line.Substring(0, line.indexOf(" "));
						string afterType = line.Substring(line.indexOf(" ") + 1);
						string varNameContents = afterType.searchPairs("[", "]", afterType.indexOf("[")).contents(afterType);
						string[] varNames = varNameContents.Split(",").Select(s => s.Trim()).ToArray();
						
						string initValue = DelimPair.searchPairs(line, "(", ")", line.indexOf("(")).contents(line);

						// TODO: use comparison in state action to check if there was a change
						stateStr += $@"
{type} {varNames[0]} = {initValue};
Action<{type}> {varNames[1]} = (___val) => {{
	{varNames[0]} = ___val;
	___node.stateChangeDown();
}};
";
					} else if (line != "") {
						if (line.StartsWith("var")) { // multi-inline var declarations
							var splitOnCommas = line.SplitUnNestedCommas();
							
							if (splitOnCommas.Count > 1) {
								var declarations = splitOnCommas;
								line = "";
								foreach (string declaration in declarations) {
									string varDeclaration = declaration.Trim();
									if (!varDeclaration.StartsWith("var ")) varDeclaration = "var " + varDeclaration;
									if (!varDeclaration.EndsWith(";")) varDeclaration += ";";
									line += varDeclaration + "\n";
								}
							}
						}

						stateStr += $"\n{line}";
					}
				}
			}
			
			var namedArrayElements = new Dictionary<string, string>();
			generateNamedArrayElements: {
				const string namedElRegex = @"}\s*=";


				var match = Regex.Match(stateStr, namedElRegex);
				while (match.Success) {

					DelimPair bracketPair = stateStr.searchPairs("{", "}", match.Index);

					string content = bracketPair.contents(stateStr);
					string[] elementNames = content.Split(",");

					int nameEnd = -1;
					string arrName = "";
					var chars = stateStr.ToCharArray();
					for (int i = bracketPair.openIndex - 1; i >= 0; i--) {
						if (nameEnd == -1) {
							if (chars[i] != ' ') nameEnd = i;
						}
						else if (chars[i] == ' ') {
							arrName = stateStr.Sub(i + 1, nameEnd + 1);
							break;
						}
					}

					for (int i = 0; i < elementNames.Length; i++) {
						namedArrayElements[$"{arrName}.{elementNames[i].Trim()}"] = $"{arrName}[{i}]";
					}

					stateStr = bracketPair.removeFrom(stateStr);
					match = Regex.Match(stateStr, namedElRegex);
				}
			}

			string output = @$"
HtmlNode Create{tag}(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null{extraPropsString}) {{
	{innerPropDefaults}
	HtmlNode ___node = null;
	{stateStr}
	___node = {StringifyNode(returnContents)};
	return ___node;
}}
";
			
			
			
			applyNamedArrayElements: {
				foreach (string key in namedArrayElements.Keys) {
					output = output.Replace(key, namedArrayElements[key]);
				}
			}

			return output;
		}

		private static string ApplyMacros(string str, Dictionary<string, string> macros) { // TODO: allow recursive macros!

			try {
				foreach (string macroID in macros.Keys) {
					if (macroID.Contains("(")) {

						int openInd = macroID.indexOf("(");
						string paramString = macroID.Substring(openInd + 1, macroID.lastIndexOf(")") - openInd - 1);

						var paramNames = paramString.Split(",").Select(str => str.Trim()).ToArray();

						string find = "@" + macroID.Substring(0, openInd) + "(";
						int currIndex = str.indexOf(find);
						while (currIndex != -1) {
							var pair = DelimPair.genPairDict(str, DelimPair.Parens)[currIndex+macroID.Substring(0, openInd).Length+1];

							string content = pair.contents(str);
							var valStrs = content.Split(",").Select(str => str.Trim()).ToArray();
						
							string macroStr = macros[macroID];
							for (int i = 0; i < paramNames.Length; i++) {
								macroStr = macroStr.Replace($"$${paramNames[i]}", valStrs[i]);
							}

							str = str.Substring(0, currIndex) + macroStr + str.Substring(pair.closeIndex + 1);

							currIndex = str.indexOf(find);
						}
							
					} else {
						str = str.Replace($"@{macroID}", macros[macroID]);
					}
				}
			} catch (Exception) {
				throw new MacroException("Invalid macro usage.");
			}
			
			return str;
		}

		private static string RemoveOpenClosed(string code) {
			int endCount = code.CountOf("/>");
			for (int i = 0; i < endCount; i++) {
				int endIndex = code.indexOf("/>");
				DelimPair pair = DelimPair.genPairDict(code, DelimPair.Carrots)[endIndex + 1];
				int startIndex = pair.openIndex;

				string str = pair.whole(code);
				string tag = (str.Contains(" ")) ? str.Sub(1, str.indexOf(" ")) :  str.Sub(1, str.indexOf("/"));

				str = str.Substring(0, str.Length - 2) + $"></{tag}>";

				code = code.Substring(0, startIndex) + str + code.Substring(endIndex + 2);
			}

			if (code.Contains("/>")) throw new InvalidHtmlStructureException("\"/>\" lacked a tag.");
			return code;
		}

		public static async Task<HtmlNode> GenHtml(string code, StatePack pack,
			Dictionary<string, string> macros = null, string[] components = null) {
			
			Parser.CheckCarrotType(code);
			
			macros ??= Macros.create();

			// cache ====
			string inputString = code;
			string[] inputArr = new List<string> {inputString}.Concat(components ?? new string[] { })
				.Concat((macros != null)
					? (macros.Keys.ToArray().Concat(macros.Keys.Select(key => macros[key])))
					: new string[] { }).ToArray();

			if (HtmlSettings.useCache && HtmlCache.IsCached(inputArr, pack)) {
				Logger.log("Using Cached HTML");
				return pack.cachedNode();
			}

			// code generation
			extras = new HtmlProcessorExtras();

			// code replacements
			if (macros != null) code = ApplyMacros(code, macros);
			code = code.Replace("=>", "=^");
			code = RemoveOpenClosed(code);

			// HTML CONSOLE LOG
			Logger.logColor(ConsoleColor.Yellow, HtmlOutput.OUTPUT_HTML);
			Logger.log(code);
			Logger.logColor(ConsoleColor.Yellow, HtmlOutput.OUTPUT_END);


			string preHTML = @"
using System.Linq;
using System.Threading.Tasks;
using MonoGameHtml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
/*IMPORTS_DONE*/
";
			if (components != null) {
				foreach (string component in components) {
					// TODO: extract to method !!!!!
					string componentString = (macros == null) ? component : ApplyMacros(component, macros);
					componentString = componentString.Replace("=>", "=^");
					AddComponentPropNames(componentString);
				}

				foreach (string component in components) {
					string componentString = (macros == null) ? component : ApplyMacros(component, macros);
					componentString = componentString.Replace("=>", "=^");
					preHTML += DefineComponent(componentString);
				}
			}

			code = preHTML + "HtmlNode node = " + StringifyNode(code) + ";";
			code += "\nsetupNode(node);";
			code += "\nreturn node;";


			foreach (string key in pack.___vars.Keys) {
				code = code.Replace($"${key}", $"(({pack.___types[key]})___vars[\"{key}\"])");
			}

			mapToSelect:
			{
				while (code.Contains(".map(")) {
					int index = code.indexOf(".map(");
					DelimPair pair = code.searchPairs("(", ")", index + 4);

					code = code.Substring(0, index) + $".Select({pair.contents(code)}).ToArray()" +
					       code.Substring(pair.closeIndex + 1);
				}
			}
			code = code.Replace("'", "\"");
			code = code.Replace("=^", "=>");
			code = code.Replace("^^", "<");
			code = code.Replace("^", ">");


			inlineArray:
			{

				int minIndex() => code.minValidIndex("arr(", "arr[");

				while (minIndex() != -1) {
					int index = minIndex();

					string type = "";
					int bracketIndex = index + 3;
					if (code.Substring(bracketIndex, 1) == "(") {
						DelimPair pair = code.searchPairs("(", ")", bracketIndex);
						bracketIndex = pair.closeIndex + 1;
						type = pair.contents(code);
					}

					DelimPair contentPair = code.searchPairs("[", "]", bracketIndex);
					string arrContents = contentPair.contents(code);
					code = code.Substring(0, index) + $"(new {type}[]{{{arrContents}}})" +
					       code.Substring(contentPair.closeIndex + 1);
				}
			}

			if (HtmlMain.loggerSettings.colorOutputCS) {
				Task.Run(() => ConsoleMain.AsyncPrintCS(code)); // (runs in background)
			} else {
				Logger.logColor(ConsoleColor.Red, HtmlOutput.OUTPUT_CS);
				Logger.log(code);
				Logger.logColor(ConsoleColor.Red, HtmlOutput.NEW_OUTPUT_END);
			}
			
			object htmlObj = await CSharpScript.EvaluateAsync(code, ScriptOptions.Default.WithImports("System", "System.Collections.Generic").AddReferences(
				typeof(HtmlNode).Assembly
				), pack);
			
			HtmlNode returnNode = (HtmlNode) htmlObj;
			
			// Caching
			if (HtmlSettings.generateCache) { // Only caches when node generation is successful
				string toCache = code.Substring(code.indexOf("/*IMPORTS_DONE*/"));
				HtmlCache.CacheHtml(inputArr, toCache, pack);
			}
			
			return returnNode;
		}

		
		public static async Task<HtmlRunner> GenerateRunner(string code, StatePack pack, Dictionary<string, string> macros = null, string[] components = null) {

			var watch = new System.Diagnostics.Stopwatch();
			watch.Start();
			
			HtmlNode node = await GenHtml(code, pack, macros, components);

			watch.Stop();
			Logger.log($"generating HTML took: {watch.Elapsed.TotalSeconds} seconds");

			
			return new HtmlRunner { node=node, statePack=pack};
		}
	}
	
}