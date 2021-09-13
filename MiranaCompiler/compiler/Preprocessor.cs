using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using MiranaCompiler.grammar;


namespace MiranaCompiler
{
    internal class Preprocessor
    {
        private readonly CompileUnit compileUnit;
        public Preprocessor(CompileUnit compileUnit)
        {
            this.compileUnit = compileUnit;
        }
        
        public string Process(string text)
        {
            string mergeText = LineMerger.MergeLine(text);
            MiranaPrepLexer lexer = new(new AntlrInputStream(mergeText));
            MiranaPrepParser parser = new(new CommonTokenStream(lexer));
            parser.AddErrorListener(new MiranaErrorListener(compileUnit));
            var listener = new MiranaPreprocessorListener(compileUnit);
            new ParseTreeWalker().Walk(listener, parser.chunk());
            var macros = listener.macros;
            string macroRemovedText = listener.PreprocessedText;
            MacroReplacer m = new MacroReplacer(compileUnit);
            string macroReplacedText = m.Replace(in macroRemovedText, macros);
            return macroReplacedText;
        }
    }

    internal class LineMerger
    {
        public static string MergeLine(string text)
        {
            StringBuilder sb = new();
            int emptyLineAfter = 0;
            var b = text.Split('\n');
            foreach (string thisLine in b) {
                if (thisLine.Length > 0 && thisLine[^1] == '\\') {
                    sb.Append(thisLine[..^2]);
                    if (sb[^1].IsIdentifierLetter())
                        sb.Append(' ');
                    ++emptyLineAfter;
                }
                else {
                    Enumerable.Range(0, emptyLineAfter).ForEach(t => sb.AppendLine());
                    emptyLineAfter = 0;
                    sb.AppendLine(thisLine);
                }
            }
            Enumerable.Range(0, emptyLineAfter).ForEach(t => sb.AppendLine());
            return sb.ToString();
        }
    }

    internal class MacroReplacer
    {
        private class MatchArgumentsMachine
        {
            private int paramLevel;
            public List<string> arguments = new();
            private int state;
            public bool Success { get; private set; }
            public int EndIndex { get; private set; }
            
            public void FindArguments(in string s)
            {
                Success = true;
                int lastIndex = 0;
                for (int i=0; i<s.Length; ++i) {
                    char k = s[i];
                    if (Char.IsWhiteSpace(k))
                        continue;
                    switch (state) {
                        case 0:
                            if (k == '(') {
                                ++paramLevel;
                                state = 1;
                                lastIndex = 1;
                            }
                            else {
                                goto failure;
                            }
                            break;
                        case 1:
                            if (k == '(') {
                                ++paramLevel;
                            }
                            else if (k == ',') {
                                if (paramLevel == 1) {
                                    arguments.Add(s[lastIndex..i]);
                                    lastIndex = i + 1;
                                }
                            }
                            else if (k == ')') {
                                --paramLevel;
                                if (paramLevel == 0) {
                                    arguments.Add(s[lastIndex..i]);
                                    EndIndex = i;
                                    return;
                                }
                            }
                            break;
                    }
                }
                failure: Success = false;
            }
            
        }

        private readonly CompileUnit compileUnit;
        public MacroReplacer(CompileUnit compileUnit)
        {
            this.compileUnit = compileUnit;
        }
        
        public string Replace(in string originalText, List<MacroFunction> macroFunctions)
        {
            foreach (var m in macroFunctions.GroupBy(t=>t.Name)) {
                var macros = m.ToArray();
                var match = Regex.Match(originalText, @$"\b{macros[0].Name}\b");
                if (match.Success) {
                    int index = match.Index + macros[0].Name.Length;
                    string searchArgumentText = originalText[index..];
                    var f = new MatchArgumentsMachine();
                    f.FindArguments(in searchArgumentText);
                    if (f.Success) {
                        var candidate = macros.FirstOrDefault(t => t.ParamList.Length == f.arguments.Count)??macros.Where(t => t.ParamList.Length < f.arguments.Count && t.HasDots).OrderByDescending(t => t.ParamList.Length).FirstOrDefault();
                        if (candidate == null) {
                            compileUnit.AddError($"Error Mirana 0004 at index {match.Index}: match macro {macros[0].Name} with {f.arguments.Count} argument(s) failed");
                            return originalText;
                        }
                        else {
                            string a = originalText[..match.Index] + candidate.Replace(f.arguments.ToArray()) + originalText[(index+f.EndIndex+1)..];
                            return Replace(in a, macroFunctions);
                        }
                    }
                    else {
                        compileUnit.AddError($"Error Mirana 0004 at index {match.Index}: match macro {macros[0].Name} failed");
                        return originalText;
                    }
                }
            }
            return originalText[0] == ' ' ? originalText[1..] : originalText;
        }
    }

    internal abstract record Macro(string Name)
    {
        
    }

    internal record MacroFunction(string Name, string[] ParamList, bool HasDots, string SubstitutionText): Macro(Name)
    {
        private abstract record Fragment
        {
            public abstract string Convert(string[] s);
        }

        private record NormalStringFragment(string Text) : Fragment
        {
            public override string Convert(string[] s)
            {
                return Text;
            }
        }

        private record ArgumentFragment(int ArgumentIndex): Fragment
        {
            public override string Convert(string[] s)
            {
                return s[ArgumentIndex];
            }
        }

