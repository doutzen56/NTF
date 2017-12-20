using System.Collections.Generic;

namespace NTF.Data
{
    /// <summary>
    /// T-SQL命令和参数
    /// </summary>
    public sealed class SQLCommand
    {
        public SQLCommand()
        {
            this.Parameters = new Dictionary<string, object>();
        }
        /// <summary>
        /// T-SQL命令
        /// </summary>
        public string CommandText { get; set; }
        /// <summary>
        /// 参数
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; }
    }
}
