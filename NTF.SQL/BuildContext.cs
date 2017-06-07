using Dapper;

namespace NTF.SQL
{
    /// <summary>
    /// SQL语句（命令与参数）
    /// </summary>
    public class BuildCommand
    {
        public BuildCommand()
        {
            SQL = string.Empty;
            Parameters = new DynamicParameters();
        }
        /// <summary>
        /// SQL语句
        /// </summary>
        public string SQL { get; set; }
        /// <summary>
        /// 命令对应的参数
        /// </summary>
        public DynamicParameters Parameters { get; set; }
    }

}
