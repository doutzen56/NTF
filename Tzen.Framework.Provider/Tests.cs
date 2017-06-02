using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Tzen.Framework.Provider {
#if DEBUG
    public class TestHarness
    {
        protected class TestFailureException : Exception
        {
            internal TestFailureException(string message)
                : base(message)
            {
            }
        }

        private delegate void TestMethod();

        DbQueryProvider provider;
        XmlTextWriter baselineWriter;
        Dictionary<string, string> baselines;
        bool executeTests;
        protected MethodInfo currentMethod;

        protected TestHarness()
        {
        }

        protected void RunTests(DbQueryProvider provider, string baselineFile, string newBaselineFile, bool executeTests)
        {
            this.provider = provider;
            this.executeTests = executeTests;

            ReadBaselines(baselineFile);

            if (!string.IsNullOrEmpty(newBaselineFile))
            {
                baselineWriter = new XmlTextWriter(newBaselineFile, Encoding.UTF8);
                baselineWriter.Formatting = Formatting.Indented;
                baselineWriter.Indentation = 2;
                baselineWriter.WriteStartDocument();
                baselineWriter.WriteStartElement("baselines");
            }

            int iTest = 0;
            int iPassed = 0;
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Running tests: {0}", this.GetType().Name);

            try
            {
                var tests = this.GetType().GetMethods().Where(m => m.Name.StartsWith("Test"));

                foreach (MethodInfo method in tests)
                {
                    iTest++;
                    currentMethod = method;
                    string testName = method.Name.Substring(4);
                    bool passed = false;
                    Console.WriteLine();
                    Setup();
                    string reason = "";
                    try
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        TestMethod test = (TestMethod)Delegate.CreateDelegate(typeof(TestMethod), this, method);
                        test();
                        passed = true;
                        iPassed++;
                    }
                    catch (TestFailureException tf)
                    {
                        if (tf.Message != null)
                            reason = tf.Message;
                    }
                    finally
                    {
                        Teardown();
                    }

                    Console.ForegroundColor = passed ? ConsoleColor.Green : ConsoleColor.Red;
                    Console.WriteLine("Test {0}: {1} - {2}", iTest, method.Name, passed ? "PASSED" : "FAILED");
                    if (!passed && !string.IsNullOrEmpty(reason)) 
                        Console.WriteLine("Reason: {0}", reason);
                }
            }
            finally
            {
                if (baselineWriter != null)
                {
                    baselineWriter.WriteEndElement();
                    baselineWriter.Close();
                }
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("SUMMARY: {0}", this.GetType().Name);
            Console.WriteLine("Total tests run: {0}", iTest);

            Console.ForegroundColor = ConsoleColor.Green;
            if (iPassed == iTest)
            {
                Console.WriteLine("ALL tests passed!");
            }
            else
            {
                Console.WriteLine("Total tests passed: {0}", iPassed);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Total tests failed: {0}", iTest - iPassed);
            }
            Console.ForegroundColor = originalColor;
            Console.WriteLine();
        }

        protected virtual void Setup()
        {
        }

        protected virtual void Teardown()
        {
        }

        private void WriteBaseline(string key, string text)
        {
            if (baselineWriter != null)
            {
                baselineWriter.WriteStartElement("baseline");
                baselineWriter.WriteAttributeString("key", key);
                baselineWriter.WriteWhitespace("\r\n");
                baselineWriter.WriteString(text);
                baselineWriter.WriteEndElement();
            }
        }

        private void ReadBaselines(string filename)
        {
            if (!string.IsNullOrEmpty(filename) && File.Exists(filename))
            {
                XDocument doc = XDocument.Load(filename);
                this.baselines = doc.Root.Elements("baseline").ToDictionary(e => (string)e.Attribute("key"), e => e.Value);
            }
        }

        protected void TestQuery(IQueryable query)
        {
            TestQuery(query.Expression, currentMethod.Name, false);
        }

        protected void TestQuery(IQueryable query, string baselineKey)
        {
            TestQuery(query.Expression, baselineKey, false);
        }

        protected void TestQuery(Expression<Func<object>> query)
        {
            TestQuery(query.Body, currentMethod.Name, false);
        }

        protected void TestQuery(Expression<Func<object>> query, string baselineKey)
        {
            TestQuery(query.Body, baselineKey, false);
        }

        protected void TestQueryFails(IQueryable query)
        {
            TestQuery(query.Expression, currentMethod.Name, true);
        }

        protected void TestQueryFails(Expression<Func<object>> query)
        {
            TestQuery(query.Body, currentMethod.Name, true);
        }

        protected void TestQuery(Expression query, string baselineKey, bool expectedToFail)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            try
            {
                if (query.NodeType == ExpressionType.Convert && query.Type == typeof(object))
                {
                    query = ((UnaryExpression)query).Operand; // remove box
                }

                if (provider.Log != null)
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    provider.Log.WriteLine(query.ToString());
                    provider.Log.WriteLine("==>");
                }

                string queryText = null;
                try
                {
                    queryText = provider.GetQueryText(query);
                    WriteBaseline(baselineKey, queryText);
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(string.Format("Query translation failed for {0}", baselineKey));
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine(query.ToString());
                    throw new TestFailureException(e.Message);
                }

                string baseline = null;
                if (this.baselines != null && this.baselines.TryGetValue(baselineKey, out baseline))
                {
                    string trimAct = TrimExtraWhiteSpace(queryText).Trim();
                    string trimBase = TrimExtraWhiteSpace(baseline).Trim();
                    if (trimAct != trimBase)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Query translation does not match baseline:");
                        WriteDifferences(trimAct, trimBase);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("---- baseline ----");
                        WriteDifferences(trimBase, trimAct);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        throw new TestFailureException("Translation differed from baseline.");
                    }
                }

                if (this.executeTests)
                {
                    Exception caught = null;
                    try
                    {
                        object result = provider.Execute(query);
                        IDisposable disposable = result as IDisposable;
                        if (disposable != null) disposable.Dispose();
                    }
                    catch (Exception e)
                    {
                        caught = e;
                        if (!expectedToFail)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Query failed to execute:");
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.WriteLine(queryText);
                            throw new TestFailureException(e.Message);
                        }
                    }
                    if (caught == null && expectedToFail)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Query succeeded when expected to fail");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine(queryText);
                        throw new TestFailureException(null);
                    }
                }

                if (baseline == null && this.baselines != null)
                {
                    throw new TestFailureException("No baseline");
                }
            }
            finally
            {
                Console.ForegroundColor = originalColor;
            }
        }

        private string TrimExtraWhiteSpace(string s)
        {
            StringBuilder sb = new StringBuilder();
            bool lastWasWhiteSpace = false;
            foreach (char c in s)
            {
                bool isWS = char.IsWhiteSpace(c);
                if (!isWS || !lastWasWhiteSpace)
                {
                    if (isWS)
                        sb.Append(' ');
                    else
                        sb.Append(c);
                    lastWasWhiteSpace = isWS;
                }
            }
            return sb.ToString();
        }

        private void WriteDifferences(string s1, string s2)
        {
            int start = 0;
            bool same = true;
            for (int i = 0, n = Math.Min(s1.Length, s2.Length); i < n; i++)
            {
                bool matches = s1[i] == s2[i];
                if (matches != same)
                {
                    if (i > start)
                    {
                        Console.ForegroundColor = same ? ConsoleColor.Gray : ConsoleColor.White;
                        Console.Write(s1.Substring(start, i - start));
                    }
                    start = i;
                    same = matches;
                }
            }
            if (start < s1.Length)
            {
                Console.ForegroundColor = same ? ConsoleColor.Gray : ConsoleColor.White;
                Console.Write(s1.Substring(start));
            }
            Console.WriteLine();
        }
    }

    public class NorthwindTestHarness : TestHarness
    {
        protected Northwind db;

        protected void RunTests(Northwind db, string baselineFile, string newBaselineFile, bool executeQueries)
        {
            this.db = db;
            DbQueryProvider provider = (DbQueryProvider)((IQueryable)db.Customers).Provider;
            base.RunTests(provider, baselineFile, newBaselineFile, executeQueries);
        }

    }

    public class NorthwindTranslationTests : NorthwindTestHarness
    {
        public static void Run(Northwind db, bool executeQueries)
        {
            new NorthwindTranslationTests().RunTests(db, @"..\..\baseline.txt", @"newbase.txt", executeQueries);
        }

        ////// Start tests here /////

        public void TestWhere()
        {
            TestQuery( db.Customers.Where(c => c.City == "London") );
        }

        public void TestWhereTrue()
        {
            TestQuery( db.Customers.Where(c => true) );
        }

        public void TestWhereFalse()
        {
            TestQuery( db.Customers.Where(c => false) );
        }

        public void TestSelectScalar()
        {
            TestQuery( db.Customers.Select(c => c.City) );
        }

        public void TestSelectAnonymousOne() 
        {
            TestQuery( db.Customers.Select(c => new { c.City }) );
        }

        public void TestSelectAnonymousTwo()
        {
            TestQuery( db.Customers.Select(c => new { c.City, c.Phone }) );
        }

        public void TestSelectAnonymousThree()
        {
            TestQuery( db.Customers.Select(c => new { c.City, c.Phone, c.Country }) );
        }

        public void TestSelectCustomerTable()
        {
            TestQuery( db.Customers );
        }

        public void TestSelectCustomerIdentity()
        {
            TestQuery( db.Customers.Select(c => c) );
        }

        public void TestSelectAnonymousWithObject()
        {
            TestQuery( db.Customers.Select(c => new { c.City, c }) );
        }

        public void TestSelectAnonymousNested()
        {
            TestQuery( db.Customers.Select(c => new { c.City, Country = new { c.Country } }) );
        }

        public void TestSelectAnonymousEmpty()
        {
            TestQuery( db.Customers.Select(c => new { }) );
        }

        public void TestSelectAnonymousLiteral()
        {
            TestQuery( db.Customers.Select(c => new { X = 10 }) );
        }

        public void TestSelectConstantInt()
        {
            TestQuery( db.Customers.Select(c => 0) );
        }

        public void TestSelectConstantNullString()
        {
            TestQuery( db.Customers.Select(c => (string)null) );
        }

        public void TestSelectLocal()
        {
            int x = 10;
            TestQuery( db.Customers.Select(c => x) );
        }

        public void TestSelectNestedCollection()
        {
            TestQuery(
                from c in db.Customers
                where c.CustomerID == "ALFKI"
                select db.Orders.Where(o => o.CustomerID == c.CustomerID && o.OrderDate.Year == 1997).Select(o => o.OrderID)
                );
        }

        public void TestSelectNestedCollectionInAnonymousType()
        {
            TestQuery(
                from c in db.Customers
                where c.CustomerID == "ALFKI"
                select new { Foos = db.Orders.Where(o => o.CustomerID == c.CustomerID && o.OrderDate.Year == 1997).Select(o => o.OrderID) }
                );
        }

        public void TestJoinCustomerOrders()
        {
            TestQuery( 
                from c in db.Customers 
                join o in db.Orders on c.CustomerID equals o.CustomerID 
                select new { c.ContactName, o.OrderID }
                );
        }

        public void TestSelectManyCustomerOrders()
        {
            TestQuery(
                from c in db.Customers
                from o in db.Orders
                where c.CustomerID == o.CustomerID
                select new { c.ContactName, o.OrderID }
                );
        }

        public void TestOrderBy()
        {
            TestQuery(
                db.Customers.OrderBy(c => c.CustomerID)
                );
        }

        public void TestOrderBySelect()
        {
            TestQuery(
                db.Customers.OrderBy(c => c.CustomerID).Select(c => c.ContactName)
                );
        }

        public void TestOrderByOrderBy()
        {
            TestQuery(
                db.Customers.OrderBy(c => c.CustomerID).OrderBy(c => c.Country).Select(c => c.City)
                );
        }

        public void TestOrderByThenBy()
        {
            TestQuery(
                db.Customers.OrderBy(c => c.CustomerID).ThenBy(c => c.Country).Select(c => c.City)
                );
        }

        public void TestOrderByDescending()
        {
            TestQuery(
                db.Customers.OrderByDescending(c => c.CustomerID).Select(c => c.City)
                );
        }

        public void TestOrderByDescendingThenBy()
        {
            TestQuery(
                db.Customers.OrderByDescending(c => c.CustomerID).ThenBy(c => c.Country).Select(c => c.City)
                );
        }

        public void TestOrderByDescendingThenByDescending()
        {
            TestQuery(
                db.Customers.OrderByDescending(c => c.CustomerID).ThenByDescending(c => c.Country).Select(c => c.City)
                );
        }

        public void TestOrderByJoin()
        {
            TestQuery(
                from c in db.Customers.OrderBy(c => c.CustomerID)
                join o in db.Orders.OrderBy(o => o.OrderID) on c.CustomerID equals o.CustomerID
                select new { c.CustomerID, o.OrderID }
                );
        }

        public void TestOrderBySelectMany()
        {
            TestQuery(
                from c in db.Customers.OrderBy(c => c.CustomerID)
                from o in db.Orders.OrderBy(o => o.OrderID)
                where c.CustomerID == o.CustomerID
                select new { c.ContactName, o.OrderID }
                );

        }

        public void TestGroupBy()
        {
            TestQuery(
                db.Customers.GroupBy(c => c.City)
                );
        }

        public void TestGroupBySelectMany()
        {
            TestQuery(
                db.Customers.GroupBy(c => c.City).SelectMany(g => g)
                );
        }

        public void TestGroupBySum()
        {
            TestQuery(
                db.Orders.GroupBy(o => o.CustomerID).Select(g => g.Sum(o => o.OrderID))
                );
        }

        public void TestGroupByCount()
        {
            TestQuery(
                db.Orders.GroupBy(o => o.CustomerID).Select(g => g.Count())
                );
        }

        public void TestGroupBySumMinMaxAvg()
        {
            TestQuery(
                db.Orders.GroupBy(o => o.CustomerID).Select(g => 
                    new { 
                        Sum = g.Sum(o => o.OrderID), 
                        Min = g.Min(o => o.OrderID), 
                        Max = g.Max(o => o.OrderID), 
                        Avg = g.Average(o => o.OrderID)
                    })
                );
        }

        public void TestGroupByWithResultSelector()
        {
            TestQuery(
                db.Orders.GroupBy(o => o.CustomerID, (k,g) =>
                    new {
                        Sum = g.Sum(o => o.OrderID),
                        Min = g.Min(o => o.OrderID),
                        Max = g.Max(o => o.OrderID),
                        Avg = g.Average(o => o.OrderID)
                    })
                );
        }

        public void TestGroupByWithElementSelectorSum()
        {
            TestQuery(
                db.Orders.GroupBy(o => o.CustomerID, o => o.OrderID).Select(g => g.Sum())
                );
        }

        public void TestGroupByWithElementSelector()
        {
            // note: groups are retrieved through a separately execute subquery per row
            TestQuery(
                db.Orders.GroupBy(o => o.CustomerID, o => o.OrderID)
                );
        }

        public void TestGroupByWithElementSelectorSumMax()
        {
            TestQuery(
                db.Orders.GroupBy(o => o.CustomerID, o => o.OrderID).Select(g => new { Sum = g.Sum(), Max = g.Max() })
                );
        }

        public void TestGroupByWithAnonymousElement()
        {
            TestQuery(
                db.Orders.GroupBy(o => o.CustomerID, o => new { o.OrderID }).Select(g => g.Sum(x => x.OrderID))
                );
        }

        public void TestGroupByWithTwoPartKey()
        {
            TestQuery(
                db.Orders.GroupBy(o => new { o.CustomerID, o.OrderDate }).Select(g => g.Sum(o => o.OrderID))
                );
        }

        public void TestOrderByGroupBy()
        {
            // note: order-by is lost when group-by is applied (the sequence of groups is not ordered)
            TestQuery(
                db.Orders.OrderBy(o => o.OrderID).GroupBy(o => o.CustomerID).Select(g => g.Sum(o => o.OrderID))
                );
        }

        public void TestOrderByGroupBySelectMany()
        {
            // note: order-by is preserved within grouped sub-collections
            TestQuery(
                db.Orders.OrderBy(o => o.OrderID).GroupBy(o => o.CustomerID).SelectMany(g => g)
                );
        }

        public void TestSumWithNoArg()
        {
            TestQuery(
                () => db.Orders.Select(o => o.OrderID).Sum()
                );
        }

        public void TestSumWithArg()
        {
            TestQuery(
                () => db.Orders.Sum(o => o.OrderID)
                );                
        }

        public void TestCountWithNoPredicate()
        {
            TestQuery(
                () => db.Orders.Count()
                );
        }

        public void TestCountWithPredicate()
        {
            TestQuery(
                () => db.Orders.Count(o => o.CustomerID == "ALFKI")
                );
        }

        public void TestDistinct()
        {
            TestQuery(
                db.Customers.Distinct()
                );
        }

        public void TestDistinctScalar()
        {
            TestQuery(
                db.Customers.Select(c => c.City).Distinct()
                );
        }

        public void TestOrderByDistinct()
        {
            TestQuery(
                db.Customers.OrderBy(c => c.CustomerID).Select(c => c.City).Distinct()
                );
        }

        public void TestDistinctOrderBy()
        {
            TestQuery(
                db.Customers.Select(c => c.City).Distinct().OrderBy(c => c)
                );
        }

        public void TestDistinctGroupBy()
        {
            TestQuery(
                db.Orders.Distinct().GroupBy(o => o.CustomerID)
                );
        }

        public void TestGroupByDistinct()
        {
            TestQuery(
                db.Orders.GroupBy(o => o.CustomerID).Distinct()
                );

        }

        public void TestDistinctCount()
        {
            TestQuery(
                () => db.Customers.Distinct().Count()
                );
        }

        public void TestSelectDistinctCount()
        {
            // cannot do: SELECT COUNT(DISTINCT some-colum) FROM some-table
            // because COUNT(DISTINCT some-column) does not count nulls
            TestQuery(
                () => db.Customers.Select(c => c.City).Distinct().Count()
                );
        }

        public void TestSelectSelectDistinctCount()
        {
            TestQuery(
                () => db.Customers.Select(c => c.City).Select(c => c).Distinct().Count()
                );
        }

        public void TestDistinctCountPredicate()
        {
            TestQuery(
                () => db.Customers.Distinct().Count(c => c.CustomerID == "ALFKI")
                );
        }

        public void TestDistinctSumWithArg()
        {
            TestQuery(
                () => db.Orders.Distinct().Sum(o => o.OrderID)
                );
        }

        public void TestSelectDistinctSum()
        {
            TestQuery(
                () => db.Orders.Select(o => o.OrderID).Distinct().Sum()
                );
        }

        public void TestTake()
        {
            TestQuery(
                db.Orders.Take(5)
                );
        }

        public void TestTakeDistinct()
        {
            // distinct must be forced to apply after top has been computed
            TestQuery(
                db.Orders.Take(5).Distinct()
                );
        }

        public void TestDistinctTake()
        {
            // top must be forced to apply after distinct has been computed
            TestQuery(
                db.Orders.Distinct().Take(5)
                );
        }

        public void TestDistinctTakeCount()
        {
            TestQuery(
                () => db.Orders.Distinct().Take(5).Count()
                );
        }

        public void TestTakeDistinctCount()
        {
            TestQuery(
                () => db.Orders.Take(5).Distinct().Count()
                );
        }

        public void TestSkip()
        {
            TestQuery(
                db.Customers.OrderBy(c => c.ContactName).Skip(5)
                );
        }

        public void TestSkipTake()
        {
            TestQuery(
                db.Customers.OrderBy(c => c.ContactName).Skip(5).Take(10)
                );
        }

        public void TestTakeSkip()
        {
            TestQuery(
                db.Customers.OrderBy(c => c.ContactName).Take(10).Skip(5)
                );
        }

        public void TestSkipDistinct()
        {
            TestQuery(
                db.Customers.OrderBy(c => c.ContactName).Skip(5).Distinct()
                );
        }

        public void TestDistinctSkip()
        {
            TestQuery(
                db.Customers.Distinct().OrderBy(c => c.ContactName).Skip(5)
                );
        }

        public void TestSkipTakeDistinct()
        {
            TestQuery(
                db.Customers.OrderBy(c => c.ContactName).Skip(5).Take(10).Distinct()
                );
        }

        public void TestTakeSkipDistinct()
        {
            TestQuery(
                db.Customers.OrderBy(c => c.ContactName).Take(10).Skip(5).Distinct()
                );
        }

        public void TestDistinctSkipTake()
        {
            TestQuery(
                db.Customers.Distinct().OrderBy(c => c.ContactName).Skip(5).Take(10)
                );
        }


        public void TestFirst()
        {
            TestQuery(
                () => db.Customers.First()
                );
        }

        public void TestFirstPredicate()
        {
            TestQuery(
                () => db.Customers.First(c => c.CustomerID == "ALFKI")
                );
        }

        public void TestWhereFirst()
        {
            TestQuery(
                () => db.Customers.Where(c => c.CustomerID == "ALFKI").First()
                );
        }

        public void TestFirstOrDefault()
        {
            TestQuery(
                () => db.Customers.FirstOrDefault()
                );
        }

        public void TestFirstOrDefaultPredicate()
        {
            TestQuery(
                () => db.Customers.FirstOrDefault(c => c.CustomerID == "ALFKI")
                );
        }

        public void TestWhereFirstOrDefault()
        {
            TestQuery(
                () => db.Customers.Where(c => c.CustomerID == "ALFKI").FirstOrDefault()
                );
        }

        public void TestSingle()
        {
            TestQueryFails(
                () => db.Customers.Single()
                );
        }

        public void TestSinglePredicate()
        {
            TestQuery(
                () => db.Customers.Single(c => c.CustomerID == "ALFKI")
                );
        }

        public void TestWhereSingle()
        {
            TestQuery(
                () => db.Customers.Where(c => c.CustomerID == "ALFKI").Single()
                );
        }

        public void TestSingleOrDefault()
        {
            TestQueryFails(
                () => db.Customers.SingleOrDefault()
                );
        }

        public void TestSingleOrDefaultPredicate()
        {
            TestQuery(
                () => db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI")
                );
        }

        public void TestWhereSingleOrDefault()
        {
            TestQuery(
                () => db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault()
                );
        }

        public void TestAnyWithSubquery()
        {
            TestQuery(
                db.Customers.Where(c => db.Orders.Where(o => o.CustomerID == c.CustomerID).Any(o => o.OrderDate.Year == 1997))
                );
        }

        public void TestAnyWithSubqueryNoPredicate()
        {
            TestQuery(
                db.Customers.Where(c => db.Orders.Where(o => o.CustomerID == c.CustomerID).Any())
                );
        }

        public void TestAnyWithLocalCollection()
        {
            string[] ids = new [] { "ABCDE", "ALFKI" };
            TestQuery(
                db.Customers.Where(c => ids.Any(id => c.CustomerID == id))
                );
        }

        public void TestAnyTopLevel()
        {
            TestQuery(
                () => db.Customers.Any()
                );
        }

        public void TestAllWithSubquery()
        {
            TestQuery(
                db.Customers.Where(c => db.Orders.Where(o => o.CustomerID == c.CustomerID).All(o => o.OrderDate.Year == 1997))
                );
        }

        public void TestAllWithLocalCollection()
        {
            string[] patterns = new [] { "a", "e" };

            TestQuery(
                db.Customers.Where(c => patterns.All(p => c.ContactName.Contains(p)))
                );
        }

        public void TestAllTopLevel()
        {
            TestQuery(
                () => db.Customers.All(c => c.ContactName.StartsWith("a"))
                );
        }

        public void TestContainsWithSubquery()
        {
            TestQuery(
                db.Customers.Where(c => db.Orders.Select(o => o.CustomerID).Contains(c.CustomerID))
                );
        }

        public void TestContainsWithLocalCollection()
        {
            string[] ids = new[] { "ABCDE", "ALFKI" };
            TestQuery(
                db.Customers.Where(c => ids.Contains(c.CustomerID))
                );
        }

        public void TestContainsTopLevel()
        {
            TestQuery(
                () => db.Customers.Select(c => c.CustomerID).Contains("ALFKI")
                );
        }

        // framework function tests

        public void TestStringLength()
        {
            TestQuery(db.Customers.Where(c => c.City.Length == 7));
        }

        public void TestStringStartsWithLiteral()
        {
            TestQuery(db.Customers.Where(c => c.ContactName.StartsWith("M")));
        }

        public void TestStringStartsWithColumn()
        {
            TestQuery(db.Customers.Where(c => c.ContactName.StartsWith(c.ContactName)));
        }

        public void TestStringEndsWithLiteral()
        {
            TestQuery(db.Customers.Where(c => c.ContactName.EndsWith("s")));
        }

        public void TestStringEndsWithColumn()
        {
            TestQuery(db.Customers.Where(c => c.ContactName.EndsWith(c.ContactName)));
        }

        public void TestStringContainsLiteral()
        {
            TestQuery(db.Customers.Where(c => c.ContactName.Contains("and")));
        }

        public void TestStringContainsColumn()
        {
            TestQuery(db.Customers.Where(c => c.ContactName.Contains(c.ContactName)));
        }

        public void TestStringConcatImplicit2Args()
        {
            TestQuery(db.Customers.Where(c => c.ContactName + "X" == "X"));
        }

        public void TestStringConcatExplicit2Args()
        {
            TestQuery(db.Customers.Where(c => string.Concat(c.ContactName, "X") == "X"));
        }

        public void TestStringConcatExplicit3Args()
        {
            TestQuery(db.Customers.Where(c => string.Concat(c.ContactName, "X", c.Country) == "X"));
        }

        public void TestStringConcatExplicitNArgs()
        {
            TestQuery(db.Customers.Where(c => string.Concat(new string[] {c.ContactName, "X", c.Country}) == "X"));
        }

        public void TestStringIsNullOrEmpty()
        {
            TestQuery(db.Customers.Where(c => string.IsNullOrEmpty(c.City)));
        }

        public void TestStringToUpper()
        {
            TestQuery(db.Customers.Where(c => c.City.ToUpper() == "SEATTLE"));
        }

        public void TestStringToLower()
        {
            TestQuery(db.Customers.Where(c => c.City.ToLower() == "seattle"));
        }

        public void TestStringReplace()
        {
            TestQuery(db.Customers.Where(c => c.City.Replace("ea", "ae") == "Saettle"));
        }

        public void TestStringReplaceChars()
        {
            TestQuery(db.Customers.Where(c => c.City.Replace("e", "y") == "Syattly"));
        }

        public void TestStringSubstring()
        {
            TestQuery(db.Customers.Where(c => c.City.Substring(0, 4) == "Seat"));
        }

        public void TestStringSubstringNoLength()
        {
            TestQuery(db.Customers.Where(c => c.City.Substring(4) == "tle"));
        }

        public void TestStringRemove()
        {
            TestQuery(db.Customers.Where(c => c.City.Remove(1, 2) == "Sttle"));
        }

        public void TestStringRemoveNoCount()
        {
            TestQuery(db.Customers.Where(c => c.City.Remove(4) == "Seat"));
        }

        public void TestStringIndexOf()
        {
            TestQuery(db.Customers.Where(c => c.City.IndexOf("tt") == 4));
        }

        public void TestStringIndexOfChar()
        {
            TestQuery(db.Customers.Where(c => c.City.IndexOf('t') == 4));
        }

        public void TestStringTrim()
        {
            TestQuery(db.Customers.Where(c => c.City.Trim() == "Seattle"));
        }

        public void TestStringToString()
        {
            // just to prove this is a no op
            TestQuery(db.Customers.Where(c => c.City.ToString() == "Seattle"));
        }

        public void TestDateTimeConstructYMD()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate == new DateTime(o.OrderID, 1, 1)));
        }

        public void TestDateTimeConstructYMDHMS()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate == new DateTime(o.OrderID, 1, 1, 10, 25, 55)));
        }

        public void TestDateTimeDay()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate.Day == 5));
        }

        public void TestDateTimeMonth()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate.Month == 12));
        }

        public void TestDateTimeYear()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate.Year == 1997));
        }

        public void TestDateTimeHour()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate.Hour == 6));
        }

        public void TestDateTimeMinute()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate.Minute == 32));
        }

        public void TestDateTimeSecond()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate.Second == 47));
        }

        public void TestDateTimeMillisecond()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate.Millisecond == 200));
        }

        public void TestDateTimeDayOfWeek()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate.DayOfWeek == DayOfWeek.Friday));
        }

        public void TestDateTimeDayOfYear()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate.DayOfYear == 360));
        }

        public void TestMathAbs()
        {
            TestQuery(db.Orders.Where(o => Math.Abs(o.OrderID) == 10));
        }

        public void TestMathAcos()
        {
            TestQuery(db.Orders.Where(o => Math.Acos(o.OrderID) == 0));
        }

        public void TestMathAsin()
        {
            TestQuery(db.Orders.Where(o => Math.Asin(o.OrderID) == 0));
        }

        public void TestMathAtan()
        {
            TestQuery(db.Orders.Where(o => Math.Atan(o.OrderID) == 0));
        }

        public void TestMathAtan2()
        {
            TestQuery(db.Orders.Where(o => Math.Atan2(o.OrderID, 3) == 0));
        }

        public void TestMathCos()
        {
            TestQuery(db.Orders.Where(o => Math.Cos(o.OrderID) == 0));
        }

        public void TestMathSin()
        {
            TestQuery(db.Orders.Where(o => Math.Sin(o.OrderID) == 0));
        }

        public void TestMathTan()
        {
            TestQuery(db.Orders.Where(o => Math.Tan(o.OrderID) == 0));
        }

        public void TestMathExp()
        {
            TestQuery(db.Orders.Where(o => Math.Exp(o.OrderID) == 0));
        }

        public void TestMathLog()
        {
            TestQuery(db.Orders.Where(o => Math.Log(o.OrderID) == 0));
        }

        public void TestMathLog10()
        {
            TestQuery(db.Orders.Where(o => Math.Log10(o.OrderID) == 0));
        }

        public void TestMathSqrt()
        {
            TestQuery(db.Orders.Where(o => Math.Sqrt(o.OrderID) == 0));
        }

        public void TestMathCeiling()
        {
            TestQuery(db.Orders.Where(o => Math.Ceiling((double)o.OrderID) == 0));
        }

        public void TestMathFloor()
        {
            TestQuery(db.Orders.Where(o => Math.Floor((double)o.OrderID) == 0));
        }

        public void TestMathPow()
        {
            TestQuery(db.Orders.Where(o => Math.Pow(o.OrderID, 3) == 0));
        }

        public void TestMathRoundDefault()
        {
            TestQuery(db.Orders.Where(o => Math.Round((decimal)o.OrderID) == 0));
        }

        public void TestMathRoundToPlace()
        {
            TestQuery(db.Orders.Where(o => Math.Round((decimal)o.OrderID, 2) == 0));
        }

        public void TestMathTruncate()
        {
            TestQuery(db.Orders.Where(o => Math.Truncate((double)o.OrderID) == 0));
        }

        public void TestStringCompareToLT()
        {
            TestQuery(db.Customers.Where(c => c.City.CompareTo("Seattle") < 0));
        }

        public void TestStringCompareToLE()
        {
            TestQuery(db.Customers.Where(c => c.City.CompareTo("Seattle") <= 0));
        }

        public void TestStringCompareToGT()
        {
            TestQuery(db.Customers.Where(c => c.City.CompareTo("Seattle") > 0));
        }

        public void TestStringCompareToGE()
        {
            TestQuery(db.Customers.Where(c => c.City.CompareTo("Seattle") >= 0));
        }

        public void TestStringCompareToEQ()
        {
            TestQuery(db.Customers.Where(c => c.City.CompareTo("Seattle") == 0));
        }

        public void TestStringCompareToNE()
        {
            TestQuery(db.Customers.Where(c => c.City.CompareTo("Seattle") != 0));
        }

        public void TestStringCompareLT()
        {
            TestQuery(db.Customers.Where(c => string.Compare(c.City, "Seattle") < 0));
        }

        public void TestStringCompareLE()
        {
            TestQuery(db.Customers.Where(c => string.Compare(c.City, "Seattle") <= 0));
        }

        public void TestStringCompareGT()
        {
            TestQuery(db.Customers.Where(c => string.Compare(c.City, "Seattle") > 0));
        }

        public void TestStringCompareGE()
        {
            TestQuery(db.Customers.Where(c => string.Compare(c.City, "Seattle") >= 0));
        }

        public void TestStringCompareEQ()
        {
            TestQuery(db.Customers.Where(c => string.Compare(c.City, "Seattle") == 0));
        }

        public void TestStringCompareNE()
        {
            TestQuery(db.Customers.Where(c => string.Compare(c.City, "Seattle") != 0));
        }

        public void TestIntCompareTo()
        {
            // prove that x.CompareTo(y) works for types other than string
            TestQuery(db.Orders.Where(o => o.OrderID.CompareTo(1000) == 0));
        }

        public void TestDecimalCompare()
        {
            // prove that type.Compare(x,y) works with decimal
            TestQuery(db.Orders.Where(o => decimal.Compare((decimal)o.OrderID, 0.0m) == 0));
        }

        public void TestDecimalAdd()
        {
            TestQuery(db.Orders.Where(o => decimal.Add(o.OrderID, 0.0m) == 0.0m));
        }

        public void TestDecimalSubtract()
        {
            TestQuery(db.Orders.Where(o => decimal.Subtract(o.OrderID, 0.0m) == 0.0m));
        }

        public void TestDecimalMultiply()
        {
            TestQuery(db.Orders.Where(o => decimal.Multiply(o.OrderID, 1.0m) == 1.0m));
        }

        public void TestDecimalDivide()
        {
            TestQuery(db.Orders.Where(o => decimal.Divide(o.OrderID, 1.0m) == 1.0m));
        }

        public void TestDecimalRemainder()
        {
            TestQuery(db.Orders.Where(o => decimal.Remainder(o.OrderID, 1.0m) == 0.0m));
        }

        public void TestDecimalNegate()
        {
            TestQuery(db.Orders.Where(o => decimal.Negate(o.OrderID) == 1.0m));
        }

        public void TestDecimalCeiling()
        {
            TestQuery(db.Orders.Where(o => decimal.Ceiling(o.OrderID) == 0.0m));
        }

        public void TestDecimalFloor()
        {
            TestQuery(db.Orders.Where(o => decimal.Floor(o.OrderID) == 0.0m));
        }

        public void TestDecimalRoundDefault()
        {
            TestQuery(db.Orders.Where(o => decimal.Round(o.OrderID) == 0m));
        }

        public void TestDecimalRoundPlaces()
        {
            TestQuery(db.Orders.Where(o => decimal.Round(o.OrderID, 2) == 0.00m));
        }

        public void TestDecimalTruncate()
        {
            TestQuery(db.Orders.Where(o => decimal.Truncate(o.OrderID) == 0m));
        }

        public void TestDecimalLT()
        {
            // prove that decimals are treated normally with respect to normal comparison operators
            TestQuery(db.Orders.Where(o => ((decimal)o.OrderID) < 0.0m));
        }

        public void TestIntLessThan()
        {
            TestQuery(db.Orders.Where(o => o.OrderID < 0));
        }

        public void TestIntLessThanOrEqual()
        {
            TestQuery(db.Orders.Where(o => o.OrderID <= 0));
        }

        public void TestIntGreaterThan()
        {
            TestQuery(db.Orders.Where(o => o.OrderID > 0));
        }

        public void TestIntGreaterThanOrEqual()
        {
            TestQuery(db.Orders.Where(o => o.OrderID >= 0));
        }

        public void TestIntEqual()
        {
            TestQuery(db.Orders.Where(o => o.OrderID == 0));
        }

        public void TestIntNotEqual()
        {
            TestQuery(db.Orders.Where(o => o.OrderID != 0));
        }

        public void TestIntAdd()
        {
            TestQuery(db.Orders.Where(o => o.OrderID + 0 == 0));
        }

        public void TestIntSubtract()
        {
            TestQuery(db.Orders.Where(o => o.OrderID - 0 == 0));
        }

        public void TestIntMultiply()
        {
            TestQuery(db.Orders.Where(o => o.OrderID * 1 == 1));
        }

        public void TestIntDivide()
        {
            TestQuery(db.Orders.Where(o => o.OrderID / 1 == 1));
        }

        public void TestIntModulo()
        {
            TestQuery(db.Orders.Where(o => o.OrderID % 1 == 0));
        }

        public void TestIntLeftShift()
        {
            TestQuery(db.Orders.Where(o => o.OrderID << 1 == 0));
        }

        public void TestIntRightShift()
        {
            TestQuery(db.Orders.Where(o => o.OrderID >> 1 == 0));
        }

        public void TestIntBitwiseAnd()
        {
            TestQuery(db.Orders.Where(o => (o.OrderID & 1) == 0));
        }

        public void TestIntBitwiseOr()
        {
            TestQuery(db.Orders.Where(o => (o.OrderID | 1) == 1));
        }

        public void TestIntBitwiseExclusiveOr()
        {
            TestQuery(db.Orders.Where(o => (o.OrderID ^ 1) == 1));
        }

        public void TestIntBitwiseNot()
        {
            TestQuery(db.Orders.Where(o => ~o.OrderID == 0));
        }

        public void TestIntNegate()
        {
            TestQuery(db.Orders.Where(o => -o.OrderID == -1));
        }

        public void TestAnd()
        {
            TestQuery(db.Orders.Where(o => o.OrderID > 0 && o.OrderID < 2000));
        }

        public void TestOr()
        {
            TestQuery(db.Orders.Where(o => o.OrderID < 5 || o.OrderID > 10));
        }

        public void TestNot()
        {
            TestQuery(db.Orders.Where(o => !(o.OrderID == 0)));
        }

        public void TestEqualNull()
        {
            TestQuery(db.Customers.Where(c => c.City == null));
        }

        public void TestEqualNullReverse()
        {
            TestQuery(db.Customers.Where(c => null == c.City));
        }

        public void TestCoalsce()
        {
            TestQuery(db.Customers.Where(c => (c.City ?? "Seattle") == "Seattle"));
        }

        public void TestCoalesce2()
        {
            TestQuery(db.Customers.Where(c => (c.City ?? c.Country ?? "Seattle") == "Seattle"));
        }

        public void TestConditional()
        {
            TestQuery(db.Orders.Where(o => (o.CustomerID == "ALFKI" ? 1000 : 0) == 1000));
        }

        public void TestConditional2()
        {
            TestQuery(db.Orders.Where(o => (o.CustomerID == "ALFKI" ? 1000 : o.CustomerID == "ABCDE" ? 2000 : 0) == 1000));
        }

        public void TestConditionalTestIsValue()
        {
            TestQuery(db.Orders.Where(o => (((bool)(object)o.CustomerID) ? 100 : 200) == 100));
        }

        public void TestConditionalResultsArePredicates()
        {
            TestQuery(db.Orders.Where(o => (o.CustomerID == "ALFKI" ? o.OrderID < 10 : o.OrderID > 10)));
        }
    }

    public class NorthwindExecutionTests : NorthwindTestHarness
    {
        public static void Run(Northwind db)
        {
            new NorthwindExecutionTests().RunTests(db, null, null, true);
        }

        public void TestCompiledQuery()
        {
            var fn = db.Compile((string id) => db.Customers.Where(c => c.CustomerID == id));
            var items = fn("ALKFI").ToList();
        }

        public void TestCompiledQuerySingleton()
        {
            var fn = db.Compile((string id) => db.Customers.SingleOrDefault(c => c.CustomerID == id));
            Customers cust = fn("ALKFI");
        }

        public void TestCompiledQueryCount()
        {
            var fn = db.Compile((string id) => db.Customers.Count(c => c.CustomerID == id));
            int n = fn("ALKFI");
        }

        public void TestCompiledQueryIsolated()
        {
            // prove that I can compile using one provider instance and execute from another
            var fn = new Northwind(null).Compile((Northwind n, string id) => n.Customers.Where(c => c.CustomerID == id));
            var items = fn(this.db, "ALFKI").ToList();
        }

        public void TestCompiledQueryIsolatedWithHeirarchy()
        {
            // prove that I can compile using one provider instance and execute from another
            var fn = new Northwind(null).Compile((Northwind n, string id) => n.Customers.Where(c => c.CustomerID == id).Select(c => n.Orders.Where(o => o.CustomerID == c.CustomerID)));
            var items = fn(this.db, "ALFKI").ToList();
        }
    }

#endif
}