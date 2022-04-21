

parser grammar MiranaPrepParser;

options { tokenVocab=MiranaPrepLexer; }

chunk: chunkLine*;

chunkLine: macdef | otherLine | emptyLine;

macdef: MPS_WS* MacroDefine MPS_WS+ NAME LPAREN MPS_WS* parlist MPS_WS* RPAREN macroSubstitutionText? NL;

otherLine: subPart NL;
emptyLine: NL;

parlist
: namelist (COMMA MPS_WS* dots)? | dots
;

dots: DOTS;
    
namelist: NAME MPS_WS* (COMMA MPS_WS* NAME)*;

subPart: NonNewLineChar+;

macroSubstitutionText: MSTS_AnythingElse+;

