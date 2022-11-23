namespace MonoGameHtml.Tokenization {
	public enum TokenType {
		WhiteSpace,
		
		String,
		Character,
		Float, Double,
		Integer,
		// Boolean,
		
		GreaterThanOrEqual,
		LessThanOrEqual,

		OpenParen, CloseParen, 
		OpenBrace, CloseBrace, 
		OpenBracket, CloseBracket,
		OpenAngle, CloseAngle,
		
		Comment,
		
		EqualsEquals,
		DotDot,
		NotEquals,
		
		PlusEquals,
		MinusEquals,
		TimesEquals,
		DivideEquals,
		AndEquals,
		OrEquals,
		XorEquals,
		ModEquals,
		LeftShiftEquals,
		RightShiftEquals,
		LeftShift,
		RightShift,
		RightTripleShift,
		NullCoalesceEquals,

		LogicalOr,
		LogicalAnd,
		NullCoalesce,

		FatArrow,
		Equals,
		Semicolon,
		Asterisk,
		At,
		Colon,
		Dot,
		QuestionMark,
		ExclamationPoint,
		Slash,
		MinusMinus,
		PlusPlus,
		Minus,
		Plus,
		Percent,
		DollarSign,
		Ampersand,
		Bar,
		Caret,
		Tilde,
		Comma,
		
		// Keyword,
		
		Identifier,
	}
}
