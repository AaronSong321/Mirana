#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Sharpen;
using Antlr4.Runtime.Tree;
using Common;
using MiranaCompiler.grammar;


namespace MiranaCompiler
{
    internal class LuaWriter
    {
        private readonly StreamWriter writer;
        private int indent;
        private readonly CompileUnit compileUnit;
        
        public LuaWriter(StreamWriter writer, MiranaToLuaWriter miranaToLuaListener, CompileUnit compileUnit)
        {
            this.writer = writer;
            this.compileUnit = compileUnit;
        }

        public void EnterChunk()
        {
            writer.Write(@$"---------------------------------------------
-- Generated from Mirana Compiler version {Compiler.Version}
-- Do not modify
-- https://github.com/AaronSong321/Mirana
---------------------------------------------
");
        }
        public void ExitChunk()
        {
            if (!firstStatementInChunk)
                writer.WriteLine();
        }
        
        private void WriteParamList(miranaParser.ParlistContext context)
        {
            writer.Write('(');
            if (context.namelist() != null) {
                WriteNameList(context.namelist());
                if (context.dots() != null) {
                    writer.Write(", ...");
                }
            }
            else {
                if (context.dots() != null) {
                    writer.Write("...");
                }
            }
            writer.Write(')');
        }

        private void WriteNameList(miranaParser.NamelistContext context)
        {
            var a = context.NAME();
            if (a.Length == 1) {
                writer.Write(a[0]);
            }
            else {
                writer.Write(a[0]);
                foreach (var b in a.Skip(1)) {
                    writer.Write($", {b.GetText()}");
                }
            }
        }

        private void WriteIndent()
        {
            for (int i=0;i<indent;++i)
                writer.Write("    ");
        }

        private void WriteEnd()
        {
            writer.WriteLine();
            --indent;
            WriteIndent();
            writer.Write("end");
        }

        private void WriteLine(string s)
        {
            
        }

        private void WriteVarSuffix(miranaParser.VarSuffixContext context)
        {
            WriteVarSuffixCalls(context);
            WriteVarSuffixIndex(context);
        }

        private void WriteVarSuffixCalls(miranaParser.VarSuffixContext context)
        {
            var nameAndArgs = context.nameAndArgs();
            foreach (var n in nameAndArgs) {
                WriteNameAndArgs(n);
            }
        }
        private void WriteVarSuffixIndex(miranaParser.VarSuffixContext context)
        {
            if (context.exp() != null) {
                writer.Write('[');
                WriteExp(context.exp());
                writer.Write(']');
            }
            else {
                writer.Write('.');
                writer.Write(context.NAME().GetText());
            }
        }

        private void WriteNameAndArgs(miranaParser.NameAndArgsContext context)
        {
            if (context.NAME() != null) {
                writer.Write($":{context.NAME().GetText()}");
            }
            WriteArgs(context.args());
        }

        private void WriteArgs(miranaParser.ArgsContext context)
        {
            if (context.kotLambda() != null && context.explist() != null) {
                writer.Write('(');
                WriteExp(context.explist());
                writer.Write(", ");
                WriteExp(context.kotLambda());
                writer.Write(')');
            }
            else if (context.kotLambda() != null) {
                writer.Write('(');
                WriteExp(context.kotLambda());
                writer.Write(')');
            }
            else if (context.explist() != null) {
                writer.Write('(');
                WriteExp(context.explist());
                writer.Write(')');
            }
            else if (context.tableconstructor() != null) {
                writer.Write(" ");
                WriteTable(context.tableconstructor());
            }
            else if (context.@string() != null) {
                writer.Write(" ");
                CopySource(context.@string());
            }
            else {
                writer.Write("()");
            }
        }
        
        public void WriteBlock(miranaParser.BlockContext context)
        {
            foreach (var statement in context.stat())
            {
                WriteStat(statement);
            }
            if (context.retstat() != null) {
                WriteStat(context.retstat());
            }
        }

        private void WriteOperatorUnary(miranaParser.OperatorUnaryContext context)
        {
            writer.Write(context.GetText());
            if (context.GetText() == "not")
                writer.Write(" ");
        }

        private int tempLocalVarIndex = 0;
        private string GetTempLocalVarName(int i) => $"__mira_locvar_{i}";
        private string NewTempLocalVar() => GetTempLocalVarName(++tempLocalVarIndex);
        
        private void WriteOperatorLambda(miranaParser.OperatorLambdaContext context)
        {
            var op = context.opcom();
            writer.Write("function(");
            var par1 = GetOperatorLambdaParamName(1);
            var par2 = GetOperatorLambdaParamName(2);
            if (op.operatorUnary() != null) {
                writer.Write($"{par1}");
                writer.Write(") return ");
                WriteOperatorUnary(op.operatorUnary());
                writer.Write(par1);
                writer.Write(" end");
            }
            else if (op.opda() != null) {
                writer.Write(par1);
                writer.Write(")");
                var a = op.opda();
                if (a.OpAddOne() != null) {
                    writer.Write($" {par1} = {par1} + 1; return {par1}");
                }
                else if (a.OpSubOne() != null) {
                    writer.Write($" {par1} = {par1} - 1; return {par1}");
                }
                writer.Write(" end");
            }
            else {
                writer.Write($"{par1}, {par2}) return {par1}");
                writer.Write(op.operatorStrcat() != null ? ".." : $" {op.GetText()} ");
                writer.Write($"{par2} end");
            }
        }

        private void WriteExp(miranaParser.ExpContext context)
        {
            while (true) {
                if (context.prefixexp() != null) {
                    WritePrefixExp(context.prefixexp());
                }
                else if (context.operatorUnary() != null) {
                    WriteOperatorUnary(context.operatorUnary());
                    context = context.exp(0);
                    continue;
                }
                else if (context.number() != null || context.@string() != null || context.dots() != null) {
                    CopySource(context);
                }
                else if (context.ifexp() != null) {
                    WriteExp(context.ifexp());
                }
                else if (context.tableconstructor() != null) {
                    WriteTable(context.tableconstructor());
                }
                else if (context.kotLambda() != null) {
                    WriteExp(context.kotLambda());
                }
                else if (context.funcLiteral() != null) {
                    WriteFunctionDef(context.funcLiteral());
                }
                else if (context.operatorLambda() != null) {
                    WriteOperatorLambda(context.operatorLambda());
                }
                else if (context.operatorAnd() != null) {
                    WriteExp(context.exp(0));
                    writer.Write($" {context.operatorAnd().GetText()} ");
                    context = context.exp(1);
                    continue;
                }
                else if (context.operatorComparison() != null) {
                    WriteExp(context.exp(0));
                    writer.Write($" {context.operatorComparison().GetText()} ");
                    context = context.exp(1);
                    continue;
                }
                else if (context.operatorOr() != null) {
                    WriteExp(context.exp(0));
                    writer.Write($" {context.operatorOr().GetText()} ");
                    context = context.exp(1);
                    continue;
                }
                else if (context.operatorPower() != null) {
                    WriteExp(context.exp(0));
                    writer.Write($" {context.operatorPower().GetText()} ");
                    context = context.exp(1);
                    continue;
                }
                else if (context.operatorAddSub() != null) {
                    WriteExp(context.exp(0));
                    writer.Write($" {context.operatorAddSub().GetText()} ");
                    context = context.exp(1);
                    continue;
                }
                else if (context.operatorMulDivMod() != null) {
                    WriteExp(context.exp(0));
                    writer.Write($" {context.operatorMulDivMod().GetText()} ");
                    context = context.exp(1);
                    continue;
                }
                else if (context.operatorStrcat() != null) {
                    WriteExp(context.exp(0));
                    writer.Write($"{context.operatorStrcat().GetText()}");
                    context = context.exp(1);
                    continue;
                }
                else if (context.OpAmpersand() != null) {
                    WriteExp(context.exp(0));
                    CopySource(context.OpAmpersand());
                    WriteExp(context.exp(1));
                }
                else if (context.OpPipe() != null) {
                    WriteExp(context.exp(0));
                    CopySource(context.OpPipe());
                    WriteExp(context.exp(1));
                }
                else if (context.OpTilde() != null) {
                    WriteExp(context.exp(0));
                    CopySource(context.OpTilde());
                    WriteExp(context.exp(1));
                }
                else if (context.OpShl() != null) {
                    WriteExp(context.exp(0));
                    CopySource(context.OpShl());
                    WriteExp(context.exp(1));
                }
                else if (context.OpShr() != null) {
                    WriteExp(context.exp(0));
                    CopySource(context.OpShr());
                    WriteExp(context.exp(1));
                }
                else {
                    CopySource(context);
                }
                break;
            }
        }

        private void WriteExp(miranaParser.KotLambdaContext context)
        {
            writer.Write("function");
            if (context.parlist() != null) {
                WriteParamList(context.parlist());
            }
            else {
                writer.Write("()");
            }
            ++indent;
            WriteBlock(context.expBlock());
            WriteEnd();
        }

        private void WriteExp(miranaParser.IfexpContext context)
        {
            writer.WriteLine("(function()");
            ++indent;
            WriteExp(context.ifexp_if());
            context.ifexp_elif().ForEach(WriteExp);
            if (context.ifexp_else() != null) {
                WriteExp(context.ifexp_else());
            }
            int writeEnds = context.ifexp_elif().Count(t => t.predicateexp().assignableExp() != null) + 2;
            Enumerable.Range(0, writeEnds).ForEach(_ => WriteEnd());
            writer.Write(")()");
        }

        private void WritePredicateExp_AssignmentPart(miranaParser.PredicateexpContext context) 
        {
            writer.Write("local ");
            WriteVar_(context.assignableExp());
            writer.Write(" = ");
            WriteExp(context.exp(0));
            writer.WriteLine();
            WriteIndent();
            writer.Write("if ");
            if (context.exp().Length is 1)
                WriteVar_(context.assignableExp());
            else
                WriteExp(context.exp(1));
            writer.Write(" then");
        }

        private void WriteExp(miranaParser.Ifexp_elifContext context)
        {
            var predicate = context.predicateexp();
            WriteIndent();
            if (predicate.assignableExp() != null) {
                writer.WriteLine("else");
                ++indent;
                WriteIndent();
                WritePredicateExp_AssignmentPart(predicate);
            }
            else {
                writer.Write("elseif ");
                WriteExp(predicate.exp(0));
                writer.Write(" then");
            }
            ++indent;
            WriteBlock(context.expBlock());
            --indent;
            writer.WriteLine();
        }

        private void WriteExp(miranaParser.Ifexp_ifContext context)
        {
            var predicate = context.predicateexp();
            WriteIndent();
            if (predicate.assignableExp() != null) {
                WritePredicateExp_AssignmentPart(predicate);
            }
            else {
                writer.Write("if ");
                WriteExp(predicate.exp(0));
                writer.Write(" then");
            }
            ++indent;
            WriteBlock(context.expBlock());
            --indent;
            writer.WriteLine();
        }

        private void WriteExp(miranaParser.Ifexp_elseContext context)
        {
            WriteIndent();
            writer.Write("else");
            ++indent;
            WriteBlock(context.expBlock());
        }

        private void WriteTable(miranaParser.TableconstructorContext context)
        {
            writer.Write('{');
            if (context.fieldlist() != null) {
                WriteFieldList(context.fieldlist());
            }
            writer.Write('}');
        }

        private void WriteFieldList(miranaParser.FieldlistContext context)
        {
            var fields = context.field();
            if (fields.Length == 1) {
                writer.Write(' ');
                WriteField(fields[0]);
                writer.Write(' ');
            }
            else {
                writer.WriteLine();
                ++indent;
                foreach (var field in fields) {
                    WriteIndent();
                    WriteField(field);
                    writer.WriteLine(",");
                }
                --indent;
                WriteIndent();
            }
        }

        private void WriteField(miranaParser.FieldContext context)
        {
            var exps = context.exp();
            if (exps.Length > 1) {
                writer.Write('[');
                WriteExp(exps[0]);
                writer.Write("] = ");
                WriteExp(exps[1]);
            }
            else if (context.NAME() != null) {
                writer.Write(context.NAME().GetText());
                writer.Write(" = ");
                WriteExp(exps[0]);
            }
            else {
                WriteExp(exps[0]);
            }
        }

        private void WriteFunctionDef(miranaParser.FuncLiteralContext context)
        {
            writer.Write("function");
            WriteFuncBody(context.funcbody());
        }

        private void WriteFuncBody(miranaParser.FuncbodyContext context)
        {
            if (context.parlist() != null)
                WriteParamList(context.parlist());
            else 
                writer.Write("()");
            if (context.block() is not null){
                indent++;
                WriteBlock(context.block());
                WriteEnd();
            }
            else if (context.exp().NIL() != null) {
                writer.Write(" end");
            }
            else {
                writer.WriteLine();
                indent++;
                WriteIndent();
                writer.Write("return ");
                WriteExp(context.exp());
                WriteEnd();
            }
        }

        private void WritePrefixExp(miranaParser.PrefixexpContext context)
        {
            WriteVarOrExp(context.varOrExp());
            foreach (var n in context.nameAndArgs()) {
                WriteNameAndArgs(n);
            }
        }

        private void WriteVarOrExp(miranaParser.VarOrExpContext context)
        {
            if (context.assignableExp() != null) {
                WriteVar_(context.assignableExp());
            }
            else {
                writer.Write("(");
                WriteExp(context.exp());
                writer.Write(")");
            }
        }

        private void WriteVar_(miranaParser.AssignableExpContext context)
        {
            var suffixes = context.varSuffix();
            if (context.NAME() != null) {
                writer.Write(context.NAME().GetText());
            }
            else if (context.lambdaImplicitParam() != null) {
                writer.Write(GetLambdaImplicitParamName(context.lambdaImplicitParam().Num));
            }
            else if (context.it() != null) {
                writer.Write(GetLambdaImplicitIterName());
            }
            else {
                writer.Write('(');
                WriteExp(context.exp());
                writer.Write(')');
            }
            foreach (var suffix in suffixes) {
                WriteVarSuffix(suffix);
            }
        }
        
        private void WriteVar_NoLast(miranaParser.AssignableExpContext context)
        {
            var suffixes = context.varSuffix();
            if (context.NAME() != null) {
                writer.Write(context.NAME().GetText());
            }
            else {
                writer.Write('(');
                WriteExp(context.exp());
                writer.Write(')');
            }
            foreach (var suffix in suffixes.SkipLast(1)) {
                WriteVarSuffix(suffix);
            }
        }

        public static string GetLambdaImplicitParamName(int index)
        {
            return $"_i_{index}";
        }
        public static string GetLambdaImplicitIterName() => "it";
        private static string GetOperatorLambdaParamName(int index) => $"opv_{index}";

        private void AddError(ParserRuleContext context, int code, string message)
        {
            compileUnit.AddError($"Error Mira{code} ({context.start.Line}, {context.start.Column}: {message}");
        }

        private void WriteBlock(miranaParser.ExpBlockContext context)
        {
            if (context.retstat() is null && context.explist() is null) {
                var statements = context.stat();
                if (statements.Length is not 0) {
                    if (statements[^1].stat_funcall() != null) {
                        statements.SkipLast(1).ForEach(WriteStat);
                        writer.WriteLine();
                        WriteIndent();
                        writer.Write("return ");
                        WriteStat(statements[^1].stat_funcall());
                    }
                    else {
                        statements.ForEach(WriteStat);
                    }
                }
            }
            else {
                context.stat().ForEach(WriteStat);
                if (context.retstat() != null) {
                    WriteStat(context.retstat());
                }
                else if (context.explist() != null) {
                    writer.WriteLine();
                    WriteIndent();
                    writer.Write("return");
                    if (context.explist() != null) {
                        writer.Write(' ');
                        WriteExp(context.explist());
                    }
                }
            }
        }

        private void CopySource(IParseTree context)
        {
            writer.Write(context.GetText());
        }

        private void WriteExp(miranaParser.ExplistContext context)
        {
            var exps = context.exp();
            WriteExp(exps[0]);
            for (int i = 1; i < exps.Length; ++i) {
                writer.Write(", ");
                WriteExp(exps[i]);
            }
        }

        private void WriteStat(miranaParser.Stat_doContext context)
        {
            writer.Write("do");
            indent++;
            WriteBlock(context.block());
            WriteEnd();
        }

        private bool firstStatementInChunk = true;
        private void WriteStat(miranaParser.StatContext context)
        {
            if (context.stat_semicolon() != null) {
                return;
            }
            if (firstStatementInChunk) {
                firstStatementInChunk = false;
            }
            else {
                writer.WriteLine();
            }
            WriteIndent();
            if (context.stat_assign() != null) {
                WriteStat(context.stat_assign());
            }
            else if (context.stat_cassign() != null) {
                WriteStat(context.stat_cassign());
            }
            else if (context.stat_break() != null) {
                WriteStat(context.stat_break());
            }
            else if (context.stat_do() != null) {
                WriteStat(context.stat_do());
            }
            else if (context.stat_foreach() != null) {
                WriteStat(context.stat_foreach());
            }
            else if (context.stat_fori() != null) {
                WriteStat(context.stat_fori());
            }
            else if (context.stat_funcall() != null) {
                WriteStat(context.stat_funcall());
            }
            else if (context.stat_fundecl() != null) {
                WriteStat(context.stat_fundecl());
            }
            else if (context.stat_goto() != null) {
                WriteStat(context.stat_goto());
            }
            else if (context.stat_if() != null) {
                WriteStat(context.stat_if());
            }
            else if (context.stat_label() != null) {
                WriteStat(context.stat_label());
            }
            else if (context.stat_localdecl() != null) {
                WriteStat(context.stat_localdecl());
            }
            else if (context.stat_localfundecl() != null) {
                WriteStat(context.stat_localfundecl());
            }
            else if (context.stat_repeat() != null) {
                WriteStat(context.stat_repeat());
            }
            else if (context.stat_while() != null) {
                WriteStat(context.stat_while());
            }
            else if (context.stat_opda() != null) {
                WriteStat(context.stat_opda());
            }
        }

        private static string GetOpdaString(miranaParser.OpdaContext context) => context.OpAddOne() != null ? "+" : "-";
        
        private void WriteStat(miranaParser.Stat_opdaContext context)
        {
            var c = context.assignableExp();
            var varSuffixes = c.varSuffix();
            if (varSuffixes.Length == 0) {
                writer.Write($"{c.NAME().GetText()} = {c.NAME().GetText()} {GetOpdaString(context.opda())} 1");
            }
            else {
                var tempVar = NewTempLocalVar();
                writer.Write($"local {tempVar} = ");
                WriteVar_NoLast(c);
                var lastSuffix = varSuffixes[^1];
                WriteVarSuffixCalls(lastSuffix);
                writer.WriteLine();
                WriteIndent();
                writer.Write($"{tempVar}");
                WriteVarSuffixIndex(lastSuffix);
                writer.Write($" = {tempVar}");
                WriteVarSuffixIndex(lastSuffix);
                writer.Write($" {GetOpdaString(context.opda())} 1");
            }
        }
        private void WriteStat(miranaParser.Stat_localfundeclContext context)
        {
            writer.Write("local function ");
            writer.Write(context.NAME().GetText());
            WriteFuncBody(context.funcbody());
        }

        private void WriteStat(miranaParser.Stat_breakContext context)
        {
            writer.Write("break");
        }

        private void WriteStat(miranaParser.Stat_funcallContext context)
        {
            WriteFunctionCall(context.functioncall());
        }

        private void WriteFunctionCall(miranaParser.FunctioncallContext context)
        {
            WriteVarOrExp(context.varOrExp());
            foreach (var n in context.nameAndArgs()) {
                WriteNameAndArgs(n);
            }
        }

        private void EnterLabel(miranaParser.LabelContext context)
        {
            CopySource(context);
        }
        
        private void WriteStat(miranaParser.Stat_gotoContext context)
        {
            writer.Write("goto ");
            writer.Write(context.NAME().GetText());
        }

        private IEnumerable<miranaParser.PredicateexpContext> GetVarDeclareIfPredicate(miranaParser.Stat_ifContext context)
        {
            if (context.stat_if_if().predicateexp().assignableExp() != null) {
                yield return context.stat_if_if().predicateexp();
            }
            var c = context.stat_if_elseif().Where(t => t.predicateexp().assignableExp() != null).Select(t => t.predicateexp());
            foreach (var d in c)
                yield return d;
        }

        private void WriteStat(miranaParser.Stat_ifContext context)
        {
            var varDeclareIf = GetVarDeclareIfPredicate(context).ToArray();
            WriteStat(context.stat_if_if());
            foreach (var e in context.stat_if_elseif()) {
                WriteStat(e);
            }
            if (context.stat_if_else() != null) {
                WriteStat(context.stat_if_else());
            }
            Enumerable.Range(0, varDeclareIf.Length+1).ForEach(_ => WriteEnd());
        }

        private static miranaParser.ExpContext? GetSingleParenthesisedExpressionIfPossible(miranaParser.ExpContext context)
        {
            if (context.prefixexp() == null)
                return null;
            var a = context.prefixexp()!;
            if (a.nameAndArgs().Length != 0)
                return null;
            var b = a.varOrExp();
            return b.exp();
        }

        private static miranaParser.ExpContext GetSingleParenthesisedExpressionOrThis(miranaParser.ExpContext context) {
            return GetSingleParenthesisedExpressionIfPossible(context)??context;
        }

        private void WriteStat(miranaParser.Stat_if_ifContext context)
        {
            var predicate = context.predicateexp();
            bool hasAssignment = predicate.assignableExp() != null;
            if (hasAssignment) {
                writer.WriteLine("do");
                ++indent;
                WriteIndent();
                WritePredicateExp_AssignmentPart(predicate);
            }
            else {
                writer.Write("if ");
                WriteExp(GetSingleParenthesisedExpressionOrThis(predicate.exp(0)));
                writer.Write(" then");
            }
            indent++;
            WriteBlock(context.block());
        }

        private void WriteStat(miranaParser.Stat_if_elseifContext context)
        {
            --indent;
            writer.WriteLine();
            WriteIndent();
            var predicate = context.predicateexp();
            bool hasAssignment = predicate.assignableExp() != null;
            if (hasAssignment) {
                writer.WriteLine("else");
                ++indent;
                WriteIndent();
                WritePredicateExp_AssignmentPart(predicate);
            }
            else {
                writer.Write("elseif ");
                WriteExp(GetSingleParenthesisedExpressionOrThis(predicate.exp(0)));
                writer.Write(" then");
            }
            indent++;
            WriteBlock(context.block());
        }

        private void WriteStat(miranaParser.Stat_if_elseContext context)
        {
            --indent;
            writer.WriteLine();
            WriteIndent();
            writer.Write("else");
            indent++;
            WriteBlock(context.block());
        }

        private void WriteStat(miranaParser.Stat_assignContext context)
        {
            WriteVarList(context.varlist());
            writer.Write(" = ");
            WriteExp(context.explist());
        }

        private static string GetOpdaString(string context) => context switch {
            "+=" => "+",
            "-=" => "-",
            "*=" => "*",
            _ => throw new()
        };
        private void WriteStat(miranaParser.Stat_cassignContext context)
        {
            var c = context.assignableExp();
            var varSuffixes = c.varSuffix();
            if (varSuffixes.Length == 0) {
                writer.Write($"{c.NAME().GetText()} = {c.NAME().GetText()} {GetOpdaString(context.OpCassign().GetText())} ");
            }
            else {
                string tempVar = NewTempLocalVar();
                writer.Write($"local {tempVar} = ");
                WriteVar_NoLast(c);
                var lastSuffix = varSuffixes[^1];
                WriteVarSuffixCalls(lastSuffix);
                writer.WriteLine();
                WriteIndent();
                writer.Write($"{tempVar}");
                WriteVarSuffixIndex(lastSuffix);
                writer.Write($" = {tempVar}");
                WriteVarSuffixIndex(lastSuffix);
                writer.Write($" {GetOpdaString(context.OpCassign().GetText())} ");
            }
            WriteExp(context.exp());
        }

        void WriteVarList(miranaParser.VarlistContext context)
        {
            var vars = context.assignableExp();
            WriteVar_(vars[0]);
            for (int i = 1; i < vars.Length; ++i) {
                writer.Write(", ");
                WriteVar_(vars[i]);
            }
        }

        private void WriteStat(miranaParser.Stat_foreachContext context)
        {
            writer.Write("for ");
            WriteNameList(context.namelist());
            writer.Write(" in ");
            WriteExp(context.explist());
            writer.Write(" ");
            WriteStat(context.stat_do());
        }

        private void WriteStat(miranaParser.Stat_foriContext context)
        {
            writer.Write("for ");
            writer.Write(context.NAME().GetText());
            writer.Write(" = ");
            var expList = context.exp();
            for (int i=0;i<expList.Length;++i) {
                if (i != 0)
                    writer.Write(", ");
                WriteExp(expList[i]);
            }
            writer.Write(' ');
            WriteStat(context.stat_do());
        }

        private void WriteStat(miranaParser.RetstatContext context)
        {
            writer.WriteLine();
            WriteIndent();
            writer.Write("return");
            if (context.explist() != null) {
                writer.Write(' ');
                WriteExp(context.explist());
            }
        }

        private void WriteStat(miranaParser.Stat_labelContext context)
        {
            EnterLabel(context.label());
        }

        private void WriteStat(miranaParser.Stat_whileContext context)
        {
            writer.Write("while ");
            WriteExp(context.exp());
            writer.Write(" ");
            WriteStat(context.stat_do());
        }

        private void WriteStat(miranaParser.Stat_localdeclContext context)
        {
            writer.Write("local ");
            WritePart(context.attNameList());
            if (context.explist() != null) {
                writer.Write(" = ");
                WriteExp(context.explist());
            }
        }

        private void WritePart(miranaParser.AttNameListContext context) {
            var names = context.attName();
            for (int i = 0; i < names.Length; ++i) {
                CopySource(names[i].NAME());
                if (names[i].attribute() != null) {
                    WritePart(names[i].attribute());
                }
                if (i != names.Length - 1) {
                    writer.Write(", ");
                }
            }
        }

        private void WritePart(miranaParser.AttributeContext context) {
            writer.Write('<');
            CopySource(context.NAME());
            writer.Write('>');
        }

        private void WriteStat(miranaParser.Stat_fundeclContext context)
        {
            writer.Write("function ");
            WriteFuncName(context.funcname());
            WriteFuncBody(context.funcbody());
        }

        private void WriteFuncName(miranaParser.FuncnameContext context)
        {
            writer.Write(context.GetText());
        }

        private void WriteStat(miranaParser.Stat_repeatContext context)
        {
            writer.Write("repeat");
            indent++;
            WriteBlock(context.block());
            indent--;
            writer.WriteLine();
            writer.Write("while ");
            WriteExp(context.exp());
        }
    }

    internal class MiranaToLuaWriter
    {
        private readonly StreamWriter outputFile;
        private readonly LuaWriter writer;
        private readonly CompileUnit compileUnit;
        private readonly miranaParser.ChunkContext context;

        public MiranaToLuaWriter(CompileUnit compileUnit, miranaParser.ChunkContext context)
        {
            this.compileUnit = compileUnit;
            this.context = context;
            outputFile = new(CreateLuaFilePath(compileUnit.FilePath));
            writer = new(outputFile, this, compileUnit);
        }

        private static string CreateLuaFilePath(string mira)
        {
            string path = Path.GetDirectoryName(mira)!;
            string name = Path.GetFileNameWithoutExtension(mira);
            name = Path.Combine(path!, name + ".lua");
            return name;
        }

        public void Compile()
        {
            writer.EnterChunk();
            writer.WriteBlock(context.block());
            writer.ExitChunk();
            outputFile.Dispose();
        }
    }

}
