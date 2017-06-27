using Microsoft.VisualStudio.TestTools.UnitTesting;
using NTF.Data;
using NTF.Provider;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Data.Common;
using Castle.MicroKernel.Registration;
using Tzen;
using NTF.Utility;
using System;
using static NTF.Data.QueryProvider;
using NTF.Ioc;
using NTF;

namespace UnitTest
{
    [TestClass()]
    public class ProviderTest
    {
        #region 属性字段
        private static DbQueryProvider provider;
        private static string conStr = "Data Source=.;Initial Catalog=Tzen;Integrated Security=True";

        private static IDbContext<UserInfo> UserList;
        private static IDbContext<Orders> OrderList;
        private static IDbContext<ScoreInfo> ScoreInfo;
        private static FileStream fs;
        private static TextWriter tr;
        #endregion
       
        #region 初始化
        [ClassInitialize()]
        public static void Init(TestContext context)
        {
            var ioc = IocManager.Instance;
            NtfBootstrapper ntf = new NtfBootstrapper();
            ntf.Init();

            UserList = ioc.Resolve<IDbContext<UserInfo>>();
            OrderList = ioc.Resolve<IDbContext<Orders>>();
            ScoreInfo = ioc.Resolve<IDbContext<ScoreInfo>>();
            provider = DbQueryProvider.From(conStr);
            fs = new FileStream("G:\\Git\\NTF\\UnitTest\\SQL.txt", FileMode.OpenOrCreate);
            tr = new StreamWriter(fs, Encoding.UTF8);
            ((QueryProvider)UserList.Provider).Log = tr;
            
            List<UserInfo> list = new List<UserInfo>();
            for (int i = 0; i < 5; i++)
            {
                list.Add(new UserInfo() { Age = 27, Name = "测试", Address = "中国广州" });
            }
            UserList.Batch(list, (u, m) => u.Insert(m));
        }
        [ClassCleanup]
        public static void Flush()
        {
            tr.Flush();
            tr.Close();
        }
        #endregion

        [TestMethod]
        public void TestInsert()
        {
            var user = new UserInfo() { Age = 27, Name = "测试", Address = "中国广州" };
            var rs = UserList.Insert(user);
            Assert.AreEqual(1, rs);
        }
        [TestMethod]
        public void TestInsertAndResult()
        {
            var user = new UserInfo() { Age = 27, Name = "测试", Address = "中国广州" };
            var rs2 = UserList.Insert(user, a => a.ID);
            Assert.IsTrue(rs2 > 0);
        }
        [TestMethod]
        public void TestDelete()
        {
            var user = UserList.FirstOrDefault(a => a.Age == 27);
            var rs = UserList.Delete(user);
            Assert.IsTrue(rs > 0);
        }
        [TestMethod]
        public void TestDeleteMany()
        {
            var rs = UserList.Delete(a => a.Name == "测试");
            Assert.IsTrue(rs > 0);
        }
        [TestMethod]
        public void TestBatchDelete()
        {
            List<UserInfo> list = new List<UserInfo>();
            for (int i = 0; i < 5; i++)
            {
                list.Add(new UserInfo() { Age = 27, Name = "TestBatchDelete", Address = "中国广州" });
            }
            var result = UserList.Batch(list, (u, m) => u.Insert(m, a => a));
            var rs = UserList.Batch(result, (u, m) => u.Delete(m));
            Assert.IsTrue(rs.Count() > 0 && rs.Any(a => a == 1));
        }
        [TestMethod]
        public void TestBatchUpdate()
        {
            var list = UserList.Where(a => a.Name == "jdc").ToList();
            list.ForEach(
                item =>
                    {
                        item.Address = "广州天河区体育中心";
                    });
            var rs = UserList.Batch(list, (u, m) => u.Update(m));
            Assert.IsTrue(rs.Count() > 0 && rs.Any(a => a == 1));
        }
        [TestMethod]
        public void TestManyUpdate()
        {
            var rs = UserList.Update(a => a.Name == "jdc", a => new UserInfo { Address = "中国北京" });
            Assert.IsTrue(rs > 0);
        }
        [TestMethod]
        public void TestUpdate()
        {
            var user = UserList.FirstOrDefault(a => a.Name == "jdc");
            user.Address = "山西、西安、甘肃";
            var rs = UserList.Update(user);
            Assert.IsTrue(rs > 0);
        }

        [TestMethod]
        public void TestPageList()
        {
            var list = UserList.Where(a => a.Name == "jdc")
                             .Select(a => a)
                             .Skip(0)
                             .Take(10)
                             .OrderBy(a => a.ID);
            Assert.AreEqual(10, list.Count());

            //var user = new UserInfo() { Age = 27, Name = "测试事务", Address = "中国广州" };
            //provider.Connection.Open();
            //using (var tran = provider.Connection.BeginTransaction())
            //{
            //    provider.Transaction = tran;
            //    UserList.Insert(user);
            //    var u = UserList.Where(a => a.Name == "测试事务");
            //    UserList.Update(a => a.Name == "测试事务", a => new UserInfo() { Address = "甘肃" });
            //    tran.Commit();
            //}
            //IDbContext<UserInfo> us = new QueryProvider.DbQueryContenxt<UserInfo>(provider, provider.Mapping.GetEntity(typeof(UserInfo)));
            //var rs = us.GetById(421);
        }

    }
}
