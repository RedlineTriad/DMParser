using Superpower.Display;

namespace DMChem.Parser {
    public enum DMToken
    {
        [Token(Category = "value", Example = "123")]
        NumericLiteral,
        [Token(Category = "value", Example = "\"This is a string\"")]
        StringLiteral,
        [Token(Category = "value", Example = "TRUE")]
        BooleanLiteral,
        [Token(Category = "seperator", Example = ".")]
        Dot,
        [Token(Category = "seperator", Example = "/")]
        Slash,
        [Token(Category = "operator", Example = "+")]
        Plus,
        [Token(Category = "operator", Example = "-")]
        Minus,
        [Token(Category = "operator", Example = "*")]
        Asterisk,
        [Token(Category = "operator", Example = "/=")]
        SlashEquals,
        [Token(Category = "operator", Example = "+=")]
        PlusEquals,
        [Token(Category = "operator", Example = "-=")]
        MinusEquals,
        [Token(Category = "operator", Example = "*=")]
        AsteriskEquals,
        [Token(Category = "operator", Example = "!")]
        Exclamation,
        [Token(Category = "operator", Example = "&")]
        Ampersand,
        [Token(Category = "operator", Example = "&&")]
        AmpersandAmpersand,
        [Token(Category = "operator", Example = "|")]
        Bar,
        [Token(Category = "operator", Example = "||")]
        BarBar,
        [Token(Category = "reference", Example = "myVar")]
        Identifier,
        [Token(Category = "seperator", Example = "(")]
        OpenParen,
        [Token(Category = "seperator", Example = ")")]
        CloseParen,
        [Token(Category = "seperator", Example = "[")]
        OpenBracket,
        [Token(Category = "seperator", Example = "]")]
        CloseBracket,
        [Token(Category = "operator", Example = "=")]
        Equals,
        [Token(Category = "operator", Example = "==")]
        EqualsEquals,
        [Token(Category = "operator", Example = "<")]
        LessThan,
        [Token(Category = "operator", Example = "<=")]
        LessThanEquals,
        [Token(Category = "operator", Example = ">")]
        GreaterThan,
        [Token(Category = "operator", Example = ">=")]
        GreaterThanEquals,
        [Token(Category = "operator", Example = "++")]
        Hash,
        [Token(Category = "keyword", Example = "#")]
        Define,
        [Token(Category = "keyword", Example = "define")]
        UnDef,
        [Token(Category = "keyword", Example = "undef")]
        PlusPlus,
        [Token(Category = "keyword", Example = "list")]
        ListKeyword,
        [Token(Category = "keyword", Example = "for")]
        ForKeyword,
        [Token(Category = "keyword", Example = "in")]
        InKeyword,
        [Token(Category = "keyword", Example = "to")]
        ToKeyword,
        [Token(Category = "keyword", Example = "if")]
        IfKeyword,
        [Token(Category = "keyword", Example = "else")]
        ElseKeyword,
        [Token(Category = "keyword", Example = "new")]
        NewKeyword,
        [Token(Category = "keyword", Example = "return")]
        RgbKeyword,
        [Token(Category = "keyword", Example = "rgb")]
        ReturnKeyword,
        [Token(Category = "seperator", Example = ",")]
        Comma,
        [Token(Category = "whitespace", Example = "whitespace")]
        TrailWhitespace,
        [Token(Category = "whitespace", Example = "whitespace")]
        LeadWhitespace,
        [Token(Category = "whitespace", Example = "newline")]
        Eol,
    }
}