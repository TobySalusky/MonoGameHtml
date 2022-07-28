using System;
using System.Collections.Generic;
using System.Linq;
using MonoGameHtml.Tokenization;

namespace MonoGameHtml.Parser {
	public interface ParseRule {
		public static Dictionary<string, ParseRule> namedRules;
		
		public ParseResult Apply(IEnumerable<Token> tokens);

		public class One : ParseRule {
			public Func<Token, bool> Predicate { get; set; }

			public ParseResult Apply(IEnumerable<Token> tokens) {
				if (!tokens.Any()) {
					return ParseResult.Fail;
				}

				var token = tokens.First();
				return new ParseResult(Predicate(token), token, tokens.Skip(1));
			}
		}
		
		public class OneOrMore : ParseRule {
			public ParseRule Proxy { get; set; }
			public ParseResult Apply(IEnumerable<Token> tokens) {
				bool hasSucceeded = false;
				var currentContinuation = tokens;
				var groupingList = new List<TokenLike>();
				while (true) { 
					var result = Proxy.Apply(currentContinuation);

					if (!result.Succeeded) {
						break;
					}

					hasSucceeded = true;
					currentContinuation = result.Continuation;
					groupingList.Add(result.Matched);
				}

				if (hasSucceeded) { 
					return new ParseResult(true, new TokenGroup(nameof(OneOrMore), groupingList.ToArray()), currentContinuation);
				}
				return ParseResult.Fail;
			}
		}

		public class All : ParseRule {
			public IEnumerable<ParseRule> Rules { get; set; }
			public ParseResult Apply(IEnumerable<Token> tokens) {
				var currentContinuation = tokens;
				var groupingList = new List<TokenLike>();

				foreach (var rule in Rules) {
					var result = rule.Apply(currentContinuation);
					if (!result.Succeeded) { 
						return ParseResult.Fail;
					}

					currentContinuation = result.Continuation;
					groupingList.Add(result.Matched);
				}
				return new ParseResult(true, new TokenGroup(nameof(All), groupingList.ToArray()), currentContinuation);
			}
		}
		
		public class Any : ParseRule {
			public IEnumerable<ParseRule> Rules { get; set; }
			public ParseResult Apply(IEnumerable<Token> tokens) {

				foreach (var rule in Rules) {
					var result = rule.Apply(tokens);
					if (result.Succeeded) { 
						return result;
					}
				}
				return ParseResult.Fail;
			}
		}

		public class Optional : ParseRule {
			public ParseRule Proxy { get; set; }
			public ParseResult Apply(IEnumerable<Token> tokens) {
				
				var result = Proxy.Apply(tokens);

				return new ParseResult(true, 
					new TokenGroup(nameof(Optional), result.Succeeded ? new []{ result.Matched } : new TokenLike[0]), 
					result.Succeeded ? result.Continuation : tokens);
			}
		}

		public class Named : ParseRule {
			public string Name { get; set; }

			public ParseResult Apply(IEnumerable<Token> tokens) {
				return namedRules[Name].Apply(tokens);
			}
		}

		public class EndOfFile : ParseRule {
			public ParseResult Apply(IEnumerable<Token> tokens) {
				if (tokens.Any()) { 
					return ParseResult.Fail;
				}

				return new ParseResult(true, null, tokens);
			}
		}
	}
}