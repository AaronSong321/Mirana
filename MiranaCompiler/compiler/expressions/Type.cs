
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Common;

namespace MiranaCompiler.Tree
{
    enum MiranaTypeName
    {
        Int,
        Float,
        Bool,
        String,
        Object,
        Any,
        Function, 
        Null,
    }

    enum TargetLanguage
    {
        None,
        Lua,
    }

    static partial class OperatorExtensions
    {
        public static TargetLanguage TargetLanguage { get; set; } = TargetLanguage.Lua;
        
        public static string GetLiteralRepresentation(this MiranaTypeName name)
        {
            if (TargetLanguage == TargetLanguage.Lua)
                return name switch {
                    MiranaTypeName.Int => "number",
                    MiranaTypeName.Float => "number",
                    MiranaTypeName.Function => "function",
                    MiranaTypeName.Null => "nil",
                    MiranaTypeName.String => "string",
                    MiranaTypeName.Object => "object",
                    _ => "unknown type"
                };
            throw new NotImplementedException();
        }

        public static string GetLiteralRepresentation(this LuaObjectTypeName name)
        {
            if (TargetLanguage == TargetLanguage.Lua)
                return name switch {
                    LuaObjectTypeName.Table => "table",
                    LuaObjectTypeName.FullUserdata => "userdata",
                    LuaObjectTypeName.LightUserdata => "userdata",
                    _ => "unknown lua type",
                };
            throw new NotImplementedException();
        }
    }

    abstract class MiranaType : SyntaxTree
    {
        public virtual MiranaType Merge(MiranaType type)
        {
            if (type is UnionType u) {
                return new UnionType(u.GetTypes().Append(this));
            }
            else {
                return new UnionType(new[] { this, type });
            }
        }
        public virtual bool AssignableTo(MiranaType o)
        {
            if (o is ConcreteType c1) {
                if (c1.MiranaTypeName == MiranaTypeName.Any)
                    return true;
            }
            return Equals(o);
        }
        public bool AssignableFrom(MiranaType o)
        {
            return o.AssignableTo(this);
        }

        public override bool Equals(object? obj)
        {
            if (this is FunctionType t1 && obj is FunctionType o1) {
                return t1.Equals(o1);
            }
            if (this is ConcreteLuaObjectType t4 && obj is ConcreteLuaObjectType o4) {
                return t4.Equals(o4);
            }
            if (this is ConcreteType t2 && obj is ConcreteType o2) {
                return t2.Equals(o2);
            }
            if (this is UnionType t3 && obj is UnionType o3) {
                return t3.Equals(o3);
            }
            return ReferenceEquals(this, obj);
        }
        public static bool operator==(MiranaType t1, MiranaType t2)
        {
            return t1.Equals(t2);
        }
        public static bool operator!=(MiranaType t1, MiranaType t2)
        {
            return !(t1 == t2);
        }

        public override int GetHashCode()
        {
            return GetLiteralRepresentation().GetHashCode();
        }
    }

    class FunctionType : MiranaType
    {
        private readonly MiranaType[] parameterTypes;
        private readonly List<MiranaType[]> returnTypeOptions;
        public FunctionType(IEnumerable<MiranaType> parameterTypes, IEnumerable<IEnumerable<MiranaType>> returnTypeOptions)
        {
            this.parameterTypes = parameterTypes.ToArray();
            this.returnTypeOptions = returnTypeOptions.Select(t => t.ToArray()).ToList();
        }
        public FunctionType(IEnumerable<MiranaType> parameterTypes)
        {
            this.parameterTypes = parameterTypes.ToArray();
            returnTypeOptions = new();
        }

        public override string GetLiteralRepresentation()
        {
            static string ToTypeList(IEnumerable<MiranaType> types)
            {
                return $"({types.ConcatStringList()})";
            }
            string p = ToTypeList(parameterTypes);
            string r = string.Concat(returnTypeOptions.Select(ToTypeList), "|");
            return $"{p} -> {r}";
        }

        private static bool ParameterTypesAssignableTo(MiranaType[] from, MiranaType[] to)
        {
            if (from.Length > to.Length)
                return false;
            MiranaType[] argumentTypes = new MiranaType[to.Length];
            for (int i = 0; i < from.Length; ++i)
                argumentTypes[i] = from[i];
            for (int i = from.Length; i < to.Length; ++i)
                argumentTypes[i] = ConcreteType.Null;
            return Enumerable.Range(0, to.Length).All(i => argumentTypes[i].AssignableTo(to[i]));
        }

        private static bool ReturnTypesMatch(List<MiranaType[]> from, List<MiranaType[]> to)
        {
            return from.Match(to, (r1, r2) => r1.Equals(r2));
        }
        public override bool AssignableTo(MiranaType o)
        {
            if (o is ConcreteType c1) {
                if (c1.MiranaTypeName == MiranaTypeName.Function)
                    return true;
            }
            if (o is FunctionType f1) {
                return ParameterTypesAssignableTo(parameterTypes, f1.parameterTypes) && ReturnTypesMatch(returnTypeOptions, f1.returnTypeOptions);
            }
            return base.AssignableTo(o);
        }

