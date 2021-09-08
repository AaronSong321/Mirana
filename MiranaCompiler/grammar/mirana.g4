

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

chunk
    : block EOF
    ;

block
    : stat* retstat?
    ;
    
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

stat_assign: varlist '=' explist;
stat_cassign: var_ OpCassign exp;
stat_funcall: functioncall;
stat_label: label;
stat_break: 'break';
stat_goto: 'goto' NAME;
stat_do: 'do' block 'end';
stat_while: 'while' exp stat_do;
stat_repeat: 'repeat' block 'until' exp;
stat_if: stat_if_if stat_if_elseif* stat_if_else? 'end';
stat_fori: 'for' 'local'? NAME '=' exp ',' exp (',' exp)? stat_do;
stat_foreach: 'for' 'local'? namelist 'in' explist stat_do;
stat_fundecl: 'function' funcname funcbody;
stat_localfundecl: 'local' 'function' NAME funcbody;
stat_localdecl: 'local' attnamelist ('=' explist)?;
stat_semicolon: ';';

stat_if_if: 'if' predicateexp 'then' block;
stat_if_elseif: ('elseif'|'elif') predicateexp 'then' block;
stat_if_else: 'else' block;

stat_opda: opda var_;

predicateexp
: exp
| 'local' var_ '=' exp
;

attnamelist
    : NAME attrib (',' NAME attrib)*
    ;

attrib
    : ('<' NAME '>')?
    ;

retstat
    : 'return' explist? ';'?
    ;

label
    : '::' NAME '::'
    ;

funcname
    : NAME ('.' NAME)* (':' NAME)?
    ;

varlist
    : var_ (',' var_)*
    ;

namelist
    : NAME (',' NAME)*
    ;
    
varName: NAME (':' typeExp)?;

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
: literalType NullableMark?
;

literalType: TINT | TFLOAT | TSTRING | TBOOL | ANY | ATABLE | TUSERDATA;
NullableMark: '?';

stat_declare
: 'declare' localOrGlobal? NAME ':' typeExp
;

localOrGlobal: 'local' | 'global';

explist
: exp (',' exp)*
;

exp
: 'nil' | 'false' | 'true'
| functiondef
| prefixexp
| ifexp
| number
| string
| dots
| tableconstructor
| kotLambda
| <assoc=right> exp operatorPower exp
| operatorUnary exp
| exp operatorMulDivMod exp
| exp operatorAddSub exp
| <assoc=right> exp operatorStrcat exp
| exp operatorComparison exp
| exp operatorAnd exp
| exp operatorOr exp
| exp operatorBitwise exp
| expext
;

expext
: arrowLambda
| funLambda
| operatorLambda
| lambdaImplicitParam
| it
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
: operatorUnary
| opda
| operatorAddSub
| operatorAnd
| operatorBitwise
| operatorComparison
| operatorMulDivMod
| operatorOr
| operatorPower
| operatorStrcat
;

arrowLambda
: oneLambdaParamList arrow exp
| funLambdaHead arrow exp
;

lambdaImplicitParam: LambdaImplicitParam;

it: IT;

arrow: '->';

oneLambdaParamList: NAME|dots;

funLambda
: FUN LBRACE expBlock RBRACE
;

expBlock: stat* (retstat | explist)?;

funLambdaHead: '(' parlist ')';

prefixexp
    : varOrExp nameAndArgs*
    ;

functioncall
    : varOrExp nameAndArgs+
    ;

varOrExp
    : var_ | '(' exp ')'
    ;

var_
    : (NAME | it | lambdaImplicitParam | '(' exp ')' varSuffix) varSuffix*
    ;

varSuffix
    : nameAndArgs* ('[' exp ']' | '.' NAME)
    ;

nameAndArgs
: (':' NAME)? args
;

args
: '(' explist? ')' kotLambda
| '(' explist? ')' funLambda
| kotLambda
| funLambda
| '(' explist? ')'
| tableconstructor 
| string
;

functiondef
: 'function' funcbody
;

funcbody
    : '(' parlist? ')' block 'end'
    ;

parlist
    : namelist (',' dots)? | dots
    ;

tableconstructor
    : LBRACE fieldlist? RBRACE
    ;

fieldlist
    : field (fieldsep field)* fieldsep?
    ;

field
    : '[' exp ']' '=' exp | NAME '=' exp | exp
    ;

fieldsep
    : ',' | ';'
    ;
dots: '...';

operatorOr
	: 'or';

operatorAnd
	: 'and';

operatorComparison
	: '<' | '>' | '<=' | '>=' | '~=' | '==';

operatorStrcat
	: '..';

operatorAddSub
	: '+' | '-';

operatorMulDivMod
	: '*' | '/' | '%' | '//';

operatorBitwise
	: '&' | '|' | '~' | '<<' | '>>';

operatorUnary
    : 'not' | '#' | '-' | '~';
    
opda: OpAddOne | OpSubOne;

operatorPower
    : '^';

number
    : INT | HEX | FLOAT | HEX_FLOAT
    ;

string
    : NORMALSTRING | CHARSTRING | LONGSTRING
    ;

// LEXER

TINT: 'int';
TFLOAT: 'float';
TSTRING: 'tstring';
TBOOL: 'bool';
ANY: 'any';
ATABLE: 'atable';
TUSERDATA: 'userdata';

OpAddOne: '++';
OpSubOne: '~~';
OpCassign: '+=' | '-=' | '*=';
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

NAME
    : [a-zA-Z_][a-zA-Z_0-9]*
    ;

NORMALSTRING
    : '"' ( EscapeSequence | ~('\\'|'"') )* '"'
    ;

CHARSTRING
    : '\'' ( EscapeSequence | ~('\''|'\\') )* '\''
    ;

LONGSTRING
    : '[' NESTED_STR ']'
    ;

fragment
NESTED_STR
    : '=' NESTED_STR '='
    | '[' .*? ']'
    ;

INT
    : Digit+
    ;

HEX
    : '0' [xX] HexDigit+
    ;

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

fragment
ExponentPart
    : [eE] [+-]? Digit+
    ;

fragment
HexExponentPart
    : [pP] [+-]? Digit+
    ;

fragment
EscapeSequence
    : '\\' [abfnrtvz"'\\]
    | '\\' '\r'? '\n'
    | DecimalEscape
    | HexEscape
    | UtfEscape
    ;

fragment
DecimalEscape
    : '\\' Digit
    | '\\' Digit Digit
    | '\\' [0-2] Digit Digit
    ;

fragment
HexEscape
    : '\\' 'x' HexDigit HexDigit
    ;

fragment
UtfEscape
    : '\\' 'u{' HexDigit+ '}'
    ;

fragment
Digit
    : [0-9]
    ;

fragment
HexDigit
    : [0-9a-fA-F]
    ;

COMMENT
    : '--[' NESTED_STR ']' -> channel(HIDDEN)
    ;

LINE_COMMENT
    : '--'
    (                                               // --
    | '[' '='*                                      // --[==
    | '[' '='* ~('='|'['|'\r'|'\n') ~('\r'|'\n')*   // --[==AA
    | ~('['|'\r'|'\n') ~('\r'|'\n')*                // --AAA
    ) ('\r\n'|'\r'|'\n'|EOF)
    -> channel(HIDDEN)
    ;

WS
    : [ \t\u000C\r\n]+ -> skip
    ;

SHEBANG
    : '#' '!' ~('\n'|'\r')* -> channel(HIDDEN)
    ;

