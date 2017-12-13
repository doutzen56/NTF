using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NTF.Data.Language
{
    /// <summary>
    /// <see cref="Expression"/>谓语解析器，主要负责将<see cref="Expression"/>翻译成T-SQL的Where条件语句
    /// </summary>
    public abstract class LambdaParser : ExpressionVisitor
    {
        /// <summary>
        /// T-SQL Where条件
        /// </summary>
        public abstract StringBuilder PredicateText { get; set; }
        /// <summary>
        /// 由谓语解析器<see cref="LambdaParser"/>翻译的<see cref="PredicateText"/>语句中对应参数的值
        /// </summary>
        public abstract dynamic Parameters { get; set; }
        /// <summary>
        /// 用于动态生成<see cref="PredicateText"/>语句中变量与参数用
        /// </summary>
        private int Index = 0;
    }
}
