
# Description DeltaX.RestApiDemo1

This sample use Dapper, Sqlite and DeltaX.LinSql

## Running DeltaX.RestApiDemo1

Browser Swagger Specification

<img src="https://user-images.githubusercontent.com/2318691/104244197-b66a6c00-5440-11eb-8de2-08d56a113362.png" width="400"/>

## Project Objects

![imagen](https://user-images.githubusercontent.com/2318691/104244833-faaa3c00-5441-11eb-853d-057814e3c2e9.png)

### Project Detail

- Folder **Controllers**: Contain UserController. This is Entry point of Rest Api and also swagger (OpenApi) definitions.

- Folder **Dtos**: Contain all Poco/Dto classes used for entities (User, Role and UsersRoles) and helpers Poco classes (CreateUser, UpdateUser, CreateUsersRoles and RemoveUsersRoles)

- Folder **Repository**: Contain UserRepository when interface is it: 

<img src="https://user-images.githubusercontent.com/2318691/104245629-5cb77100-5443-11eb-84c4-f311e04b1d7d.png" height="200"/>

- Folder **SqliteHelper**: Contain DapperSqlite helper for DateTimeOffset mapper. Sql query for create Tables Users, Roles and UsersRoles. Configuration mapper between Sql Tables and Dtos.

- File **Startup.cs**: Contain classics Configuration and escentialy Dependency Injection configuration 


