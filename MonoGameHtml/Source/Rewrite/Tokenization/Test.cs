using System;
using System.Numerics;
using MonoGameHtml.Parser;

namespace MonoGameHtml.Tokenization {
	public class Test {
		public static void RunTest() {
			const string testFileStr = @"

void Hi(int i, Action f) {
	var test = 1;
	int i = 0;
	float f;
}

void Yo() {
	HtmlNode node = 
	(
		<div>
			<span class={'huh'}>yoo</span>
			<sep/>
		</div>
	);
}

float Calc(float f1, float f2 = 0.0f) {
	float f;
}

";
			
			var tokens = Tokenizer.Tokenize(testFileStr).RemoveSuperfluous();
			
			var lex = Parser.Parser.Parse(tokens, "file");
			
			foreach (var token in tokens) {
				Console.WriteLine($"{token.type}{(token.type == TokenType.WhiteSpace ? "" : $" (`{token.value}`)")}");
			}
			
			Console.WriteLine($"{lex.Succeeded}");




// 			const string testStr = @"
//
// const int hi = 78;
//
// public class Poggers {
// 	public static void Yo() {
// 		Console.WriteLine(""Sup!"");
// 		Console.WriteLine(""Why hello there, what \""is up!!!??"");
// 	}
// }
//
// Poggers pog = new Poggers(); // pog
//
// double d = 1.0; /* This is lit */ float f = 1.0f;
//
// object[] objs = new [] {
// 	10f, 10 * d, 10d, 10/d, 1 + 1.0 - 1.0f, 'a', '\'', ' ',""fjewiaofjewaio'fewafewa'fewa'fwe'a""
// };
// ";
// 			foreach (var token in Tokenizer.Tokenize(testStr)) {
// 				Console.WriteLine($"{token.type}{(token.type == TokenType.WhiteSpace ? "" : $" (`{token.value}`)")}");
// 			}
		}
	}
}