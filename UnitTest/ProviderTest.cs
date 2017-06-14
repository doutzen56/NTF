using Microsoft.VisualStudio.TestTools.UnitTesting;
using NTF.Data;
using NTF.Provider;
using System.Linq;

namespace UnitTest
{
    [TestClass()]
    public class ProviderTest
    {
        #region 属性字段
        public static EntityProvider provider;
        public static string conStr = "Data Source=.;Initial Catalog=Tzen;Integrated Security=True";

        public static IDbContext<UserInfo> UserList;
        public static IDbContext<Orders> OrderList;
        #endregion

        #region 初始化
        [ClassInitialize()]
        public static void Init(TestContext context)
        {
            provider = DbEntityProvider.From(conStr);
            UserList = provider.GetTable<UserInfo>();
            OrderList = provider.GetTable<Orders>();
        }
        #endregion

        [TestMethod]
        public void TestInsert()
        {
            var user = new UserInfo() { Age = 27, Name = "测试", Address = "中国广州" };
            var rs = UserList.Insert(user);
            Assert.AreEqual(rs, 1);
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

        public void TestBatch()
        {
            var list = UserList.Where(a => a.Name == "测试").ToList();
            var rs = UserList.Batch(list, (u, m) =>u.Update(m) );
        }
    }
}
