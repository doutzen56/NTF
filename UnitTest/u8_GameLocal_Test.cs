
using IntegrationAdmin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NTF;
using NTF.Data;
using NTF.Ioc;
using NTF.Provider;
using NTF.Uow;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest
{
    [TestClass()]
    public class u8_GameLocal_Test
    {
        #region 属性字段
        private static DbQueryProvider provider;
        private static string conStr = "Data Source=192.168.3.180,1500;Initial Catalog=IntegrationAdmin;Persist Security Info=True;User ID=sa;Password=!QAZxsw2";
        private static IocManager ioc;
        private static IDbContext<u8_GameLocal> games;
        private static FileStream fs;
        private static TextWriter tr;
        #endregion
        #region 初始化
        [ClassInitialize()]
        public static void Init(TestContext context)
        {
            ioc = IocManager.Instance;
            NtfBootstrapper ntf = new NtfBootstrapper();
            ntf.Init();

            //provider = DbQueryProvider.From(conStr);
            games = ioc.Resolve<IDbContext<u8_GameLocal>>();
            //fs = new FileStream("D:\\工作  work\\奖金池API\\EvebService\\UnitTest\\SQL.txt", FileMode.OpenOrCreate);
            //tr = new StreamWriter(fs, Encoding.UTF8);
            //((QueryProvider)games.Provider).Log = tr;

        }
        [ClassCleanup]
        public static void Flush()
        {
            //tr.Flush();
            //tr.Close();
        }
        #endregion

        [TestMethod]
        public void MyTestMethod()
        {
            //var up = ioc.Resolve<IDbContext<u8_Update_Jackpot>>();
            //up.Insert(new u8_Update_Jackpot()
            //{
            //    Amount = 100,
            //    GameIdentify = "sdsf",
            //    GamePlatform = "sdfsdf",
            //    IntervalLength = 100,
            //    IntervalTime = 10,
            //    JackpotsInfo = 2,
            //    JackpotsParams = "",
            //    UpdateTime = DateTime.Now
            //});
            var t=ioc.Resolve<Test>();
            t.Test1();
        }
        
    }
}
