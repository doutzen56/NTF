using NTF.Ioc;
using System;

namespace NTF.Logger
{
    /// <summary>
    /// 日志基类
    /// </summary>
    public interface INtfLog
    {
        /// <summary>
        /// 基础信息日志
        /// </summary>
        /// <param name="msg">日志说明/内容</param>
        void Info(string msg);
        /// <summary>
        /// 基础信息类日志
        /// </summary>
        /// <param name="ex">异常信息</param>
        void Info(Exception ex);
        /// <summary>
        /// 基础信息类日志
        /// </summary>
        /// <param name="msg">日志说明/内容</param>
        /// <param name="ex">异常信息</param>
        void Info(string msg, Exception ex);
        /// <summary>
        /// 错误/异常信息日志
        /// </summary>
        /// <param name="msg">日志说明/内容</param>
        void Error(string msg);
        /// <summary>
        /// 错误/异常信息日志
        /// </summary>
        /// <param name="ex">异常信息</param>
        void Error(Exception ex);
        void Error(string msg, Exception ex);
        void Fatal(string msg);
        void Fatal(Exception ex);
        void Fatal(string msg, Exception ex);
        void Trace(string msg);
        void Trace(Exception ex);
        void Trace(string msg, Exception ex);
        void Warn(Exception ex);
        void Warn(string msg);
        void Warn(string msg, Exception ex);
        void Debug(Exception ex);
        void Debug(string msg);
        void Debug(string msg, Exception ex);
    }
}
