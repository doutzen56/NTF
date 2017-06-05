using System.Collections.Generic;
using System.Linq.Expressions;

namespace Tzen.Framework.Provider
{
    /// <summary>
    ///  返回查询源的所有别名集
    /// </summary>
    internal class AliasesProduced : DbExpressionVisitor
    {
        HashSet<string> aliases;

        private AliasesProduced()
        {
            this.aliases = new HashSet<string>();
        }

        internal static HashSet<string> Gather(Expression source)
        {
            AliasesProduced produced = new AliasesProduced();
            produced.Visit(source);
            return produced.aliases;
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            this.aliases.Add(select.Alias);
            return select;
        }

        protected override Expression VisitTable(TableExpression table)
        {
            this.aliases.Add(table.Alias);
            return table;
        }
    }
}
