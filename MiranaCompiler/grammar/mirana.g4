

grammar mirana;

@lexer::header{
using System;
using System.Collections.Generic;
}
@lexer::members{
private Stack<int> funLambdaLevel = new();
private void AddFunLambdaLevel() => funLambdaLevel.Push(0);
private bool IsInFunLambda => funLambdaLevel.Count != 0;
}

chunk: block EOF;

block: stat* retstat?;
    
stat
: stat_assign
| stat_cassign
| stat_funcall
| stat_label
| stat_break
| stat_goto
| stat_do
| stat_while
| stat_repeat
| stat_if
| stat_fori
| stat_foreach
| stat_fundecl
| stat_localfundecl
| stat_localdecl
| stat_semicolon
| stat_opda
| stat_declare
;

stat_assign: varlist OpAssign explist;
stat_cassign: assignableExp OpCassign exp;
stat_funcall: functioncall;
stat_label: label;
stat_break: BREAK;
stat_goto: GOTO NAME;
stat_do: DO block END;
stat_while: WHILE exp stat_do;
stat_repeat: REPEAT block UNTIL exp;
stat_if: stat_if_if stat_if_elseif* stat_if_else? END;
stat_fori: FOR LOCAL? NAME OpAssign exp OpComma exp (OpComma exp)? stat_do;
stat_foreach: FOR LOCAL? namelist IN explist stat_do;
stat_fundecl: FUNCTION funcname funcbody;
stat_localfundecl: LOCAL FUNCTION NAME funcbody;
stat_localdecl: LOCAL attNameList (OpAssign explist)?;
stat_semicolon: ';';
stat_opda: opda assignableExp;

stat_if_if: IF predicateexp THEN block;
stat_if_elseif: (ELSEIF|ELIF) predicateexp THEN block;
stat_if_else: ELSE block;

predicateexp
: exp
| LOCAL assignableExp OpAssign exp
;

attNameList: attName (OpComma attName)*;
attName: NAME attribute?;
attribute: OpLt NAME OpGt;

retstat: RETURN explist? ';'?;

label: OpDoubleColon NAME OpDoubleColon;

funcname: NAME (OpDot NAME)* (OpColon NAME)?;

varlist: assignableExp (OpComma assignableExp)*;

namelist: NAME (OpComma NAME)*;
    
varName: NAME (OpColon typeExp)?;

typeExp
: funcType
;

funcType
: typeList arrow typeList
| unionType
;

typeList
: '(' ')'
| '(' typeExp (',' typeExp)* ')'
;

unionType
: primaryType ('|' primaryType)*
;

primaryType
: simpleArrayType
| '(' typeExp ')'
| typeofExp
;

typeofExp
: 'typeof' '(' exp ')'
| 'typeof' tableconstructor
;

simpleArrayType
: simpleType
| simpleArrayType '[]'
;

simpleType
: literalType OpQues?
;

literalType: TINT | TFLOAT | TSTRING | TBOOL | ANY | ATABLE | TUSERDATA;

stat_declare: 'declare' localOrGlobal? NAME OpColon typeExp;

localOrGlobal: LOCAL | GLOBAL;

explist
: exp (OpComma exp)*
;

exp
: NIL
| TRUE | FALSE
| funcLiteral
| prefixexp
| ifexp
| number
| string
| dots
| it
| lambdaImplicitParam
| tableconstructor
| kotLambda
| operatorLambda
| <assoc=right> exp operatorPower exp
| operatorUnary exp
| exp operatorMulDivMod exp
| exp operatorAddSub exp
| <assoc=right> exp operatorStrcat exp
| exp operatorComparison exp
| exp operatorAnd exp
| exp operatorOr exp
| exp OpAmpersand exp
| exp OpPipe exp
| exp OpTilde exp
| exp (OpShl | OpShr) exp
;

ifexp
: ifexp_if ifexp_elif* ifexp_else?
;

ifexp_if: 'if' predicateexp LBRACE expBlock RBRACE;
ifexp_elif: 'elif' predicateexp LBRACE expBlock RBRACE;
ifexp_else: 'else' LBRACE expBlock RBRACE;

kotLambda
: LBRACE parlist? arrow expBlock RBRACE
| LBRACE '(' parlist? ')' arrow expBlock RBRACE
;

operatorLambda
: LBRACE opcom RBRACE
;

opcom
: operatorAddSub
| opda
| operatorUnary
| operatorAnd
| operatorBitwise
| operatorComparison
| operatorMulDivMod
| operatorOr
| operatorPower
| operatorStrcat
;

lambdaImplicitParam: LambdaImplicitParam;
it: IT;

arrow: OpArrow;

expBlock: stat* (retstat | explist)?;

prefixexp: varOrExp nameAndArgs*;

functioncall: varOrExp nameAndArgs+;

varOrExp: assignableExp | '(' exp ')';

assignableExp: (NAME | it | lambdaImplicitParam | '(' exp ')' varSuffix) varSuffix*;

varSuffix: nameAndArgs* ('[' exp ']' | OpDot NAME);
nameAndArgs: (OpColon NAME)? args;

