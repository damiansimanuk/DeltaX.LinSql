using DeltaX.LinSql.Table;
using NUnit.Framework;
using System;
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

    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        public void ConfigureTalbes()
        {
            var factory = TableQueryFactory.GetInstance();

            factory.ConfigureTable<Poco>("poco", cfg =>
            {
                cfg.AddColumn(c => c.Id, "idPoco", true, true);
                cfg.AddColumn(c => c.Name);
                cfg.AddColumn(c => c.Updated, p => { p.IgnoreInsert = true; p.IgnoreUpdate = true; });
                cfg.AddColumn(c => c.Active);
            });
        }

        [Test]
        public void TestQuerySimpleConstant()
        {
            ConfigureTalbes();

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
            ConfigureTalbes();
            var updated = DateTime.Now;

            Expression<Func<Poco, bool>> expression = t => t.Id == 2 && t.Updated < updated;

            var qp = new QueryParser(expression);
            var whereSql = qp.GetSql();
            var param = qp.GetParameters();
            var tableColumns = qp.GetTableColumns().ToList();

            Assert.AreEqual("t_1.\"idPoco\" = @arg_0 AND t_1.\"Updated\" < @arg_1", whereSql);
            Assert.AreEqual(2, tableColumns[0].Value.Count());
            Assert.AreEqual("poco", tableColumns[0].Key.Name);

            Assert.AreEqual(2, param.Count());
            Assert.IsTrue(param.ContainsKey("arg_0"));
            Assert.AreEqual(2, param["arg_0"]);
            Assert.AreEqual(updated, param["arg_1"]);
        
            Assert.Pass();
        }

    }
}