        private record StringificationFragment(int ArgumentIndex, int Sides) : Fragment
        {
            public override string Convert(string[] s)
            {
                string a = s[ArgumentIndex].Replace(@"""", @"\""");
                string b = $"\"{a}\"";
                if ((Sides & 1) != 0) {
                    b = $"..{b}";
                }
                if ((Sides & 2) != 0) {
                    b = $"{b}..";
                }
                return b;
            }
        }

        private record ConcatenationFragment(int ArgumentIndex) : Fragment
        {
            public override string Convert(string[] s)
            {
                return s[ArgumentIndex];
            }
        }

        private record VarArgsFragment(bool Stringification, int Sides) : Fragment
        {
            public override string Convert(string[] s)
            {
                throw new NotImplementedException();
            }

            public string Convert(string[] s, int matchFrom)
            {
                string b = string.Join(", ", s[matchFrom..]);
                if (Stringification) {
                    b = b.Replace(@"""", @"\""");
                    b = $"\"{b}\"";
                }
                if ((Sides & 1) != 0) {
                    b = $"..{b}";
                }
                if ((Sides & 2) != 0) {
                    b = $"{b}..";
                }
                return b;
            }
        }

        private readonly List<Fragment> fragments = new();

        public bool Conflict(MacroFunction t)
        {
            return Name == t.Name && ParamList.Length == t.ParamList.Length && HasDots == t.HasDots;
        }

        private bool IsConcatenation(int start, int end)
        {
            return start >= 2 && SubstitutionText[start - 2] is '#' && SubstitutionText[start - 1] is '#' || end < SubstitutionText.Length - 2 && SubstitutionText.Substring(end+1, 2) is "##";
        }
        private bool IsStringification(int start, int end)
        {
            return (start >= 1 && SubstitutionText[start - 1] is '#' || end < SubstitutionText.Length - 1 && SubstitutionText[end + 1] is '#') && !IsConcatenation(start, end);
        }
        private int GetStringificationSide(int start, int end)
        {
            int p = (start >= 1 && SubstitutionText[start - 1] is '#') ? 1 : 0;
            int q = end < SubstitutionText.Length - 1 && SubstitutionText[end + 1] is '#' ? 2 : 0;
            return p & q;
        }
        public void Build()
        {
            StringBuilder pattern = new("(\\b(");
            pattern.Append(string.Join("|", ParamList));
            if (HasDots)
                pattern.Append("|__VA_ARGS__");
            pattern.Append(")\\b)");
            Regex a = new Regex(pattern.ToString());
            var matchCollection = a.Matches(SubstitutionText);
            int lastMatchIndex = 0;
            foreach (Match match in matchCollection) {
                var matchText = match.Groups[2];
                int matchLength = matchText.Length;
                int startIndex = match.Index + 1;
                int endIndex = startIndex + matchLength - 2;
                string k = SubstitutionText[lastMatchIndex..(startIndex-1)].Replace("#", "");
                if (k != string.Empty) {
                    fragments.Add(new NormalStringFragment(k));
                }
                lastMatchIndex = endIndex+1;
                int argumentIndex = Array.IndexOf(ParamList, matchText.Value);
                --startIndex;
                if (argumentIndex == -1) {
                    bool stringification = IsStringification(startIndex, endIndex);
                    fragments.Add(new VarArgsFragment(stringification, stringification?GetStringificationSide(startIndex, endIndex):0));
                }
                else if (IsConcatenation(startIndex, endIndex)) 
                    fragments.Add(new ConcatenationFragment(argumentIndex));
                else if (IsStringification(startIndex, endIndex))
                    fragments.Add(new StringificationFragment(argumentIndex, GetStringificationSide(startIndex, endIndex)));
                else {
                    fragments.Add(new ArgumentFragment(argumentIndex));
                }
            }
            {
                string k = SubstitutionText[lastMatchIndex..].Replace("#", "");
                if (k != string.Empty) {
                    fragments.Add(new NormalStringFragment(k));
                }
            }
        }

        public string Replace(string[] arguments)
        {
            StringBuilder sb = new();
            foreach (var fragment in fragments) {
                if (fragment is VarArgsFragment varArgsFragment) {
                    sb.Append(varArgsFragment.Convert(arguments, ParamList.Length));
                }
                else {
                    sb.Append(fragment.Convert(arguments));
                }
            }
            return sb.ToString();
        }
    }


    internal class MiranaPreprocessorListener : MiranaPrepParserBaseListener
    {
        private readonly CompileUnit compileUnit;

        public MiranaPreprocessorListener(CompileUnit compileUnit)
        {
            this.compileUnit = compileUnit;
            allText = compileUnit.AllText;
        }
        
        public readonly List<MacroFunction> macros = new();
        private readonly StringBuilder sb = new();
        public string PreprocessedText => sb.ToString();
        private string allText;

        public override void EnterMacdef(MiranaPrepParser.MacdefContext context)
        {
            string macroName = context.NAME().GetText();
            var paramList = context.parlist().namelist()?.NAME().Select(t => t.GetText()).ToArray()??Array.Empty<string>();
            bool dotsPresent = context.parlist().dots() != null;
            string m = context.macroSubstitutionText()?.GetText()??" ";
            if (m[0].IsIdentifierLetter())
                m = " " + m;
            if (m[^1].IsIdentifierLetter())
                m += " ";
            var macro = new MacroFunction(macroName, paramList, dotsPresent, m);
            macro.Build();
            if (macros.Any(t => t.Conflict(macro))) {
                compileUnit.AddError(context, 2, $"duplicate macro definition {macroName}");
            }
            else
                macros.Add(macro);
            sb.Append(context.NL().GetText());
        }

        public override void EnterOtherLine(MiranaPrepParser.OtherLineContext context)
        {
            sb.Append(context.GetText());
        }

        public override void EnterEmptyLine(MiranaPrepParser.EmptyLineContext context)
        {
            sb.Append(context.GetText());
        }
    }
    
}
