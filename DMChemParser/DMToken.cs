namespace DMChemParser {
    public enum DMToken
    {
        NumericLiteral,
        StringLiteral,
        TrueKeyword,
        FalseKeyword,
        Dot,
        Slash,
        Plus,
        Minus,
        Asterisk,
        SlashEquals,
        PlusEquals,
        MinusEquals,
        AsteriskEquals,
        Exclamation,
        Ampersand,
        AmpersandAmpersand,
        Bar,
        BarBar,
        Identifier,
        Datum,
        Var,
        OpenParen,
        CloseParen,
        OpenBracket,
        CloseBracket,
        Equals,
        EqualsEquals,
        LessThan,
        LessThanEquals,
        GreaterThan,
        GreaterThanEquals,
        PlusPlus,
        ListKeyword,
        ForKeyword,
        InKeyword,
        ToKeyword,
        IfKeyword,
        ElseKeyword,
        NewKeyword,
        Comma,
        TrailWhitespace,
        LeadWhitespace,
        Eol,
    }
}