args
: '(' explist? ')' kotLambda
| kotLambda
| '(' explist? ')'
| tableconstructor 
| string
;

funcLiteral
: FUNCTION funcbody
;

funcbody: '(' parlist? ')' block END;

parlist: namelist (OpComma dots)? | dots;

tableconstructor: LBRACE fieldlist? RBRACE;

fieldlist: field (fieldsep field)* fieldsep?;

field: '[' exp ']' OpAssign exp | NAME OpAssign exp | exp;

fieldsep: OpComma | ';';

dots: '...';

operatorOr: 'or';

operatorAnd: 'and';

operatorComparison: OpLt | OpGt | '<=' | '>=' | '~=' | '==';

operatorStrcat: '..';

operatorAddSub: '+' | '-';

operatorMulDivMod: '*' | '/' | '%' | '//';

operatorBitwise
: OpAmpersand
| OpPipe
| OpTilde
| OpShl
| OpShr
;

operatorUnary: 'not' | '#' | '-' | '~';
    
opda: OpAddOne | OpSubOne;

operatorPower: '^';

number: INT | HEX | FLOAT | HEX_FLOAT;

string: NORMALSTRING | CHARSTRING | LONGSTRING;

// LEXER

TINT: 'int';
TFLOAT: 'float';
TSTRING: 'tstring';
TBOOL: 'bool';
ANY: 'any';
ATABLE: 'atable';
TUSERDATA: 'userdata';


// control keywords
RETURN: 'return';
IF: 'if';
FOR: 'for';
BREAK: 'break';
ELSE: 'else';
ELIF: 'elif';
ELSEIF: 'elseif';
DO: 'do';
THEN: 'then';
GOTO: 'goto';
END: 'end';
WHILE: 'while';
REPEAT: 'repeat';
UNTIL: 'until';
IN: 'in';
LOCAL: 'local';
FUNCTION: 'function';
DECLARE: 'declare';
GLOBAL: 'global';
NIL: 'nil';
FALSE: 'false';
TRUE: 'true';

OpAddOne: '++';
OpSubOne: '~~';
OpDoubleColon: '::';
OpArrow: '->';
OpCassign: '+=' | '-=' | '*=';
OpShr: '>>';
OpShl: '<<';
IT: { IsInFunLambda }? 'it';
LambdaImplicitParam: { IsInFunLambda }? '$' [1-8];
FUN: { AddFunLambdaLevel(); } 'fun';
LBRACE: {
if (IsInFunLambda) {
    funLambdaLevel.Push(funLambdaLevel.Pop()+1);
}
} '{';
RBRACE: {
if (IsInFunLambda) {
    int a = funLambdaLevel.Pop()-1;
    if (a != 0)
        funLambdaLevel.Push(a);
}
}'}';
OpAssign: '=';
OpComma: ',';
OpColon: ':';
OpLt: '<';
OpGt: '>';
OpDot: '.';
OpQues: '?';
OpAmpersand: '&';
OpPipe: '|';
OpTilde: '~';


NAME: [a-zA-Z_][a-zA-Z_0-9]*;

NORMALSTRING: '"' ( EscapeSequence | ~('\\'|'"') )* '"';

CHARSTRING: '\'' ( EscapeSequence | ~('\''|'\\') )* '\'';

LONGSTRING: '[' NESTED_STR ']';

fragment NESTED_STR
: '=' NESTED_STR '='
| '[' .*? ']'
;

INT: Digit+;

HEX: '0' [xX] HexDigit+;

FLOAT
: Digit+ '.' Digit* ExponentPart?
| '.' Digit+ ExponentPart?
| Digit+ ExponentPart
;

HEX_FLOAT
: '0' [xX] HexDigit+ '.' HexDigit* HexExponentPart?
| '0' [xX] '.' HexDigit+ HexExponentPart?
| '0' [xX] HexDigit+ HexExponentPart
;

fragment ExponentPart
: [eE] [+-]? Digit+
;

fragment HexExponentPart
: [pP] [+-]? Digit+
;

fragment EscapeSequence
: '\\' [abfnrtvz"'\\]
| '\\' '\r'? '\n'
| DecimalEscape
| HexEscape
| UtfEscape
;

fragment DecimalEscape
: '\\' Digit
| '\\' Digit Digit
| '\\' [0-2] Digit Digit
;

fragment HexEscape: '\\' 'x' HexDigit HexDigit;

fragment UtfEscape: '\\' 'u{' HexDigit+ '}';

fragment Digit: [0-9];

fragment HexDigit: [0-9a-fA-F];

COMMENT: '--[' NESTED_STR ']' -> channel(HIDDEN);

LINE_COMMENT
: '--' (                                               // --
    | '[' '='*                                      // --[==
    | '[' '='* ~('='|'['|'\r'|'\n') ~('\r'|'\n')*   // --[==AA
    | ~('['|'\r'|'\n') ~('\r'|'\n')*                // --AAA
) ('\r\n'|'\r'|'\n'|EOF)
-> channel(HIDDEN)
;

WS: [ \t\u000C\r\n]+ -> skip;

SHEBANG: '#' '!' ~('\n'|'\r')* -> channel(HIDDEN);
