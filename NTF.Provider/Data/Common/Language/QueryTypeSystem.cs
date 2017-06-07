using System;

namespace NTF.Provider.Data.Common
{
    public abstract class QueryType
    {
        public abstract bool NotNull { get; }
        public abstract int Length { get; }
        public abstract short Precision { get; }
        public abstract short Scale { get; }
    }

    public abstract class QueryTypeSystem 
    {
        public abstract QueryType Parse(string typeDeclaration);
        public abstract QueryType GetColumnType(Type type);
        public abstract string GetVariableDeclaration(QueryType type, bool suppressSize);
    }
}