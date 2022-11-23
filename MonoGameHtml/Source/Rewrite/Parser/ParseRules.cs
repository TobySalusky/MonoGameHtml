namespace MonoGameHtml.Parser {
	public static class ParseRules {

		public const string LEXER_RULES = @"
literal {
	String | Character | Float | Double | Integer | boolean
}

interpolated_string {
	DollarSign (interpolated_string | verbatim_string | String)
}

// TODO: make double quote-ing work inside verbatim string (maybe must change lexer??)
verbatim_string {
	At (interpolated_string | String)
}

special_string {
	interpolated_string | verbatim_string
}

boolean {
	'true' | 'false'
}

parenthetical_grouping {
	OpenParen expression CloseParen
}

postfix {
	non_operator_postfix | DotDot
}

non_operator_postfix {
	function_call | property_access | member_index | invocation | PlusPlus | MinusMinus | with_conjunction | as_type | is_type | ternary_body | switch_expression_body
}

ternary_body {
	QuestionMark expression Colon expression
}

as_type {
	'as' type_name
}

// TODO: implement a bunch of pattern matching stuff
is_type {
	'is' ('not')? (type_name | 'null') Identifier?
}

with_conjunction {
	'with' named_initializer_list
}

switch_expression_body {
	'switch' OpenBrace (switch_expression_chunk (Comma switch_expression_chunk)* Comma?)? CloseBrace
}

switch_expression_chunk {
	('_' | case_pattern) FatArrow expression
}

tuple {
	OpenParen expression Comma expression (Comma expression)* CloseParen
}

prefix_expression {
	(ExclamationPoint | PlusPlus | MinusMinus | Tilde | Plus | Minus | type_caster | DotDot | 'await') expression_component
}

// TODO: 	`return i is int ? 0 : 1;` -- thinks type is int? (maybe add non-greedy-ness??)
expression {
	((expression_component non_operator_postfix* operator expression) | expression_component postfix*)
}

expression_component {
	prefix_expression | new_object | new_array_shorthand | list_initialized_object | literal | special_string | lambda | Identifier | html_literal | tuple | parenthetical_grouping
}

qualified_statement {
	control_flow | unqualified_statement Semicolon | Semicolon | method | code_block
}

statement {
	control_flow | unqualified_statement
}

// NOTE: `expression` here is kind of a cop-out TBH
unqualified_statement {
	yield_return_statement | yield_break_statement | return_statement | break_statement | continue_statement | goto_statement | using_block_statement | throw_statement | declaration | assignment | do_while_loop | expression
}

typed_identifier {
	type_name Identifier
}

// OPERATIONS -----------------------------------------------------------------

function_call {
	Dot Identifier generic_type_specifier? method_call_args
}

invocation {
	method_call_args
}


property_access {
	Dot Identifier
}

member_index {
	OpenBracket expression (Comma expression)* CloseBracket
}

operator {
	Plus | Minus | Asterisk | Slash | OpenAngle | CloseAngle | EqualsEquals | NotEquals | GreaterThanOrEqual | LessThanOrEqual | DotDot | LogicalOr | LogicalAnd | Bar | Ampersand | Caret | NullCoalesce | Percent
}

type_caster {
	OpenParen type_name CloseParen
}

// FILE -----------------------------------------------------------------

// TODO: for real tho  
file {
	file_innard* __EOF__
}

file_innard {
	using_statement | namespace_statement | method | namespace | class | struct | interface | record
}

// IMPORTS (USINGS) -------------------------------------------------------

// TODO: ADD using () {} blocks and using var blah = something; 

using_statement {
	'using' type_name Semicolon
}

// NAME SPACES ------------------------------------------------------------

namespace {
	'namespace' type_name OpenBrace (class | struct | interface | record | namespace)* CloseBrace
}

namespace_statement {
	'namespace' type_name Semicolon
}

// CLASSES ----------------------------------------------------------------

// TODO: more attribute stuff :)

record {
	attribute* visibility_modifier? 'static'? 'record' ('class' | 'struct')? type_name generic_defs? extends_block? method_args? (Semicolon | OpenBrace class_innard* CloseBrace Semicolon?)
}

interface {
	attribute* visibility_modifier? 'static'? 'interface' type_name generic_defs? extends_block? OpenBrace class_innard* CloseBrace
}

struct {
	attribute* visibility_modifier? 'static'? 'struct' type_name generic_defs? extends_block? OpenBrace class_innard* CloseBrace
}

class {
	attribute* visibility_modifier? 'static'? 'class' type_name generic_defs? extends_block? OpenBrace class_innard* CloseBrace
}

extends_block {
	Colon type_name (Comma type_name)*
}

class_innard {
	class_variable_declaration | class_prop_declaration | class_method_declaration | class_constructor
}

class_constructor {
	visibility_modifier? type_name method_args (Colon ('this' | 'base') method_call_args)? code_block
}

class_static_block {
	'static' type_name OpenParen CloseParen code_block
}

class_method_declaration {
	visibility_modifier? (method | method_stub Semicolon) 
}

class_variable_declaration {
	visibility_modifier? 'static'? declaration Semicolon
}

class_prop_declaration {
	visibility_modifier? 'static'? typed_identifier OpenBrace prop_chunk* CloseBrace (Equals expression Semicolon)?
}

prop_chunk {
	(visibility_modifier? prop_keyword) (((FatArrow (expression | statement))? Semicolon) | code_block)
}

prop_keyword {
	'set' | 'get' | 'init'
}

// METHODS -----------------------------------------------------------------

// TODO: generics
method {
	 method_stub (code_block | FatArrow expression Semicolon)
}

method_stub {
	'static'? 'abstract'? ('virtual' | 'override')? 'async'? type_name Identifier generic_defs? method_args
}

generic_defs {
	OpenAngle Identifier (Comma Identifier)* CloseAngle
}

code_block {
	OpenBrace method_innards CloseBrace
}

method_args {
	OpenParen (method_arg (Comma method_arg)*)? CloseParen
}

method_arg {
	param_modifier? typed_identifier (Equals expression)?
}

param_modifier {
	'ref' | 'params' | 'in' | 'out'
}

method_innards {
	method_innard*
}

method_innard {
	label? qualified_statement
}

// CALLING -----------------------------------------------------------------

method_call_args {
	OpenParen (method_call_arg (Comma method_call_arg)*)? CloseParen
}

method_call_arg {
	(Identifier Colon)? ('ref' | 'out')? expression
}

// CONTROL FLOW -----------------------------------------------------------------

control_flow {
	if_statement | for_loop | foreach_loop | while_loop | switch_statement | try_catch | using_block
}

control_flow_chunk {
	code_block | qualified_statement
}

if_statement {
	'if' parenthetical_grouping control_flow_chunk ('else' 'if' parenthetical_grouping control_flow_chunk)* ('else' control_flow_chunk)?
}

switch_statement {
	'switch' parenthetical_grouping OpenBrace (case_label method_innards)* CloseBrace
}

case_label {
	('case' case_pattern | 'default') Colon
}

// TODO: pattern matching
case_pattern {
	(type_name Identifier) | expression
}

using_block_statement {
	'using' declaration
}

using_block {
	'using' OpenParen (declaration | expression) CloseParen code_block
}

try_catch {
	'try' code_block ('catch' (OpenParen typed_identifier CloseParen)? code_block)* ('finally' code_block)?
}

for_loop {
	'for' OpenParen statement Semicolon expression Semicolon statement? CloseParen control_flow_chunk
}

foreach_loop {
	'foreach' OpenParen typed_identifier 'in' expression CloseParen control_flow_chunk
}

while_loop {
	'while' parenthetical_grouping control_flow_chunk
}

do_while_loop {
	'do' control_flow_chunk 'while' parenthetical_grouping
}

break_statement {
	'break'
}

continue_statement {
	'continue'
}

goto_statement {
	'goto' Identifier
}

label {
	Identifier Colon
}

// THROWING ERRORS --------------------------------------------------------------------------- 

throw_statement {
	'throw' expression
}

// NEW - OBJECT CONSTRUCTION -----------------------------------------------------------------

new_object {
	'new' type_name? (method_call_args initializer_list? | initializer_list)
}

new_array_shorthand {
	'new' OpenBracket CloseBracket list_initializer_list
}

list_initialized_object {
	list_initializer_list
}

initializer_list {
	named_initializer_list | list_initializer_list | dictionary_initializer_list
}

dictionary_initializer_list {
	OpenBrace (dictionary_initializer (Comma dictionary_initializer)* Comma?)? CloseBrace
}

dictionary_initializer {
	OpenBracket expression CloseBracket Equals expression
}

list_initializer_list {
	OpenBrace (expression (Comma expression)* Comma?)? CloseBrace
}

named_initializer_list {
	OpenBrace (named_initializer (Comma named_initializer)* Comma?)? CloseBrace
}

named_initializer {
	Identifier Equals expression
}

// ATTRIBUTES -------------------------------------------------------------

attribute {
	OpenBracket attribute_target? attribute_creation (Comma attribute_creation)* CloseBracket
}

attribute_creation {
	type_name (OpenParen (attribute_parameter (Comma attribute_parameter)*)? CloseParen)?
}

attribute_parameter {
	(Identifier Equals)? expression
}

// NOTE: (technically set-list of possibilities, although this is probably fine...)
attribute_target {
	Identifier Colon
}

// LAMBDA -----------------------------------------------------------------

lambda {
	lambda_head FatArrow lambda_body
}

lambda_head {
	'static'? 'async'? type_name? (Identifier | (OpenParen (typed_identifier (Comma typed_identifier))? CloseParen) | (OpenParen (Identifier (Comma Identifier))? CloseParen))
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
	'const'? typed_identifier (Equals expression)? (Comma Identifier (Equals expression)?)*
}

visibility_modifier {
	('protected' 'internal') | 'public' | 'private' | 'protected' | 'internal'
}

field_declaration {
	visibility_modifier* declaration
}

// ASSIGNMENT -----------------------------------------------------------------

assignment {
	simple_assignment | operation_assignment
}

simple_assignment {
	expression Equals expression
}

operation_assignment {
	expression operation_assigner expression
}

operation_assigner {
	PlusEquals | MinusEquals | TimesEquals | DivideEquals | AndEquals | OrEquals | XorEquals | ModEquals | LeftShiftEquals | RightShiftEquals | NullCoalesceEquals
}

// RETURNING ---------------------------------------------------------------

return_statement {
	'return' expression?
}

// YIELDING ---------------------------------------------------------------
yield_return_statement {
	'yield' 'return' expression
}

yield_break_statement {
	'yield' 'break'
}

// TYPES -----------------------------------------------------------------

// TODO: dotted types ex. T.T1.T2, T<T1, T2>.T3.T4
// TODO: arrays, pointers, and nullable '?' types

type_name {
	(dottable_type | tuple_type) (array_chunk | Asterisk | QuestionMark)*
}

array_chunk {
	OpenBracket Comma* CloseBracket
}

dottable_type {
	dottable_segment (Dot dottable_segment)*
}

dottable_segment {
	generic_type | Identifier
}

generic_type {
	Identifier generic_type_specifier
}

generic_type_specifier {
	OpenAngle type_name (Comma type_name)* CloseAngle
}

tuple_type {
	OpenParen tuple_segment (Comma tuple_segment)* CloseParen
}

// (tuple members can optionally have names)
tuple_segment {
	type_name Identifier?
}

// HTML -----------------------------------------------------------------

html_literal {
	html_self_closed_opener | html_opener html_innards html_closer
}

html_innards {
	(html_inner_text (html_literal | html_jsx))* html_inner_text
}

html_jsx {
	OpenBrace expression CloseBrace
}

html_inner_text {
	((OpenAngle | OpenBrace)^)*
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