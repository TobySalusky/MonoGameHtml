namespace MonoGameHtml.Parser {
	public static class ParseRules {

		public const string LEXER_RULES = @"
// TODO: remove
test {
	method
}

literal {
	String | Character | Float | Double | Integer
}

parenthetical_grouping {
	OpenParen expression CloseParen
}

expression {
	literal | Identifier | html_literal | new_object | parenthetical_grouping
}

statement {
	declaration | assignment
}

typed_identifier {
	type_name Identifier
}

// OPERATIONS -----------------------------------------------------------------

operation {
	expression operator expression
}

// type_caster is problematic due to lack of recursive look-back (collides with parenthetical_clause)
unary_operation {
	Plus | Minus
}

unary_operator {
	type_caster | Plus | Minus | ExclamationPoint | Tilde
}

operator {
	Plus | Minus | Asterisk | Slash
}

type_caster {
	OpenParen type_name CloseParen
}

// FILE -----------------------------------------------------------------

// TODO: for real tho  
file {
	method* __EOF__
}

// METHODS -----------------------------------------------------------------

// TODO: generics
method {
	type_name Identifier method_args OpenBrace method_innards CloseBrace
}

method_args {
	OpenParen (method_arg (Comma method_arg)*)? CloseParen
}

method_arg {
	typed_identifier (Equals expression)?
}

method_innards {
	method_innard*
}

method_innard {
	statement Semicolon
}

// CALLING -----------------------------------------------------------------

method_call_args {
	OpenParen (method_call_arg (Comma method_call_arg)*)? CloseParen
}

method_call_arg {
	(Identifier Colon)? expression
}

// CONTROL FLOW -----------------------------------------------------------------



// NEW - OBJECT CONSTRUCTION -----------------------------------------------------------------

new_array {
	'new' 
}

new_object {
	'new' type_name (method_call_args named_initializer_list? | named_initializer_list)
}

named_initializer_list {
	OpenBrace (named_initializer (Comma named_initializer)*)? CloseBrace
}

named_initializer {
	Identifier Equals expression
}

// LAMBDA -----------------------------------------------------------------

lambda {
	lambda_head FatArrow lambda_body
}

// TODO: (remember with and w/o parens)
lambda_head {
	
}

lambda_body {
	lambda_body_inline | lambda_body_multiline
}

lambda_body_inline {
	expression | statement
}

// TODO: lambda_body_inline_contents

lambda_body_multiline {
	OpenBrace method_innards CloseBrace
}

// DECLARATION -----------------------------------------------------------------

declaration {
	typed_identifier (Equals expression)?
}

declaration_modifier {
	'const'
}

visibility_modifier {
	'public' | 'private' | 'protected' | 'internal'
}

field_declaration {
	visibility_modifier* declaration
}

// ASSIGNMENT -----------------------------------------------------------------

assignment {
	Identifier Equals expression
}

// TYPES -----------------------------------------------------------------

type_name {
	Identifier | generic_type | tuple_type
}

generic_type {
	Identifier OpenAngle type_name (Comma type_name)* CloseAngle
}

tuple_type {
	OpenParen type_name (Comma type_name)* CloseParen
}

// HTML -----------------------------------------------------------------

html_literal {
	html_self_closed_opener | html_opener expression* html_closer
}

html_opener_stub {
	OpenAngle Identifier html_prop_pass*
}

html_opener {
	html_opener_stub CloseAngle
}

html_closer {
	OpenAngle Slash Identifier CloseAngle
}

html_self_closed_opener {
	html_opener_stub Slash CloseAngle
}

html_prop_pass {
	Identifier Equals OpenBrace expression CloseBrace
}
";
		// TODO: html_self_closed_opener should have /> forced next to each-other, html_closer too
	}
}