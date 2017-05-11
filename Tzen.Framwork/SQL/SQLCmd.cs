using Dapper;

namespace Tzen.Framwork.SQL
{
    public class SQLCmd
    {
        public SQLCmd()
        {
            Parameters = new DynamicParameters();
        }
        public string SQL { get; set; }
        public DynamicParameters Parameters { get; set; }
    }
}
