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

		private static string EditJsx(string jsx) {
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

						jsx = $"(Func<{returnType}>)(() => {jsx})";
					} else {
						typeless = true;
					}
				} else {
					typeless = true;
				}

				if (typeless) {
					jsx = $"(Func<object>)(() => {jsx.Trim()})";
				}
			} else if (Util.noSpaces(jsx).StartsWith("()=>")) { // parameter-less Action
				int sep = jsx.indexOf("=>");
				jsx = jsx.Substring(sep + 2).Trim();
				jsx = $"(Action)(()=>{jsx})";
			} else if (Util.noSpaces(jsx).StartsWith("(")) { // handles possible Actions with parameters
				DelimPair parenPair = jsx.searchPairs(DelimPair.Parens, jsx.indexOf("("));

				if (Util.noSpaces(parenPair.removeFrom(jsx)).StartsWith("=>")) { // (if parens are followed by =>)
					var actionArgs = parenPair.contents(jsx).SplitUnNestedCommas();
					var actionTypes = actionArgs.Select(arg => arg.Substring(0, arg.indexOf(" ")));
					string actionTypeString = "";
					foreach (string type in actionTypes) {
						actionTypeString += (actionTypeString == "") ? type : $",{type}";
					}

					jsx = $"(Action<{actionTypeString}>)({jsx})";
				}
			}

			return jsx;
		}

		private static string StringifyNode(string node) {
			node = node.Trim();

			//if (node == "<html></html>") return "null";

			var htmlPairs = Parser.FindHtmlPairs(node, true);
			
			HtmlPair mainPair = htmlPairs[^1];
			
			var childNodes = htmlPairs.Where(pair => pair.nestCount == 1).ToList();
			string mainInnerContents = mainPair.contents(node);
			int mainStartIndex = mainPair.contentStartIndex();

			var childNodesIndicesDict = new Dictionary<int, string>();
			foreach (HtmlPair htmlPair in htmlPairs) {
				childNodesIndicesDict[htmlPair.openIndex - mainStartIndex] = htmlPair.whole(node);
			}
			
			string tag = mainPair.openingTag(node);
			
			char firstTagLetter = tag[0];
			bool customComponent = (firstTagLetter >= 'A' && firstTagLetter <= 'Z');
			
			string output = customComponent ? $"Create{tag}(" : "newNode(";
			output += $"'{tag}'";


			// PROP SECTION =============================================
			var props = mainPair.jsxFrags ?? new Dictionary<string, string>(); // TODO: PLEASE REMOVE THIS
			var keys = props.Keys.ToArray();
			for (int i = 0; i < keys.Length; i++) {
				string key = keys[i];
				props[key] = EditJsx(props[key]);
			}
			
			string propStr = "props: new Dictionary<string, object> {";

			string startWith = "";
			foreach (string key in keys) {
				propStr += $"{startWith}['{key}']={props[key]}";
				startWith = ", ";
			}

			propStr += "}";
			//output += propStr;

			// CHILDREN SECTION ============================================
			string childrenStr = "children: null", childrenFuncStr = "childrenFunc: null", textContentStr = "textContent: null", textContentFuncStr = "textContentFunc: null";
			if (childNodes.Count > 0) {

				bool staticChildren = true;

				var bracketPairs = DelimPair.genPairs(mainInnerContents, "{", "}");
				foreach (DelimPair bracketPair in bracketPairs) {// TODO: remove the last thing!!!
					var dict = mainInnerContents.nestAmountsRange((bracketPair.openIndex, bracketPair.closeIndex), 
						DelimPair.CurlyBrackets, DelimPair.SquareBrackets, DelimPair.Parens);
					if (DelimPair.allNestOf(0, dict) && !childNodes
						                                 .Any(pair => bracketPair.isPartOf(new DelimPair(pair.openIndex - mainStartIndex, 
							                                 pair.closeIndex - mainStartIndex, pair.openLen, pair.closeLen)))) {
						staticChildren = false;
					}
				}

				if (staticChildren) {
					childrenStr = "children: nodeArr(";
					for (int i = 0; i < childNodes.Count; i++) {
						childrenStr += StringifyNode(childNodes[i].whole(node)) + ((i + 1 < childNodes.Count) ? ", " : "");
					}
					childrenStr += ')';
				} else { // TODO: don't regenerate non-jsx segments (define them above creation code)

					int elemCount = 0;
					childrenFuncStr = "childrenFunc: (Func<HtmlNode[]>) (() => nodeArr(";

					int i = 0;
					var chars = mainInnerContents.ToCharArray();
					var bracketDict = DelimPair.genPairDict(mainInnerContents, DelimPair.CurlyBrackets);
					while (i < mainInnerContents.Length) {

						char c = chars[i];
						if (c == '<' || c == '{') {
							if (elemCount > 0) childrenFuncStr += ", ";

							if (c == '<') {
								string thisChildNode = childNodesIndicesDict[i];
								childrenFuncStr += StringifyNode(thisChildNode);
								i += thisChildNode.Length;
							} else {
								DelimPair bracketPair = bracketDict[i];

								string jsxChild = bracketPair.contents(mainInnerContents);

								for (int j = childNodes.Count - 1; j >= 0; j--) {
									int childIndex = childNodes[j].openIndex - mainStartIndex;
									string childNodeStr = childNodesIndicesDict[childIndex];
									if (childIndex > bracketPair.openIndex && childIndex < bracketPair.closeIndex) {

										int childNewIndex = childIndex - (i + 1);
										jsxChild = jsxChild.Substring(0, childNewIndex) + StringifyNode(childNodeStr) 
										                                                + jsxChild.Substring(childNewIndex + childNodeStr.Length);
									}
								}

								childrenFuncStr += $"({jsxChild})";

								i = bracketPair.closeIndex + 1;
							}

							elemCount++;
						} else {
							i++;
						}
					}


					childrenFuncStr += "))";
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

					string dynamicText = $"(Func<string>)(()=> {textExpression})";
					textContentStr = $"textContent: {dynamicText}";
				} else { 
					textContentStr = $"textContent: '{text}'";
				}
				
				// TODO: DO THIS FOR SPECIAL <multi></multi> tags that allow multiline text
				// if (text.Contains("{")) {
				// 	string textExpression = "";
				// 	while (text.Contains("{")) {
				// 		int index = text.indexOf("{");
				// 		DelimPair pair = text.searchPairs("{", "}", index);
				// 		if (textExpression != "") textExpression += "+";
				// 		textExpression += $"@'{text.beforePair(pair).TrimAllLines()}'+({pair.contents(text)})";
				// 		text = text.afterPair(pair);
				// 	}
				//
				// 	textExpression += $"+@'{text.TrimAllLines()}'";
				//
				// 	string dynamicText = $"(Func<string>)(()=> {textExpression})";
				// 	output += $"textContent: {dynamicText}";
				// } else { 
				// 	output += $"textContent: @'{text.TrimAllLines()}'";
				// }
			}
			
			// create output

			if (customComponent) {
				output += $", {propStr}, {childrenStr}, {childrenFuncStr}, {textContentStr}";
			}
			else {
				output += $", {propStr}";
				if (childrenStr != "children: null") output += $", {childrenStr}";
				if (childrenFuncStr != "childrenFunc: null") output += $", {childrenFuncStr}";
				if (textContentStr != "textContent: null") output += $", {textContentStr}";
			}

			// CUSTOM PROPS
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
						DelimPair.Quotes, DelimPair.SingleQuotes)));
			} catch (InvalidOperationException) {
				throw new InvalidHtmlComponentException($"Found no main return statement. {(indices.Any() ? "(Must be un-nested)" : "")}");
			}

			string afterReturn = componentContents[(mainReturnIndex + returnStr.Length)..].Trim();
			DelimPair pair = DelimPair.genPairDict(afterReturn, "(", ")")[0];
			string returnContents = pair.contents(afterReturn).Trim();

			if (returnContents == "") throw new Exception($"{tag}'s main return statement is empty.");
			
			string stateStr = "";
			state: {
				string stateDefinitions = componentContents[..mainReturnIndex];

				string[] statements = stateDefinitions.SplitUnNested(";").Select(statement => statement.Trim().EndsWith(";") ? statement : statement + ';').ToArray();

				foreach (string str in statements) {
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
	___node?.stateChangeDown();
}};
";
					} else if (line != "") {// TODO: IMPORTANT! instead of splitting lines, split un-nested semicolons !!!!!!!!!!!
						if (line.StartsWith("var")) {
							int afterEquals = line.indexOf("=") + 1;
							string initialization = line[afterEquals..].Trim();
							Logger.log("DOING LINE",line, initialization);

							if (initialization.StartsWith("(")) {
								DelimPair parenPair = initialization.searchPairs(DelimPair.Parens, 0);

								bool colon = false, arrow = false;
								int colonIndex = -1, arrowIndex = -1;
								for (int i = parenPair.AfterClose; i < initialization.Length - 1; i++) {
									char c = initialization[i];
									
									if (c.IsWhiteSpace()) continue;

									if (!arrow) {
										if (!colon && c == ':') {
											colon = true;
											colonIndex = i;
											continue;
										}

										if (colon && c.IsValidTypeNameCharacter()) {
											continue;
										}

										if (initialization.Substring(i, 2) == "=>") {
											arrow = true;
											arrowIndex = i;
											i++;
											continue;
										}
									} else {

										string body;
										if (c == '{') {
											DelimPair bracketPair = initialization.searchPairs(DelimPair.CurlyBrackets, i);
											body = $"{{{bracketPair.contents(initialization)}}}";
										} else {
											body = initialization[(arrowIndex + 2)..].Trim();
											while (body.EndsWith(";")) {
												body = body[..^1];
											}
										}

										Logger.log("TEST?", initialization);
										// do stuff

										var decs = parenPair.contents(initialization).SplitUnNestedCommas().
											Select(dec => dec.SplitUnNested(" ").Select(str => str.Trim()).Where(str => str != "").ToArray());
										string varNames = "", varTypes = "";

										foreach (string[] list in decs) {
											if (list.Length != 2) break;
											if (varNames != "") {
												varNames += ", ";
												varTypes += ", ";
											}

											varTypes += list[0];
											varNames += list[1];
										}

										string returnType = colon ? initialization[(colonIndex + 1)..arrowIndex].Trim() : "";

										string fType = "Action";
										if (returnType != "") {
											if (varTypes != "") varTypes += ", ";
											varTypes += returnType;
											fType = "Func";
										}

										string possibleGenerics = varTypes == "" ? "" : $"<{varTypes}>";

										string resultStr = $"({fType}{possibleGenerics})(({varNames})=>{body});";

										initialization = initialization[..parenPair.openIndex] + resultStr;
										//Logger.log("TEST", initialization);
										line = $"{line[..afterEquals]} {initialization}";
										
									}
									break;
								}
								
							}




							// multi-inline var declarations
							// Multiple 'var'-s per line (I have mixed feelings)
							/*var splitOnCommas = line.SplitUnNestedCommas();
							
							if (splitOnCommas.Count > 1) {
								var declarations = splitOnCommas;
								line = "";
								foreach (string declaration in declarations) {
									string varDeclaration = declaration.Trim();
									if (!varDeclaration.StartsWith("var ")) varDeclaration = "var " + varDeclaration;
									if (!varDeclaration.EndsWith(";")) varDeclaration += ";";
									line += varDeclaration + "\n";
								}
							}*/
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
HtmlNode Create{tag}(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, Func<HtmlNode[]> childrenFunc = null{extraPropsString}) {{
	{innerPropDefaults}
	HtmlNode ___node = null;
	{stateStr}
	___node = {returnContents};
	return ___node;
}}
";
			var basePairs = Parser.FindHtmlPairs(output).Where(htmlPair => htmlPair.nestCount == 0).Reverse();

			foreach (HtmlPair htmlPair in basePairs) {
				output = output[..htmlPair.openIndex] + StringifyNode(htmlPair.whole(output)) + output[(htmlPair.closeIndex + htmlPair.closeLen)..];
			}
			
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

		internal static string preprocess(string code, Dictionary<string, string> macros) { // applies macros and expands tags
			code = ApplyMacros(code, macros);
			code = Parser.ExpandSelfClosedHtmlTags(code);
			code = code.Replace("\"", "'");
			code = InlineActionFuncCasts(code);
			return code;
		}

		private static string InlineActionFuncCasts(string code) {
			// TODO:
			return code;
		}

		public static async Task<HtmlNode> GenHtml(string code, StatePack pack,
			Dictionary<string, string> macros = null, string[] components = null, HtmlIntermediateUser intermediateUser = null) {
			
			//Logger.log("fixed\n",Parser.ExpandSelfClosedHtmlTags(code));
			//Parser.CheckCarrotType(code);
			
			macros ??= Macros.create();
			components ??= HtmlComponents.Create();
			
			// preprocess
			code = preprocess(code, macros);
			components = components.Select(componentCode => preprocess(componentCode, macros)).ToArray();

			// cache ====
			string[] inputArr = new List<string> {code}.Concat(components).ToArray();

			if (HtmlSettings.useCache && HtmlCache.IsCached(inputArr, pack)) {
				Logger.log("Using Cached HTML");
				return pack.cachedNode();
			}

			// code generation
			extras = new HtmlProcessorExtras();

			// code replacements
			code = code.Replace("=>", "=>");

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
using System.IO;

/*IMPORTS_DONE*/
";
			if (components.Length != 0) {
				foreach (string component in components) {
					AddComponentPropNames(component);
				}

				foreach (string component in components) {
					preHTML += DefineComponent(component);
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

			intermediateUser?.useCS?.Invoke(code);

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