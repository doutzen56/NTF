using System.Linq.Expressions;

namespace NTF.Provider.Data.Common
{
    /// <summary>
    /// Defines query execution & materialization policies. 
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

        public virtual Expression Translate(Expression expression)
        {
            // pre-evaluate local sub-trees
            expression = PartialEvaluator.Eval(expression, this.mapper.Mapping.CanBeEvaluatedLocally);

            // apply mapping (binds LINQ operators too)
            expression = this.mapper.Translate(expression);

            // any policy specific translations or validations
            expression = this.police.Translate(expression);

            // any language specific translations or validations
            expression = this.linguist.Translate(expression);

            return expression;
        }
    }
}