using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MiranaCompiler.Tree
{
    static class PrintingExtensions
    {
        public static string ConcatStringList(this IEnumerable<SyntaxTree> trees, string s = ", ")
        {
            return string.Concat(trees.Select(t => t.GetLiteralRepresentation()), s);
        }
    }
}
