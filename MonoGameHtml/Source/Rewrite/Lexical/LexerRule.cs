using System;
using System.Collections.Generic;
using System.Linq;
using MonoGameHtml.Tokenization;

namespace MonoGameHtml.Lexical {
	public interface LexerRule {
		public static Dictionary<string, LexerRule> namedRules;
		
		public LexResult Apply(IEnumerable<Token> tokens);

		public class One : LexerRule {
			public Func<Token, bool> Predicate { get; set; }

			public LexResult Apply(IEnumerable<Token> tokens) {
				if (!tokens.Any()) {
					return LexResult.Fail;
				}

				var token = tokens.First();
				return new LexResult(Predicate(token), token, tokens.Skip(1));
			}
		}
		
		public class OneOrMore : LexerRule {
			public LexerRule Proxy { get; set; }
			public LexResult Apply(IEnumerable<Token> tokens) {
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
					return new LexResult(true, new TokenGroup(nameof(OneOrMore), groupingList.ToArray()), currentContinuation);
				}
				return LexResult.Fail;
			}
		}

		public class All : LexerRule {
			public IEnumerable<LexerRule> Rules { get; set; }
			public LexResult Apply(IEnumerable<Token> tokens) {
				var currentContinuation = tokens;
				var groupingList = new List<TokenLike>();

				foreach (var rule in Rules) {
					var result = rule.Apply(currentContinuation);
					if (!result.Succeeded) { 
						return LexResult.Fail;
					}

					currentContinuation = result.Continuation;
					groupingList.Add(result.Matched);
				}
				return new LexResult(true, new TokenGroup(nameof(All), groupingList.ToArray()), currentContinuation);
			}
		}
		
		public class Any : LexerRule {
			public IEnumerable<LexerRule> Rules { get; set; }
			public LexResult Apply(IEnumerable<Token> tokens) {

				foreach (var rule in Rules) {
					var result = rule.Apply(tokens);
					if (result.Succeeded) { 
						return result;
					}
				}
				return LexResult.Fail;
			}
		}

		public class Optional : LexerRule {
			public LexerRule Proxy { get; set; }
			public LexResult Apply(IEnumerable<Token> tokens) {
				
				var result = Proxy.Apply(tokens);

				return new LexResult(true, 
					new TokenGroup(nameof(Optional), result.Succeeded ? new []{ result.Matched } : new TokenLike[0]), 
					result.Succeeded ? result.Continuation : tokens);
			}
		}

		public class Named : LexerRule {
			public string Name { get; set; }

			public LexResult Apply(IEnumerable<Token> tokens) {
				return namedRules[Name].Apply(tokens);
			}
		}

		public class EndOfFile : LexerRule {
			public LexResult Apply(IEnumerable<Token> tokens) {
				if (tokens.Any()) { 
					return LexResult.Fail;
				}

				return new LexResult(true, null, tokens);
			}
		}
	}
}