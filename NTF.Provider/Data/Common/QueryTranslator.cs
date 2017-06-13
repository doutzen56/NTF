using System.Linq.Expressions;

namespace NTF.Provider.Data.Common
{
    /// <summary>
    /// <see cref="Expression"/>查询解析器。
    /// </summary>
    public class QueryTranslator
    {
        QueryLinguist linguist;
        QueryMapper mapper;
        QueryPolice police;

        public QueryTranslator(QueryLanguage language, QueryMapping mapping, QueryPolicy policy)
        {
            this.linguist = language.CreateLinguist(this);
            this.mapper = mapping.CreateMapper(this);
            this.police = policy.CreatePolice(this);
        }

        public QueryLinguist Linguist
        {
            get { return this.linguist; }
        }

        public QueryMapper Mapper
        {
            get { return this.mapper; }
        }

        public QueryPolice Police
        {
            get { return this.police; }
        }
        /// <summary>
        /// <see cref="Expression"/>解析、翻译
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public virtual Expression Translate(Expression expression)
        {
            // 预执行本地表达式
            expression = PartialEvaluator.Eval(expression, this.mapper.Mapping.CanBeEvaluatedLocally);

            // 表达式映射，绑定LINQ运算
            expression = this.mapper.Translate(expression);

            // 解析和验证所有策略
            expression = this.police.Translate(expression);

            // 多（数据库）语言支持解析和验证
            expression = this.linguist.Translate(expression);

            return expression;
        }
    }
}