using System;
using System.Collections.Generic;
using System.Linq;

namespace MonoGameHtml {
    public class HtmlStartInfo {
        public bool valid;
        public int startIndex, closeIndex;
        public Dictionary<string, string> jsxFrags;

        public HtmlStartInfo(string code, int startIndex, bool selfClosing = false, bool extractData = false,
	        
	        IReadOnlyDictionary<int, DelimPair> braceDict = null, IReadOnlyDictionary<int, DelimPair> singleQuoteDict = null, 
            IReadOnlyDictionary<int, DelimPair> doubleQuoteDict = null, IReadOnlyDictionary<int, DelimPair> parenDict = null) {
	        
	        this.startIndex = startIndex;
	        if (extractData) jsxFrags = new Dictionary<string, string>();

	        braceDict ??= DelimPair.genPairDict(code, DelimPair.CurlyBrackets);
	        singleQuoteDict ??= DelimPair.genPairDict(code, DelimPair.SingleQuotes);
	        doubleQuoteDict ??= DelimPair.genPairDict(code, DelimPair.Quotes);
	        parenDict ??= DelimPair.genPairDict(code, DelimPair.Parens);
	        
            // TODO: when bold/annotations come in, this will have to be case-based
        	// looks backwards to check that the preceding character is not part of a valid variable name
        	// avoids capturing generics as HTML
            
            bool Valid() {
	            
	            // backwards check (makes sure the last character is not the end of a reference name [differentiates from generics])
	            for (int i = startIndex - 1; i >= 0; i--) {
		            if (code[i].IsWhiteSpace()) continue;
		            if (code[i].IsValidReferenceNameCharacter()) {
			            return false;
		            }
		            break;
	            }
	            
	            // steps through and checks string validity
	            bool tagDone = false, onPropName = false, propEquals = false, parensUsed = false;
	            int lastPropNameStart = -1;
	            string lastPropName = null;
	            
	            void AddJsx(string key, string val) {
		            jsxFrags[key] = val;
	            }

	            void TryAddJsx(string key, string val) {
		            if (extractData) AddJsx(key, val);
	            }

	            void StartPropName(int i) {
		            onPropName = true;
		            lastPropNameStart = i;
	            }

	            for (int i = startIndex + 1; i < code.Length; i++) {
		            char c = code[i];
		            if (i == startIndex + 1) {
			            if (c.IsLetter()) continue;
		            }
		            else {
			            if (c.IsAlphanumeric()) {
				            if (tagDone && !onPropName) StartPropName(i);
				            continue;
			            }

			            if (tagDone && c == '-') { // TODO: temporary- remove dash
				            if (!onPropName) StartPropName(i);
				            continue;
			            }

			            if (c.IsWhiteSpace()) {
				            tagDone = true;
				            continue;
			            }

			            if (onPropName) {
				            
				            /*if (c.IsWhiteSpace()) { // TODO: true for name-only
					            TryAddJsx();
					            onPropName = false;
				            }*/

				            if (c == '=') {
					            if (propEquals) break; // no chaining equals
					            propEquals = true;
					            lastPropName = code[lastPropNameStart..i];
					            continue;
				            }

				            if (propEquals) {
					            DelimPair pair = c switch {
									'{' => braceDict[i],
									'\'' => singleQuoteDict[i],
									'"' => doubleQuoteDict[i],
									_ => null
					            };

					            if (pair == null) break; // must be one of the three
					            
					            onPropName = false;
					            propEquals = false;
					            i = pair.closeIndex;

					            if (extractData) {
						            string val = pair.contents(code);
						            if (c != '{') val = $"'{val}'";
						            AddJsx(lastPropName, val);
						            jsxFrags[lastPropName] = val;
					            }

					            continue;
				            }
			            } 
			            else {
				            if (c == '(') {
					            if (parensUsed) break; // TODO: throw exception
					            DelimPair pair = parenDict[i];

					            TryAddJsx("__parens__", pair.contents(code));
					            
					            i = pair.closeIndex;
					            parensUsed = true;
					            continue;
				            }
			            }

			            if (selfClosing) {
				            if (c == '/' && (i + 1) < code.Length && code[i + 1] == '>') {
					            closeIndex = i;
					            return true;
				            }
			            }
			            else {
				            if (c == '>') {
					            closeIndex = i;
					            return true;
				            }
			            }
		            }

		            break;
	            }

	            return false;
            }
            
            valid = Valid();
        }
    }
}