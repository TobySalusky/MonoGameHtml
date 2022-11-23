using System;
using System.Collections.Generic;
using MonoGameHtml.Parser;
using MonoGameHtml.Tokenization;

namespace MonoGameHtml.MainMethod.Rewrite.AST; 

public class AbstractSyntaxTree {

    private static HashSet<string> boringNames = new HashSet<string> {
        nameof(ParseRule.All),
        nameof(ParseRule.Any),
        nameof(ParseRule.Optional),
        nameof(ParseRule.OneOrMore)
    };

    public static void WalkSyntax(TokenLike tokenLike, int indentationLevel = 0) {
        void LogAtIndentation(string name) {
            Console.WriteLine(new string(' ', Math.Max(0, indentationLevel - 1) * 2) + (indentationLevel > 0 ? "* " : "") + name);
        }
            
            
        switch (tokenLike) {
            case TokenGroup group:
                int newIndentation = indentationLevel;

                if (!boringNames.Contains(group.Name)) {
                    LogAtIndentation(group.Name);
                    newIndentation++;
                }

                foreach (var child in group.TokenLikes) {
                    WalkSyntax(child, newIndentation);
                }
                break;
            case Token token:
                LogAtIndentation($"{token.type} \"{token.value}\"");
                break;
        }
    }
}