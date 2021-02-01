﻿using DeltaX.LinSql.Table;
using DeltaX.RestApiDemo1.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeltaX.RestApiDemo1
{
    public class DemoTableQueryFactory : TableQueryFactory
    {
        public DemoTableQueryFactory() : base(DialectType.SQLite)
        {

            this.ConfigureTable<UserModel>("Users", cfg =>
            {
                cfg.Identifier = "u";
                cfg.AddColumn(c => c.Id, null, true, true);
                cfg.AddColumn(c => c.Username);
                cfg.AddColumn(c => c.FullName);
                cfg.AddColumn(c => c.Email);
                cfg.AddColumn(c => c.Image);
                cfg.AddColumn(c => c.Active);
                cfg.AddColumn(c => c.CreatedAt, p => { p.IgnoreInsert = true; p.IgnoreUpdate = true; });
            });

            ConfigureTable<RoleModel>("Roles", cfg =>
            {
                cfg.Identifier = "r";
                cfg.AddColumn(c => c.Id, null, true, true);
                cfg.AddColumn(c => c.RolName, "Name");
            });

            ConfigureTable<UsersRolesModel>("UsersRoles", cfg =>
            {
                cfg.Identifier = "ur";
                cfg.AddColumn(c => c.UserId, null, false, true);
                cfg.AddColumn(c => c.RolId, null, false, true);
                cfg.AddColumn(c => c.Create, "C");
                cfg.AddColumn(c => c.Read, "R");
                cfg.AddColumn(c => c.Update, "U");
                cfg.AddColumn(c => c.Delete, "D");
                cfg.AddColumn(c => c.CreatedAt, p => { p.IgnoreInsert = true; p.IgnoreUpdate = true; });
            });
        }
    }
}
