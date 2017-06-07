using NTF.Provider.Data;
using NTF.Provider.Data.Common;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace NTF.Provider.SqlServerClient
{

    /// <summary>
    /// TSQL specific QueryLanguage
    /// </summary>
    public class SqlLanguage : QueryLanguage
    {
        DbTypeSystem typeSystem = new DbTypeSystem();

        public SqlLanguage()
        {
        }

        public override QueryTypeSystem TypeSystem
        {
            get { return this.typeSystem; }
        }

        public override string Quote(string name)
        {
            if (name.StartsWith("[") && name.EndsWith("]"))
            {
                return name;
            }
            else if (name.IndexOf('.') > 0)
            {
                return "[" + string.Join("].[", name.Split(splitChars, StringSplitOptions.RemoveEmptyEntries)) + "]";
            }
            else
            {
                return "[" + name + "]";
            }
        }

        private static readonly char[] splitChars = new char[] { '.' };

        public override bool AllowsMultipleCommands
        {
            get { return true; }
        }

        public override bool AllowSubqueryInSelectWithoutFrom
        {
            get { return true; }
        }

        public override bool AllowDistinctInAggregates
        {
            get { return true; }
        }

        public override Expression GetGeneratedIdExpression(MemberInfo member)
        {
            return new FunctionExpression(TypeHelper.GetMemberType(member), "SCOPE_IDENTITY()", null);
        }

        public override QueryLinguist CreateLinguist(QueryTranslator translator)
        {
            return new TSqlLinguist(this, translator);
        }

        class TSqlLinguist : QueryLinguist
        {
            public TSqlLinguist(SqlLanguage language, QueryTranslator translator)
                : base(language, translator)
            {
            }

            public override Expression Translate(Expression expression)
            {
                // fix up any order-by's
                expression = OrderByRewriter.Rewrite(this.Language, expression);

                expression = base.Translate(expression);

                // convert skip/take info into RowNumber pattern
                expression = SkipToRowNumberRewriter.Rewrite(this.Language, expression);

                // fix up any order-by's we may have changed
                expression = OrderByRewriter.Rewrite(this.Language, expression);

                return expression;
            }

            public override string Format(Expression expression)
            {
                return SqlServerFormatter.Format(expression, this.Language);
            }
        }

        private static SqlLanguage _default;

        public static SqlLanguage Default
        {
            get
            {
                if (_default == null)
                {
                    System.Threading.Interlocked.CompareExchange(ref _default, new SqlLanguage(), null);
                }
                return _default;
            }
        }
    }
}