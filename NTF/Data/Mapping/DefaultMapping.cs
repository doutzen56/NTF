using System;

namespace NTF.Data.Mapping
{
    public class DefaultMapping : BasicMapping
    {
        public override string GetTableAlias(Type type)
        {
            return type.Name;
        }

        public override string GetTableName(Type type)
        {
            return type.Name;
        }
    }
}
