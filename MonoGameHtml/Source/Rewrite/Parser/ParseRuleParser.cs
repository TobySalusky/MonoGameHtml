using System;
using System.Collections.Generic;
using System.Linq;
using MonoGameHtml.Tokenization;

namespace MonoGameHtml.Parser {
	public static class ParseRuleParser {

		public static Dictionary<string, ParseRule> ParseLexerRules() {
			
			var tokens = Tokenizer.Tokenize(ParseRules.LEXER_RULES)
				.RemoveSuperfluous();

			var specialRules = new (string name, ParseRule rule)[] {
				("__EOF__", new ParseRule.EndOfFile()) 
			};
			
			var tokenTypeRules = Util.GetValues<TokenType>().Select(tokenType => {
				var name = tokenType.GetName();
				var rule = (ParseRule) new ParseRule.One { Predicate = t => t.type == tokenType };
				return (name, rule);
			});

			var ruleSections = tokens.SplitOn(token => token.type == TokenType.CloseBrace).Select(val => val.ToArray());

			var composedRules = ruleSections.Select(section => {
				string name = section[0].value;
				var contents = section[2..];
				var rule = ComposeRule(contents, name);
				return (name, rule);
			}).ToArray();

			var allNamedRules = composedRules.Concat(tokenTypeRules).Concat(specialRules);

			// foreach (var (name, rule) in allNamedRules) {
			// 	Console.WriteLine($"{name} {rule.GetType().Name}");
			// }
			
			return allNamedRules.ToDictionary(val => val.name, val => val.rule);
		}

		public static ParseRule ComposeRule(IEnumerable<Token> ruleContents, string name = nameof(ParseRule.All)) {
			var tokenArr = ruleContents.ToArray();
			var outputRules = new List<ParseRule>();
			
			for (int i = 0; i < tokenArr.Length; i++) {
				var token = tokenArr[i];
				switch (token.type) {
					case TokenType.Identifier:
						// use other rules
						outputRules.Add(new ParseRule.Named { Name = token.value });
						break;
					case TokenType.Character:
						// keywords
						outputRules.Add(new ParseRule.One { Predicate = t => t.type == TokenType.Identifier && t.value == token.value[1..^1] });
						break;
					case TokenType.QuestionMark:
						// optional
						outputRules[^1] = new ParseRule.Optional { Proxy = outputRules[^1] };
						break;
					case TokenType.Caret:
						// not (only works for single token)
						outputRules[^1] = new ParseRule.Not { Proxy = outputRules[^1] };
						break;
					case TokenType.Plus:
						// one or more
						outputRules[^1] = new ParseRule.OneOrMore { Proxy = outputRules[^1] };
						break;
					case TokenType.Asterisk:
						// none or more
						outputRules[^1] = new ParseRule.Optional { 
							Proxy = new ParseRule.OneOrMore { Proxy = outputRules[^1] }
						};
						break;
					case TokenType.OpenParen:
						// parenthetical groupings
						int nestCount = 1;
						var subContents = tokenArr.Skip(i + 1).TakeWhile(t => {
							nestCount += t.type switch {
								TokenType.OpenParen => 1,
								TokenType.CloseParen => -1,
								_ => 0
							};
							return nestCount != 0;
						}).ToArray();
						
						outputRules.Add(ComposeRule(subContents));
						i += subContents.Length + 1; // skip over subContents!
						break;
					case TokenType.Bar:
						// or
						outputRules = new List<ParseRule> {
							new ParseRule.Any { Rules = new []{
								new ParseRule.All { Rules = outputRules }, 
								ComposeRule(tokenArr.Skip(i + 1))
							}}
						};
						goto Finished;
					default:
						throw new Exception($"Unexpected TokenType in lexer rule parsing ({token.type}).");
				}
			}
			
			Finished: { }

			return new ParseRule.All { Rules = outputRules, Name = name };
		}
	}
}