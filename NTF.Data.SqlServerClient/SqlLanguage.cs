using NTF.Data;
using NTF.Data.Common;
using NTF.Extensions;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace NTF.Data.SqlServerClient
{

    /// <summary>
    /// TSQL特定查询语言
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
            return new FunctionExpression(TypeEx.GetMemberType(member), "SCOPE_IDENTITY()", null);
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
                expression = OrderByRewriter.Rewrite(this.Language, expression);
                expression = base.Translate(expression);
                expression = SkipToRowNumberRewriter.Rewrite(this.Language, expression);
                expression = OrderByRewriter.Rewrite(this.Language, expression);
                return expression;
            }

            public override string Format(Expression expression)
            {
                return SqlFormatter.Format(expression, this.Language);
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