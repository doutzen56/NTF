using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTF.Data.Language
{
    /// <summary>
    /// 构建T-SQL语句
    /// </summary>
    public abstract class SQLFormatter
    {
        /// <summary>
        /// 
        /// </summary>
        public abstract StringBuilder CommandText { get; set; }
    }
}
