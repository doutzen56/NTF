using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Tzen.Framework.Provider {
    public class Customers {
        public string CustomerID;
        public string ContactName;
        public string Phone;
        public string City;
        public string Country;
    }

    public class Orders {
        public int OrderID;
        public string CustomerID;
        public DateTime OrderDate;
    }

    public class Northwind {
        public Query<Customers> Customers;
        public Query<Orders> Orders;

        private DbQueryProvider provider;

        public Northwind(DbConnection connection) {
            this.provider = new DbQueryProvider(connection);
            this.Customers = new Query<Customers>(this.provider);
            this.Orders = new Query<Orders>(this.provider);
        }

        public TextWriter Log {
            get { return this.provider.Log; }
            set { this.provider.Log = value; }
        }

        public D Compile<D>(Expression<D> query) {
            return (D)this.provider.Execute(query);
        }

        public Func<T> Compile<T>(Expression<Func<T>> query) {
            return (Func<T>)this.provider.Execute(query);
        }

        public Func<T1, T2> Compile<T1,T2>(Expression<Func<T1, T2>> query) {
            return (Func<T1, T2>)this.provider.Execute(query);
        }

        public Func<T1, T2, T3> Compile<T1, T2, T3>(Expression<Func<T1, T2, T3>> query) {
            return (Func<T1, T2, T3>)this.provider.Execute(query);
        }

        public Func<T1, T2, T3, T4> Compile<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4>> query) {
            return (Func<T1, T2, T3, T4>)this.provider.Execute(query);
        }
    }

    class Program {
        static void Main(string[] args) {
            string constr = @"Data Source=.\SQLEXPRESS;AttachDbFilename=C:\data\Northwind.mdf;Integrated Security=True;Connect Timeout=30;User Instance=True;MultipleActiveResultSets=true";

            using (SqlConnection con = new SqlConnection(constr)) {
                con.Open();

                Northwind db = new Northwind(con);
                db.Log = Console.Out;

                NorthwindTranslationTests.Run(db, true);
                NorthwindExecutionTests.Run(db);
            }

            Console.ReadLine();
        }
    }
}
