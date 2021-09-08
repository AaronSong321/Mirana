using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using MiranaCompiler.grammar;


namespace MiranaCompiler
{
    internal class TypeChecker
    {
        private readonly CompileUnit compileUnit;
        public TypeChecker(CompileUnit compileUnit)
        {
            this.compileUnit = compileUnit;
        }

        public void Check(string text)
        {
            
        }
    }

    internal class TypeCheckListener : miranaBaseListener
    {
        
    }

    internal abstract class MiranaType
    {
        public abstract override string ToString();
        public override bool Equals(object? obj)
        {
            if (this is StringType && obj is StringType)
                return true;
            if (this is IntType && obj is IntType)
                return true;
            if (this is FloatType && obj is FloatType)
                return true;
            if (this is ArrayType t1 && obj is ArrayType t2)
                return t1.Equals(t2);
            if (this is UnionType t3 && obj is UnionType t4)
                return t3.Equals(t4);
            if (this is NilType && obj is NilType)
                return true;
            return false;
        }
        public abstract override int GetHashCode();
    }

    internal sealed class StringType : MiranaType
    {
        public override string ToString()
        {
            return "string";
        }
        public override int GetHashCode()
        {
            return "string".GetHashCode();
        }
    }

    internal sealed class IntType : MiranaType
    {
        public override string ToString()
        {
            return "int";
        }
        public override int GetHashCode()
        {
            return 0.GetHashCode();
        }
    }

    internal sealed class FloatType : MiranaType
    {
        public override string ToString()
        {
            return "float";
        }
        public override int GetHashCode()
        {
            return 0f.GetHashCode();
        }
    }

    internal sealed class ArrayType : MiranaType
    {
        public MiranaType UnderlyingType { get; }
        public ArrayType(MiranaType underlyingType)
        {
            UnderlyingType = underlyingType;
        }
        public override string ToString()
        {
            return $"{UnderlyingType}[]";
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(UnderlyingType, "[]");
        }
        public override bool Equals(object? obj)
        {
            return obj is ArrayType o && UnderlyingType.Equals(o.UnderlyingType);
        }
    }

    internal sealed class NilType : MiranaType
    {
        public override string ToString()
        {
            return $"nil";
        }
        public override int GetHashCode()
        {
            return "nil".GetHashCode();
        }
    }

    internal sealed class UnionType : MiranaType
    {
        public bool IsNullable { get; }
        public MiranaType[] TypeCollection { get; }
        private UnionType(MiranaType[] types, bool isNullable)
        {
            TypeCollection = types;
            IsNullable = isNullable;
        }

        public static MiranaType Create(params MiranaType[] types)
        {
            var a = types.Distinct().ToArray();
            bool isNullable = a.Any(t => t is NilType);
            var b = a.Where(t => t is not NilType).ToArray();
            if (b.Length == 0) {
                if (isNullable) {
                    return new NilType();
                }
                throw new();
            }
            if (b.Length == 1) {
                if (!isNullable) {
                    return b[0];
                }
            }
            return new UnionType(b, isNullable);
        }
        public static MiranaType Create(MiranaType t1, MiranaType t2) => Create(new[] { t1, t2 });
        public static UnionType CreateNullableType(MiranaType a)
        {
            return a switch {
                NilType => throw new(),
                UnionType u => new(u.TypeCollection, true),
                _ => new(new[] { a }, true)
            };
        }
        
        public override string ToString()
        {
            string a = string.Join("|", TypeCollection.Select(t => t.ToString()));
            if (TypeCollection.Length == 1) {
                if (IsNullable) {
                    a = a + "?";
                }
            }
            else {
                if (IsNullable) {
                    a = $"({a})?";
                }
            }
            return a;
        }

        private sealed class MiranaTypeComparer : IEqualityComparer<MiranaType>
        {
            private MiranaTypeComparer() { }
            public static MiranaTypeComparer Instance { get; } = new();
            public bool Equals(MiranaType? x, MiranaType? y)
            {
                return x != null && x.Equals(y);
            }
            public int GetHashCode(MiranaType obj)
            {
                return obj.GetHashCode();
            }
        }

        public override bool Equals(object? obj)
        {
            return obj is UnionType o && (TypeCollection.Length == o.TypeCollection.Length && TypeCollection.Intersect(o.TypeCollection, MiranaTypeComparer.Instance).Count() == TypeCollection.Length && IsNullable == o.IsNullable);
        }

        public override int GetHashCode()
        {
            HashCode a = new();
            if (IsNullable)
                a.Add("nil");
            TypeCollection.ForEach(a.Add);
            return a.ToHashCode();
        }
    }
}
