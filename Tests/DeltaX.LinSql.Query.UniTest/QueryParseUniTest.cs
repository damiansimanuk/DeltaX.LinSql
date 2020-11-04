using DeltaX.LinSql.Table;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DeltaX.LinSql.Query.UniTest
{
    public class Poco
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Updated { get; set; }
        public bool Active { get; set; }
    }

    public class Poco2
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public DateTime Updated { get; set; }
        public bool Active { get; set; }
    }

    public class Tests
    {
        [SetUp]
        public void Setup()
        {
            ConfigureTalbes();
        }

        public void ConfigureTalbes()
        {
            var factory = TableQueryFactory.GetInstance();

            if (!factory.IsConfiguredTable<Poco>())
            {
                factory.ConfigureTable<Poco>("poco", cfg =>
                {
                    cfg.AddColumn(c => c.Id, "idPoco", true, true);
                    cfg.AddColumn(c => c.Name);
                    cfg.AddColumn(c => c.Updated, p => { p.IgnoreInsert = true; p.IgnoreUpdate = true; });
                    cfg.AddColumn(c => c.Active);
                });
            }

            if (!factory.IsConfiguredTable<Poco2>())
            {
                factory.ConfigureTable<Poco2>("poco2", cfg =>
                {
                    cfg.AddColumn(c => c.Id, null, true, true);
                    cfg.AddColumn(c => c.FullName);
                    cfg.AddColumn(c => c.Updated, p => { p.IgnoreInsert = true; p.IgnoreUpdate = true; });
                    cfg.AddColumn(c => c.Active);
                });
            }
        }

        [Test]
        public void TestQuerySimpleConstant()
        {
            Expression<Func<Poco, bool>> expression = t => t.Id == 2;

            var qp = new QueryParser(expression);
            var whereSql = qp.GetSql();
            var param = qp.GetParameters();
            var tableColumns = qp.GetTableColumns().ToList();

            Assert.AreEqual("t_1.\"idPoco\" = @arg_0", whereSql);
            Assert.IsTrue(param.ContainsKey("arg_0"));
            Assert.AreEqual(2, param["arg_0"]);
            Assert.AreEqual("poco", tableColumns[0].Key.Name);
            Assert.AreEqual(1, tableColumns[0].Value.Count());

            Assert.Pass();
        }

        [Test]
        public void TestQuerySimpleConstantAndVar()
        {
            var updated = DateTime.Now;

            Expression<Func<Poco, bool>> expression = t => t.Id == 2 && t.Updated < updated;

            var qp = new QueryParser(expression);
            var whereSql = qp.GetSql();
            var param = qp.GetParameters();
            var tableColumns = qp.GetTableColumns().ToList();

            Assert.AreEqual("(t_1.\"idPoco\" = @arg_0) AND (t_1.\"Updated\" < @arg_1)", whereSql);
            Assert.AreEqual(2, tableColumns[0].Value.Count());
            Assert.AreEqual("poco", tableColumns[0].Key.Name);

            Assert.AreEqual(2, param.Count());
            Assert.IsTrue(param.ContainsKey("arg_0"));
            Assert.AreEqual(2, param["arg_0"]);
            Assert.AreEqual(updated, param["arg_1"]);

            Assert.Pass();
        }

        [Test]
        public void TestQuerySimpleConstantAndVarModified()
        {
            var updated = DateTime.Now;
            var original_update = updated;

            Expression<Func<Poco, bool>> expression = t => t.Id == 2 && t.Updated < updated;

            var qp = new QueryParser(expression);
            var param = qp.GetParameters();

            Assert.AreEqual(2, param.Count());
            Assert.IsTrue(param.ContainsKey("arg_0"));
            Assert.AreEqual(2, param["arg_0"]);
            Assert.AreEqual(updated, param["arg_1"]);

            // Modify updated 
            updated = updated.AddMinutes(1);
            param = qp.GetParameters();

            Assert.AreEqual(2, param.Count());
            Assert.IsTrue(param.ContainsKey("arg_0"));
            Assert.AreEqual(2, param["arg_0"]);
            Assert.AreEqual(updated, param["arg_1"]);
            Assert.AreNotEqual(original_update, updated);

            Assert.Pass();
        }


        [Test]
        public void TestQueryStringContain()
        {

            Expression<Func<Poco, bool>> expression = t => t.Name.Contains("Pepe");

            var qp = new QueryParser(expression);
            var whereSql = qp.GetSql();
            var param = qp.GetParameters();

            Assert.AreEqual("t_1.\"Name\" LIKE '%' + @arg_0+ '%'", whereSql);
            Assert.AreEqual(1, param.Count());
            Assert.IsTrue(param.ContainsKey("arg_0"));
        }


        [Test]
        public void TestQueryAndOrGroup()
        {

            Expression<Func<Poco, bool>> expression = t => (t.Id == 2 && t.Id < 3) || t.Id == 5;

            var qp = new QueryParser(expression);
            var whereSql = qp.GetSql();
            var param = qp.GetParameters();

            Assert.AreEqual("((t_1.\"idPoco\" = @arg_0) AND (t_1.\"idPoco\" < @arg_1)) OR (t_1.\"idPoco\" = @arg_2)", whereSql);
            Assert.AreEqual(3, param.Count());
            Assert.IsTrue(param.ContainsKey("arg_0"));
            Assert.IsTrue(param.ContainsKey("arg_1"));
            Assert.IsTrue(param.ContainsKey("arg_2"));
            Assert.AreEqual(5, param["arg_2"]);
        }

        [Test]
        public void TestQueryUnary()
        {

            Expression<Func<Poco, bool>> expression = t => t.Active;

            var qp = new QueryParser(expression);
            var whereSql = qp.GetSql();
            var param = qp.GetParameters();

            Assert.AreEqual("t_1.\"Active\" <> 0", whereSql.Trim());
            Assert.AreEqual(0, param.Count());
        }

        [Test]
        public void TestQueryUnaryNot()
        {

            Expression<Func<Poco, bool>> expression = t => !t.Active;

            var qp = new QueryParser(expression);
            var whereSql = qp.GetSql();
            var param = qp.GetParameters();

            Assert.AreEqual("t_1.\"Active\" = 0", whereSql.Trim());
            Assert.AreEqual(0, param.Count());
        }

        [Test]
        public void TestQueryStringNull()
        {

            Expression<Func<Poco, bool>> expression = t => t.Name == null;

            var qp = new QueryParser(expression);
            var whereSql = qp.GetSql();
            var param = qp.GetParameters();

            Assert.AreEqual("t_1.\"Name\" IS NULL", whereSql.Trim());
            Assert.AreEqual(0, param.Count());
        }

        [Test]
        public void TestQueryStringNullOrEmpty()
        {

            Expression<Func<Poco, bool>> expression = t => string.IsNullOrEmpty(t.Name);

            var qp = new QueryParser(expression);
            var whereSql = qp.GetSql();
            var param = qp.GetParameters();

            Assert.AreEqual("ISNULL(t_1.\"Name\", '') = ''", whereSql.Trim());
            Assert.AreEqual(0, param.Count());
        }

        [Test]
        public void TestQueryUnaryNotAnd()
        {
            Expression<Func<Poco, bool>> expression = t => !t.Active && t.Updated < DateTime.Now.AddDays(1);

            var qp = new QueryParser(expression);
            var whereSql = qp.GetSql();
            var param = qp.GetParameters();

            Assert.AreEqual("t_1.\"Active\" = 0 AND (t_1.\"Updated\" < @arg_0)", whereSql.Trim());
            Assert.AreEqual(1, param.Count());
            Assert.LessOrEqual(DateTime.Now.AddDays(1).ToString(), param["arg_0"].ToString());
        }

        [Test]
        public void TestQueryInListInt()
        {
            List<int> list = Enumerable.Range(1, 3).ToList();
            Expression<Func<Poco, bool>> expression = t => list.Contains(t.Id);

            var qp = new QueryParser(expression);
            var whereSql = qp.GetSql();
            var param = qp.GetParameters();

            Assert.AreEqual("t_1.\"idPoco\" IN (1, 2, 3)", whereSql.Trim());
            Assert.AreEqual(0, param.Count());
        }

        [Test]
        public void TestQueryInListString()
        {
            List<string> list = Enumerable.Range(1, 3).Select(e => $"Name{e}").ToList();
            Expression<Func<Poco, bool>> expression = t => list.Contains(t.Name);

            var qp = new QueryParser(expression);
            var whereSql = qp.GetSql();
            var param = qp.GetParameters();

            Assert.AreEqual("t_1.\"Name\" IN ('Name1', 'Name2', 'Name3')", whereSql.Trim());
            Assert.AreEqual(0, param.Count());
        }

        [Test]
        public void TestQueryInListStringAndListInt()
        {
            List<string> listStr = Enumerable.Range(1, 3).Select(e => $"Name{e}").ToList();
            List<int> listInt = Enumerable.Range(1, 3).ToList();
            Expression<Func<Poco, bool>> expression = t => listStr.Contains(t.Name) && listInt.Contains(t.Id);

            var qp = new QueryParser(expression);
            var whereSql = qp.GetSql();
            var param = qp.GetParameters();

            Assert.AreEqual("(t_1.\"Name\" IN ('Name1', 'Name2', 'Name3')) AND (t_1.\"idPoco\" IN (1, 2, 3))", whereSql.Trim());
            Assert.AreEqual(0, param.Count());
        }

        [Test]
        public void TestQueryNotInListStringAndListInt()
        {
            List<string> listStr = Enumerable.Range(1, 3).Select(e => $"Name{e}").ToList();
            List<int> listInt = Enumerable.Range(1, 3).ToList();
            Expression<Func<Poco, bool>> expression = t => !listStr.Contains(t.Name) && !listInt.Contains(t.Id);

            var qp = new QueryParser(expression);
            var whereSql = qp.GetSql();
            var param = qp.GetParameters();

            Assert.AreEqual("(t_1.\"Name\" NOT IN ('Name1', 'Name2', 'Name3')) AND (t_1.\"idPoco\" NOT IN (1, 2, 3))", whereSql.Trim());
            Assert.AreEqual(0, param.Count());
        }


        [Test]
        public void TestQueryMultipleTable()
        {
            Expression<Func<Poco, Poco2, bool>> expression = (t1, t2) => t1.Id == t2.Id && t2.Active;

            var qp = new QueryParser(expression);
            var whereSql = qp.GetSql();
            var param = qp.GetParameters();

            Assert.AreEqual("(t_1.\"idPoco\" = t_2.\"Id\") AND t_2.\"Active\" <> 0", whereSql.Trim());
            Assert.AreEqual(0, param.Count());
        }

        [Test]
        public void TestQueryMultipleTableNotNull()
        {
            Expression<Func<Poco, Poco2, bool>> expression = (t1, t2) => t1.Name != null && t2.Active;

            var qp = new QueryParser(expression);
            var whereSql = qp.GetSql();
            var param = qp.GetParameters();

            Assert.AreEqual("(t_1.\"Name\" IS NOT NULL) AND t_2.\"Active\" <> 0", whereSql.Trim());
            Assert.AreEqual(0, param.Count());
        }

        [Test]
        public void TestQuerySelectMultipleTableArrayResult()
        {
            Expression<Func<Poco, Poco2, object[]>> expression = (t1, t2) => new object[] { t1.Id, t2.Id, t2.Active };

            var qp = new SelectParser(expression);
            var whereSql = qp.GetSql();
            var param = qp.GetParameters();

            Assert.AreEqual("t_1.\"idPoco\" as \"Id\", t_2.\"Id\", t_2.\"Active\"", whereSql.Trim());
            Assert.AreEqual(0, param.Count());
        }

        [Test]
        public void TestQuerySelectMultipleTableArrayResultAndString()
        {
            Expression<Func<Poco, Poco2, object[]>> expression = (t1, t2) => new object[] { t1.Id, t2.Id, t2.Active, "t_2.FullName" };

            var qp = new SelectParser(expression);
            var whereSql = qp.GetSql();
            var param = qp.GetParameters();

            Assert.AreEqual("t_1.\"idPoco\" as \"Id\", t_2.\"Id\", t_2.\"Active\", t_2.FullName", whereSql.Trim());
            Assert.AreEqual(0, param.Count());
        }

        [Test]
        public void TestQuerySelectMultipleTableObejct()
        {
            Expression<Func<Poco, Poco2, object>> expression = (t1, t2) => new { pepe = t1.Id, t2.Active };

            var qp = new SelectParser(expression);
            var whereSql = qp.GetSql();
            var param = qp.GetParameters();

            Assert.AreEqual("t_1.\"idPoco\" as \"Id\", t_2.\"Active\"", whereSql.Trim());
            Assert.AreEqual(0, param.Count());
        }


        [Test]
        public void TestQuerySelectMultipleTableAll()
        {
            Expression<Func<Poco, Poco2, object[]>> expression = (t1, t2) => new object[] { "t_2.*" };

            var factory = TableQueryFactory.GetInstance();
            var table = factory.GetTable<Poco>();
            var columns = "" + factory.DialectQuery.GetSelectColumnsList(table, table.Identifier);
            var c = columns.ToString();

            var qp = new SelectParser(expression);
            var whereSql = qp.GetSql();
            var param = qp.GetParameters();

            Assert.AreEqual("t_2.*", whereSql.Trim());
            Assert.AreEqual(0, param.Count());
        }
                       

        [Test]
        public void test_QueryBuilder_parser_with_join()
        { 
            var q = new QueryBuilder<Poco>();

            q
                .Select(t1 => new { t1.Active })
                .Join<Poco2>((t1, t2) => t1.Id == t2.Id)
                .Select((t1, t2) => new { t2.Id, t2.FullName })
                .Where((t1, t2) => t1.Active && t2.Id == 2);

            var stream = q.Parse();
            var sql = stream.GetSql();
            var param = stream.GetParameters();
             
            Assert.AreEqual("SELECT t_1.\"Active\"" +
                "\n\t, t_2.\"Id\", t_2.\"FullName\" " +
                "\nFROM poco t_1 " +
                "\nJOIN poco2 t_2 ON t_1.\"idPoco\" = t_2.\"Id\" " +
                "\nWHERE t_1.\"Active\" <> 0 AND (t_2.\"Id\" = @arg_0)", sql.Trim());
            Assert.AreEqual(1, param.Count());
            Assert.AreEqual(2, param["arg_0"]);
        }
    }
}