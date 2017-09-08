using System;

namespace NTF.Logger
{
    /// <summary>
    /// 日志基类
    /// </summary>
    public interface INtfLog
    {
        void Info(string msg);
        void Info(Exception ex);
        void Error(string msg);
        void Error(Exception ex);
        void Fatal(string msg);
        void Fatal(Exception ex);
        void Trace(string msg);
        void Trace(Exception ex);
        void Warn(Exception ex);
        void Warn(string msg);
    }
}
