﻿using System.Collections.Generic;
using System.Linq.Expressions;

namespace NTF.Data.Common
{
    /// <summary>
    ///  returns the set of all aliases produced by a query source
    /// </summary>
    public class ReferencedAliasGatherer : DbExpressionVisitor
    {
        HashSet<TableAlias> aliases;

        private ReferencedAliasGatherer()
        {
            this.aliases = new HashSet<TableAlias>();
        }

        public static HashSet<TableAlias> Gather(Expression source)
        {
            var gatherer = new ReferencedAliasGatherer();
            gatherer.Visit(source);
            return gatherer.aliases;
        }

        protected override Expression VisitColumn(ColumnExpression column)
        {
            this.aliases.Add(column.Alias);
            return column;
        }
    }
}
