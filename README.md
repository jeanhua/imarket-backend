# iMarket 校园集市后端

## 🔍 介绍

iMarket 是一个为校园环境设计的开源在线校园集市后端系统，旨在提供一个方便、快捷、安全的发帖平台，帮助学生发布、浏览、收藏和管理信息。该后端基于 .NET Web API 构建，配合 MySQL 数据库进行数据存储。项目采用模块化设计，支持高扩展性和可维护性。

## 📚 使用说明

### 1. 创建数据库

1. 确保已安装 MySQL 数据库，并运行以下命令以创建项目所需的数据库和表。

   连接数据库

   ```bash
   mysql -u root -p
   ```

   创建数据库

   ```
   CREATE DATABASE imarket;
   ```

   

2. 导入数据库初始化脚本（`create_tables_script.sql`），该脚本包含完整的表结构和外键约束。

   ```bash
   mysql -u root -p < create_tables_script.sql
   ```

### 2. 配置项目

1. 克隆代码仓库：
   ```bash
   git clone git@github.com:jeanhua/imarket_server.git
   cd imarket-backend
   ```

2. 修改配置文件：
   编辑 `appsettings.json`，填写数据库连接字符串，例如：
   ```json
   {
       "ConnectionStrings": {
           "DefaultConnection": "Server=localhost;Database=imarket;User=root;Password=yourPassword;"
       }
   }
   ```
   
   填写密钥和token有效时间 ( 默认60分钟 )
   
   ```json
   "JwtSettings": {
       "Key": "YourSuperSecretKeyHere",
       "Issuer": "imarket",
       "Audience": "web-client",
       "ExpiresInMinutes": 60
     }
   ```
   
   

### 3. 运行项目

#### 下载release版本，运行即可，支持参数`--port=端口`来指定端口

---

#### 或者运行代码

1. 安装依赖：
   ```bash
   dotnet restore
   ```

2. 运行项目：
   ```bash
   dotnet run
   ```

3. 后端 API 默认运行在 `https://localhost:5001`。

### 4. 测试接口

1. 使用 Postman 或其他 API 测试工具导入项目的 API 文档（例如：Swagger 文件）。
2. 根据 API 文档测试后端接口功能，例如用户注册、登录、发帖、评论等。

## 📂 项目结构

- `Controllers`：控制器层，处理客户端请求并返回响应。

- `Models`：数据模型层，定义数据库实体和相关业务逻辑。

- `Services`：服务层，包含核心业务逻辑。

- `utils`：存放一些辅助函数

- `wwwroot`：存放前端静态网页

  ---

- [Database.md](./Database.md) 数据库表结构

## 🌟 功能模块

1. **用户管理**：用户注册、登录、资料修改。
2. **帖子管理**：发帖、结束、删除、查看帖子详情。
3. **评论功能**：对帖子发表评论。
4. **点赞与收藏**：支持用户对帖子和评论的点赞与收藏。
5. **分类系统**：支持为帖子分配分类。
6. **图片管理**：支持帖子图片上传与管理。

## 🛠️ 技术栈

- 后端框架：.NET 8
- 数据库：MySQL
- 依赖管理：NuGet

## 📝 贡献指南

1. Fork 本仓库。
2. 创建一个新分支：`git checkout -b feature/your-feature-name`。
3. 提交更改：`git commit -m 'Add your message here'`。
4. 推送到远程分支：`git push origin feature/your-feature-name`。
5. 提交 Pull Request。

## 📞 联系方式

如有问题或建议，请通过以下方式联系我：

- 邮箱：jeanhua_official@outlook.com
- 项目主页：https://github.com/jeanhua/imarket-backend

---
感谢您的支持和贡献！