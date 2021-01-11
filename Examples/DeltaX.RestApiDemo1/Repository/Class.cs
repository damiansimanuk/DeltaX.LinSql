using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace DeltaX.RestApiDemo1.Repository
{
	public class TableCrator
	{
		public static readonly string ScriptCreateTables = @"
PRAGMA foreign_keys = off;
BEGIN TRANSACTION;
 
CREATE TABLE IF NOT EXISTS Users(
    Id           INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    Username     TEXT    UNIQUE NOT NULL,
    FullName     TEXT,
    Email        TEXT    UNIQUE, 
    Active       BOOLEAN DEFAULT (1) NOT NULL,
    PasswordHash TEXT,
    CreatedAt    DATE    DEFAULT (datetime('now', 'localtime') ) 
);

CREATE TABLE IF NOT EXISTS Roles (
    Id        INTEGER PRIMARY KEY AUTOINCREMENT,
    Name      TEXT    UNIQUE NOT NULL,
    CreatedAt DATE    DEFAULT (datetime('now', 'localtime') ) 
);

CREATE TABLE IF NOT EXISTS UsersRoles (
    UserId		INTEGER REFERENCES Users (Id) ON DELETE CASCADE,
    RolId		INTEGER REFERENCES Roles (Id) ON DELETE CASCADE,
    Create		BOOLEAN DEFAULT (0),
    Read		BOOLEAN DEFAULT (1),
    Update		BOOLEAN DEFAULT (0),
    Delete		BOOLEAN DEFAULT (0),
    CreatedAt DATE    DEFAULT (datetime('now', 'localtime') ),
    UNIQUE (UserId, RolId)
);

COMMIT TRANSACTION;
PRAGMA foreign_keys = on;
";

		private IDbConnection connection;
		private ILogger log;

		public TableCrator(IDbConnection connection, ILogger log = null)
		{
			this.connection = connection;
			this.log = log;
		}

		public void Start()
		{
			log?.LogInformation("Executing CreateDatabase Script...");

			using (var objCommand = ((SqliteConnection)connection).CreateCommand())
			{
				objCommand.CommandText = ScriptCreateTables;
				var result = objCommand.ExecuteNonQuery();
				log?.LogInformation("CreateDatabase Execute result {result}", result);
			}
		}
	}
}