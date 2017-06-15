using System;

namespace NTF.Data.Common
{
    /// <summary>
    /// 定义SQL查询中列的数据类型描述
    /// </summary>
    public abstract class QueryType
    {
        /// <summary>
        /// 是否可空
        /// </summary>
        public abstract bool NotNull { get; }
        /// <summary>
        /// 长度
        /// </summary>
        public abstract int Length { get; }
        /// <summary>
        /// 精度
        /// </summary>
        public abstract short Precision { get; }
        /// <summary>
        /// 比例
        /// </summary>
        public abstract short Scale { get; }
    }
    /// <summary>
    ///<see cref="QueryType"/>类型转换
    /// </summary>
    public abstract class QueryTypeSystem 
    {
        /// <summary>
        /// 根据类型声明<see cref="QueryType"/>
        /// </summary>
        /// <param name="typeDeclaration"></param>
        /// <returns></returns>
        public abstract QueryType Parse(string typeDeclaration);
        /// <summary>
        /// 根据CLR类型获取<see cref="QueryType"/>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public abstract QueryType GetColumnType(Type type);
        /// <summary>
        /// 获取变量声明
        /// </summary>
        /// <param name="type"><see cref="QueryType"/></param>
        /// <param name="suppressSize">是否限制大小</param>
        /// <returns></returns>
        public abstract string GetVariableDeclaration(QueryType type, bool suppressSize);
    }
}