using NUnit.Framework;
using System;

namespace DeltaX.LinSql.Table.Unitest
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
        }

        [Test]
        public void TestInsert()
        {
            ConfigureTalbes();
            var factory = TableQueryFactory.GetInstance();

            var sql = factory.GetInsertQuery<Poco>();

            Assert.AreEqual("INSERT INTO poco \n\t(\"Name\", \"Active\") VALUES\n\t(@Name, @Active)", sql.Trim());
        }

        [Test]
        public void TestUpdate()
        {
            ConfigureTalbes();
            var factory = TableQueryFactory.GetInstance();

            var sql = factory.GetUpdateQuery<Poco>();

            Assert.AreEqual("UPDATE poco SET\n\t \"Name\" = @Name\n\t, \"Active\" = @Active \nWHERE \"idPoco\" = @Id", sql.Trim());
        }


        [Test]
        public void TestUpdateWithArg()
        {
            ConfigureTalbes();
            var factory = TableQueryFactory.GetInstance();

            var sql = factory.GetUpdateQuery<Poco>("WHERE \"idPoco\" = @IdPoco", new[] {"Name" });

            Assert.AreEqual("UPDATE poco SET\n\t \"Name\" = @Name \nWHERE \"idPoco\" = @IdPoco", sql.Trim());
        }


        [Test]
        public void TestSelect()
        {
            ConfigureTalbes();
            var factory = TableQueryFactory.GetInstance();

            var sql = factory.GetSingleQuery<Poco>( );

            Assert.AreEqual("SELECT \n\tt_1.\"idPoco\" as \"Id\"\n\t, t_1.\"Name\"\n\t, t_1.\"Updated\"\n\t, t_1.\"Active\" \nFROM poco t_1 \nWHERE t_1.\"idPoco\" = @Id", sql.Trim());
        }
    }
}