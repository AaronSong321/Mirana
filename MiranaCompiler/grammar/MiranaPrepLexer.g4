
lexer grammar MiranaPrepLexer;

@lexer::members{
//private int parenLevel;
private bool insideMacro;
}

MacroDefine: '#define' -> pushMode(MacroParameterSearch);

NL: '\r\n' | '\r' | '\n';
NonNewLineChar: ~('\r'|'\n');

mode MacroSubstitutionTextSearch;

MSTS_NL: ('\r\n' | '\r' | '\n') -> popMode, type(NL);
MSTS_AnythingElse: ~('\r'|'\n');

mode MacroParameterSearch;

LPAREN: '(';
RPAREN: ')' -> popMode, pushMode(MacroSubstitutionTextSearch);
COMMA: ',';
DOTS: '...';
NAME
:  [a-zA-Z_][a-zA-Z_0-9]*
;
MPS_WS: [ \t];
//MPS_NL: '\r\n' | '\r' | '\n';
