using System.Collections.Generic;

namespace NTF.SQL
{
    /// <summary>
    /// 定义基本模板
    /// </summary>
    public interface ICmdTemplate
    {
        string InsertTmp { get; set; }
        string UpdateTmp { get; set; }
        string DeleteTmp { get; set; }
        string GetFirstTmp { get; set; }
        string GetListTmp { get; set; }
        string GetPageListTmp { get; set; }
    }
    /// <summary>
    /// 模板替换方法，支持自定义
    /// </summary>
    public interface IReplacer
    {
        string Replace(string template, Dictionary<ReplaceArgs, string> parameters);
    }
    /// <summary>
    /// SQL模板替换参数枚举值
    /// </summary>
    public enum ReplaceArgs
    {
        /// <summary>
        /// 数据库表名
        /// </summary>
        TableName = 0,
        /// <summary>
        /// 更新语句SET部分，如：Name=@Name,Age=@Age
        /// </summary>
        NameValues = 1,
        /// <summary>
        /// 查询字段部分，如：Name,Age,Address
        /// </summary>
        Columns = 2,
        /// <summary>
        /// Insert语句Values部分，如:@Name,@Age,@Address
        /// </summary>
        Values = 3,
        /// <summary>
        /// 排序字段
        /// </summary>
        OrderBy = 4,
        /// <summary>
        /// Where条件
        /// </summary>
        Where = 5,
        /// <summary>
        /// 页码数大于？，分页使用
        /// </summary>
        LeftThan = 6,
        /// <summary>
        /// 页码数小于等于？，分页使用
        /// </summary>
        RightThan = 7
    }
    /// <summary>
    /// SQL模板类，提供基本的增删改查模板以及默认的模板参数替换方法
    /// </summary>
    public abstract class CmdTemplate : ICmdTemplate, IReplacer
    {
        public abstract string DeleteTmp { get; set; }
        public abstract string UpdateTmp { get; set; }
        public abstract string InsertTmp { get; set; }
        public abstract string GetFirstTmp { get; set; }
        public abstract string GetListTmp { get; set; }
        public abstract string GetPageListTmp { get; set; }

        public virtual string Replace(string template, Dictionary<ReplaceArgs, string> parameters)
        {
            foreach (var item in parameters)
            {
                template = template.Replace("#{0}".Fmt(item.Key.ToString()), item.Value);
            }
            return template;
        }
    }

}
