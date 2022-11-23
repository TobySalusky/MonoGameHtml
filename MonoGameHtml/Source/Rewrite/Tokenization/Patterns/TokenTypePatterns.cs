using System;
using System.Collections.Generic;
using System.Linq;

namespace MonoGameHtml.Tokenization.Patterns {

	public static class TokenTypePatterns {

		public static readonly string[] keywords = new [] {
			/*"bool", "byte", "sbyte", "short", "ushort", "int", "uint", "long", "ulong", "double", "float", "decimal",
			"string", "char", "void", "object",*/ "typeof", "sizeof", "null", /*"true", "false",*/ "if", "else", "while", "for", "foreach", "do", "switch",
			"case", "default", "lock", "try", "throw", "catch", "finally", "goto", "break", "continue", "return", "public", "private", "internal",
			"protected", "static", "readonly", "sealed", "const", "fixed", "stackalloc", "volatile", "new", "override", "abstract", "virtual",
			"event", "extern", "ref", "out", "in", "is", "as", "params", /*"__arglist", "__makeref", "__reftype", "__refvalue",*/ "this", "base",
			"namespace", "using", "class", "struct", "interface", "enum", "delegate", "checked", "unchecked", "unsafe", "operator", "implicit", "explicit"
		};

		public static TokenPattern GetPatternFor(TokenType type) {
			var integralPattern = new OneOrMore(StringUtil.IsNumber);
			var decimalPattern = new All(
				new OneOrMore(StringUtil.IsNumber), 
				new One('.'), 
				new OneOrMore(StringUtil.IsNumber)
			);

			var identifierPattern = new All(
				new Optional(new One('@')),
				new One(c => c.IsLetter() || c == '_'),
				new Optional(new OneOrMore(c => c.IsAlphanumeric() || c == '_'))
			);
			
			var normalStringRule = new All( // TODO: add special strings!
				Exact('"'),
				new Optional(new OneOrMore(new Any(
					new One(c => c != '"'),
					LookingBack(1, new Match("\\\""))
				))),
				Exact('"')
				);
			
			var singleLineComment = new All(
				new Match("//"), new OneOrMore(c => c != '\n')
				);
			
			var multiLineComment = new All(
				new Match("/*"),
				new OneOrMore(new Any(
					new NotMatch("*"), new NotMatch("*/")
					)),
				new Match("*/")
			);

			static TokenPattern Exact(char c) { 
				return new One(c);
			}

			static TokenPattern LookingBack(int lookBackCount, TokenPattern pattern) {
				return new All(new Skip(-lookBackCount), pattern);
			}

			static TokenPattern AnyCase(char eq) { 
				return new One(c => c.EqualsAnyCase(eq));
			}

			static TokenPattern AnyString(params string[] strs) { 
				return new Any(strs.Select(str => (TokenPattern) new Match(str)).ToArray());
			}

			return type switch {
				TokenType.Identifier => identifierPattern,
				
				TokenType.Integer => integralPattern,
				
				TokenType.Double => decimalPattern,
				TokenType.Float => new All(decimalPattern, AnyCase('f')),
				
				// TokenType.Boolean => AnyString("true", "false"),

				TokenType.OpenParen => Exact('('),
				TokenType.CloseParen => Exact(')'),
				
				TokenType.OpenBrace => Exact('{'),
				TokenType.CloseBrace => Exact('}'),
				
				TokenType.OpenBracket => Exact('['),
				TokenType.CloseBracket => Exact(']'),
				
				TokenType.OpenAngle => Exact('<'),
				TokenType.CloseAngle => Exact('>'),
				
				TokenType.PlusEquals => new Match("+="),
				TokenType.MinusEquals => new Match("-="),
				TokenType.TimesEquals => new Match("*="),
				TokenType.DivideEquals => new Match("/="),
				TokenType.ModEquals => new Match("%="),
				TokenType.LeftShiftEquals => new Match("<<="),
				TokenType.RightShiftEquals => new Match(">>="),
				TokenType.AndEquals => new Match("&="),
				TokenType.OrEquals => new Match("|="),
				TokenType.XorEquals => new Match("^="),
				TokenType.NullCoalesceEquals => new Match("??="),
				
				TokenType.EqualsEquals => new Match("=="),
				TokenType.NotEquals => new Match("!="),
				TokenType.DotDot => new Match(".."),
				TokenType.LogicalAnd => new Match("&&"),
				TokenType.LogicalOr => new Match("||"),
				TokenType.NullCoalesce => new Match("??"),
				TokenType.LeftShift => new Match("<<"),
				TokenType.RightShift => new Match(">>"),
				TokenType.RightTripleShift => new Match(">>>"),
				
				TokenType.Semicolon => Exact(';'),
				TokenType.Colon => Exact(':'),
				TokenType.At => Exact('@'),
				TokenType.Asterisk => Exact('*'),
				TokenType.Dot => Exact('.'),
				TokenType.Equals => Exact('='),
				TokenType.ExclamationPoint => Exact('!'),
				TokenType.QuestionMark => Exact('?'),
				TokenType.Slash => Exact('/'),
				TokenType.PlusPlus => new Match("++"),
				TokenType.MinusMinus => new Match("--"),
				TokenType.Plus => Exact('+'),
				TokenType.Minus => Exact('-'),
				TokenType.Caret => Exact('^'),
				TokenType.Percent => Exact('%'),
				TokenType.DollarSign => Exact('$'),
				TokenType.Tilde => Exact('~'),
				TokenType.Ampersand => Exact('&'),
				TokenType.Bar => Exact('|'),
				TokenType.Comma => Exact(','),
				TokenType.FatArrow => new Match("=>"),
				TokenType.GreaterThanOrEqual => new Match("<="),
				TokenType.LessThanOrEqual => new Match(">="),

				// TokenType.Keyword => AnyString(keywords),
				TokenType.WhiteSpace => new OneOrMore(StringUtil.IsWhiteSpace),
				TokenType.String => normalStringRule,
				TokenType.Character => new All( // TODO: sus, (b/c character should only contain one, but must worry for multi-character combos (like \\))
					new One('\''),
					new OneOrMore(new Any(new Match("\\'"), new NotMatch("'"))),
					new One('\'')
				),
				
				TokenType.Comment => new Any(singleLineComment, multiLineComment)
			};
			throw new Exception($"No pattern for TokenType: {type}");
		}
	}
}