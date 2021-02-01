using Dapper;
using DeltaX.LinSql.Query;
using DeltaX.LinSql.Table;
using DeltaX.RestApiDemo1.Dtos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Transactions;

namespace DeltaX.RestApiDemo1.Repository
{
    public class UserRepository : IUserRepository
    {
        IDbConnection db;
        private readonly DemoTableQueryFactory queryFactory;
        private readonly ILogger<UserRepository> logger;

        public UserRepository(IDbConnection db, DemoTableQueryFactory queryFactory, ILogger<UserRepository> logger = null)
        {
            this.db = db;
            this.queryFactory = queryFactory;
            this.logger = logger;
        }

        public async Task<IEnumerable<UserListDto>> GetUsersAsync()
        {
            (var sql, var param) = new QueryBuilder<UserModel>()
                .SelectAll()
                .GetSqlParameters(queryFactory);

            var users = await db.QueryAsync<UserListDto>(sql, param);
            return users;
        }

        public async Task<UserModel> GetUserAsync(int id)
        {  
            var user = await GetSingleAsync<UserModel>(u => u.Id == id);
            if (user != null)
            {
                (var sql, var param) = new QueryBuilder<UsersRolesModel>()
                    .Join<RoleModel>((ur, r) => ur.RolId == r.Id)
                    .SelectAll()
                    .Where(ur => ur.UserId == id)
                    .GetSqlParameters(queryFactory);

                var roles = await db.QueryAsync<UsersRolesModel>(sql, param);
                user.Roles = roles.ToArray();
            }
            return user;
        }

        public async Task<int> RemoveUserAsync(int id)
        {
            using (var transactionScope = new TransactionScope())
            { 
                (var sql, var param) = new QueryBuilder<UsersRolesModel>()
                    .Join<UserModel>((ur, u) => ur.UserId == u.Id)
                    .Where((ur, u) => u.Id == id)
                    .Delete()
                    .GetSqlParameters(queryFactory);

                var affectedRows = await db.ExecuteAsync(sql, param);
                 
                (sql, param) = new QueryBuilder<UserModel>()
                    .Where(u => u.Id == id)
                    .Delete()
                    .GetSqlParameters(queryFactory);

                affectedRows += await db.ExecuteAsync(sql, param);
                return affectedRows;
            }
        }

        public async Task<UserModel> InsertUserAsync(CreateUserDto item)
        {
            using (var transactionScope = new TransactionScope())
            {
                var userId = await InsertAsync<UserModel, int>(new UserModel
                {
                    Username = item.Username,
                    FullName = item.FullName,
                    Email = item.Email,
                    Active = true
                });

                if (item.Roles?.Any() == true)
                {
                    InsertUsersRolesAsync(userId, item.Roles);
                }
                return await GetUserAsync(userId);
            }            
        }

        public async Task<UserModel> UpdateUserAsync(int userId, UpdateUserDto item)
        {
            var user = await GetUserAsync(userId);
            user.FullName = item.FullName ?? user.FullName;
            user.Email = item.Email ?? user.Email;
            user.Image = item.Image ?? user.Image;
            user.Active = item.Active ?? user.Active;

            using (var transactionScope = new TransactionScope())
            {
                var query = queryFactory.GetUpdateQuery<UserModel>();
                await db.ExecuteAsync(query, user);

                if (item.AddRoles?.Any() == true)
                {
                    InsertUsersRolesAsync(userId, item.AddRoles);
                }

                if (item.RemoveRoles?.Any() == true)
                {
                    RemoveUsersRolesAsync(userId, item.RemoveRoles.Select(i => i.RolName).ToArray());
                }
                return await GetUserAsync(userId);
            }
        }

        public async void InsertUsersRolesAsync(int userId, IEnumerable<CreateUsersRolesDto> roles)
        {
            foreach (var rol in roles)
            {
                var rolEntity = await GetSingleAsync<RoleModel>(r => r.RolName == rol.RolName);
                var rolId = rolEntity?.Id 
                    ?? await InsertAsync<RoleModel, int>(new RoleModel { RolName = rol.RolName });

                await InsertAsync<UsersRolesModel>(new UsersRolesModel
                {
                    UserId = userId,
                    RolId = rolId,
                    Create = rol.Create,
                    Read = rol.Read,
                    Update = rol.Update,
                    Delete = rol.Delete
                });
            }
        }

        public async void RemoveUsersRolesAsync(int userId, string[] roleNames)
        {
            (var sql, var param) = new QueryBuilder<UsersRolesModel>()
                .Join<RoleModel>((ur, r) => ur.RolId == r.Id)
                .Where((ur, r) => ur.UserId == userId && roleNames.Contains(r.RolName))
                .Delete()
                .GetSqlParameters(queryFactory);

            await db.ExecuteAsync(sql, param);
        }


        private async Task<TEntity> GetSingleAsync<TEntity>(Expression<Func<TEntity, bool>> predicate)
             where TEntity : class
        {
            (var sql, var param) = new QueryBuilder<TEntity>()
                .Where(predicate)
                .SelectAll()
                .GetSqlParameters(queryFactory);

            return await db.QueryFirstOrDefaultAsync<TEntity>(sql, param);
        }

        private Task<int> InsertAsync<TEntity>(TEntity item, IEnumerable<string> fieldsToInsert = null)
           where TEntity : class
        {
            var query = queryFactory.GetInsertQuery<TEntity>(fieldsToInsert); 
            logger?.LogDebug("InsertAsync query:{query} item:{@item}", query, item);

            return db.ExecuteAsync(query, item);
        }

        private Task<Tkey> InsertAsync<TEntity, Tkey>(TEntity item, IEnumerable<string> fieldsToInsert = null)
           where TEntity : class
        {
            var query = queryFactory.GetInsertQuery<TEntity>(fieldsToInsert);
            query += "; " + queryFactory.DialectQuery.IdentityQueryFormatSql;
            logger?.LogDebug("InsertAsync query:{query} item:{@item}", query, item);

            return db.ExecuteScalarAsync<Tkey>(query, item);
        } 
    }
}