        public override bool Equals(object? obj)
        {
            return obj is FunctionType f1 && (parameterTypes.Match(f1.parameterTypes) && ReturnTypesMatch(returnTypeOptions, f1.returnTypeOptions));
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    class ConcreteType : MiranaType
    {
        protected ConcreteType(MiranaTypeName miranaTypeName){
            this.MiranaTypeName = miranaTypeName;
        }

        public MiranaTypeName MiranaTypeName { get; }
        public static ConcreteType Any { get; } = new(MiranaTypeName.Any);
        public static ConcreteType Int { get; } = new(MiranaTypeName.Int);
        public static ConcreteType Float { get; } = new(MiranaTypeName.Float);
        public static ConcreteType Function { get; } = new(MiranaTypeName.Function);
        public static ConcreteType Null { get; } = new(MiranaTypeName.Null);
        public static ConcreteType Bool { get; } = new(MiranaTypeName.Bool);
        public static ConcreteType String { get; } = new(MiranaTypeName.String);

        public override string GetLiteralRepresentation()
        {
            return MiranaTypeName.GetLiteralRepresentation();
        }
        public override bool AssignableTo(MiranaType o)
        {
            if (o is ConcreteType c1) {
                return MiranaTypeName == c1.MiranaTypeName;
            }
            return base.AssignableTo(o);
        }

        public override bool Equals(object? obj)
        {
            return obj is ConcreteType t1 && MiranaTypeName == t1.MiranaTypeName;
        }
        public override int GetHashCode()
        {
            return MiranaTypeName.GetHashCode();
        }
    }

    class UnionType : MiranaType
    {
        private readonly MiranaType[] typeOptions;
        public static MiranaType FromTwoTypes (MiranaType t1, MiranaType t2)
        {
            if (t1 is ErrorType || t2 is ErrorType)
                return ErrorType.Instance;
            if (t1 is UnionType t3) {
                return t3.Merge(t2);
            }
            return new UnionType(new[] { t1 }).Merge(t2);
        }
        public UnionType(IEnumerable<MiranaType> options)
        {
            typeOptions = options.ToArray();
        }
        public override MiranaType Merge(MiranaType type)
        {
            if (type is UnionType u) {
                return new UnionType(GetTypes().Concat(u.GetTypes()));
            }
            else {
                return new UnionType(GetTypes().Append(type));
            }
        }

        public IEnumerable<MiranaType> GetTypes() => typeOptions;
        public override string GetLiteralRepresentation()
        {
            return typeOptions.ConcatStringList(" | ");
        }

        public override bool Equals(object? obj)
        {
            return obj is UnionType t1 && GetTypes().Match(t1.GetTypes());
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(typeOptions);
        }
    }
    
    public enum LuaObjectTypeName
    {
        Table,
        LightUserdata,
        FullUserdata
    }

    class ConcreteLuaObjectType : ConcreteType
    {
        public LuaObjectTypeName LuaObjectTypeName { get; }
        public MiranaType? ArrayType { get; private set; }
        
        private Dictionary<object, MiranaType> Pairs { get; } = new();
        private readonly ReadWriteLock modifyLock = new();
        
        public ConcreteLuaObjectType(LuaObjectTypeName luaObjectTypeName) : base(MiranaTypeName.Object)
        {
            LuaObjectTypeName = luaObjectTypeName;
        }
        public void AddArrayType(MiranaType t)
        {
            using var _ = modifyLock.Write();
            ArrayType = ArrayType?.Merge(t)??t;
        }
        public void AddType(int index, MiranaType type)
        {
            
        }
        public override MiranaType Merge(MiranaType type)
        {
            return base.Merge(type);
        }

        public override string GetLiteralRepresentation()
        {
            using var _ = modifyLock.Read();
            var fieldMaps = Pairs.Select(t => $"\n\t{M.StringifyLuaIndex(t.Key)}: {t.Value.GetLiteralRepresentation()}").ToList();
            string objectDescription;
            if (fieldMaps.Count is 0) {
                if (ArrayType is null) {
                    objectDescription = "{}";
                }
                else {
                    objectDescription = $"{{ {MiranaTypeName.Int.GetLiteralRepresentation()}: {ArrayType.GetLiteralRepresentation()} }}";
                }
            }
            else {
                if (ArrayType is null) {
                    objectDescription = $"{{{fieldMaps}\n}}";
                }
                else {
                    objectDescription = $"{{\n\t{MiranaTypeName.Int.GetLiteralRepresentation()}: {ArrayType.GetLiteralRepresentation()}{fieldMaps}\n}}";
                }
            }
            string objectName = LuaObjectTypeName.GetLiteralRepresentation();
            return $"{objectName} {objectDescription}";
        }

        public override bool Equals(object? obj)
        {
            if (obj is ConcreteLuaObjectType o1) {
                using var _ = modifyLock.Read();
                using var _1 = o1.modifyLock.Read();
                if (LuaObjectTypeName != o1.LuaObjectTypeName)
                    return false;
                if (ArrayType is null) {
                    if (o1.ArrayType is not null)
                        return false;
                }
                else {
                    if (!ArrayType.Equals(o1.ArrayType))
                        return false;
                }
                if (!Pairs.Match(o1.Pairs, (p1, p2) => p1.Key == p2.Key && p1.Value.Equals(p2.Value)))
                    return false;
                return true;
            }
            return false;
        }
        public override int GetHashCode()
        {
            using var _ = modifyLock.Read();
            return HashCode.Combine(Pairs.Count(), ArrayType?.GetHashCode()??0);
        }
    }

    class ErrorType : MiranaType
    {
        private ErrorType() { }
        public static ErrorType Instance { get; } = new();
        public override MiranaType Merge(MiranaType type)
        {
            return Instance;
        }
        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj);
        }
        public override bool AssignableTo(MiranaType o)
        {
            return true;
        }
        public override string GetLiteralRepresentation()
        {
            return "<error-type>";
        }
        public override int GetHashCode()
        {
            return GetLiteralRepresentation().GetHashCode();
        }
    }
}