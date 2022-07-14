namespace MonoGameHtml.Tokenization {
	public enum TokenType {
		WhiteSpace,
		
		String,
		Character,
		Float, Double,
		Integer,
		// Boolean,
		
		OpenParen, CloseParen, 
		OpenBrace, CloseBrace, 
		OpenBracket, CloseBracket,
		OpenAngle, CloseAngle,
		
		Comment,

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