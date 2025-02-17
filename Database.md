# 数据库表

## 1. 用户表（Users）
| 字段名       | 数据类型        | 是否为空 | 主键 | 外键 | 默认值   | 说明                                           |
| ------------ | --------------- | -------- | ---- | ---- | -------- | ---------------------------------------------- |
| Id           | BIGINT UNSIGNED | 否       | 是   | 否   | 无       | 自增                                           |
| Username     | VARCHAR(255)    | 否       | 否   | 否   | 无       | 用户名，只能设置一次                           |
| Nickname     | TEXT            | 否       | 否   | 否   | 无       | 用户昵称                                       |
| Avatar       | VARCHAR(255)    | 是       | 否   | 否   | 无       | 用户头像                                       |
| Email        | VARCHAR(255)    | 否       | 否   | 否   | 无       | 用户邮箱                                       |
| PasswordHash | VARCHAR(255)    | 否       | 否   | 否   | 无       | 用户密码的哈希值，使用sha256加密，结果为16进制 |
| Role         | VARCHAR(20)     | 否       | 否   | 否   | 'user'   | 用户角色（admin/user）                         |
| CreatedAt    | DATETIME        | 否       | 否   | 否   | 当前时间 | 创建时间                                       |
| Status       | INT             | 否       | 否   | 否   | 0        | 0：未认证，1：已认证，2：封禁                  |

---

## 2. 帖子表（Posts）
| 字段名    | 数据类型        | 是否为空 | 主键 | 外键        | 默认值   | 说明             |
| --------- | --------------- | -------- | ---- | ----------- | -------- | ---------------- |
| Id        | BIGINT UNSIGNED | 否       | 是   | 否          | 无       | 自增             |
| UserId    | BIGINT UNSIGNED | 否       | 否   | 外键(Users) | 无       | 发帖用户 ID      |
| Title     | TEXT            | 否       | 否   | 否          | 无       | 帖子标题         |
| Content   | TEXT            | 否       | 否   | 否          | 无       | 帖子内容         |
| CreatedAt | DATETIME        | 否       | 否   | 否          | 当前时间 | 创建时间         |
| Status    | INT             | 否       | 否   | 否          | 0        | 0：正常，1：结束 |

---

## 3. 评论表（Comments）
| 字段名    | 数据类型        | 是否为空 | 主键 | 外键        | 默认值   | 说明        |
| --------- | --------------- | -------- | ---- | ----------- | -------- | ----------- |
| Id        | BIGINT UNSIGNED | 否       | 是   | 否          | 无       | 自增        |
| PostId    | BIGINT UNSIGNED | 否       | 否   | 外键(Posts) | 无       | 所属帖子 ID |
| UserId    | BIGINT UNSIGNED | 否       | 否   | 外键(Users) | 无       | 评论用户 ID |
| Content   | TEXT            | 否       | 否   | 否          | 无       | 评论内容    |
| CreatedAt | DATETIME        | 否       | 否   | 否          | 当前时间 | 创建时间    |

---

## 4. 点赞表（Likes）
| 字段名    | 数据类型        | 是否为空 | 主键 | 外键           | 默认值   | 说明          |
| --------- | --------------- | -------- | ---- | -------------- | -------- | ------------- |
| Id        | BIGINT UNSIGNED | 否       | 是   | 否             | 无       | 自增          |
| UserId    | BIGINT UNSIGNED | 否       | 否   | 外键(Users)    | 无       | 点赞用户 ID   |
| PostId    | BIGINT UNSIGNED | 是       | 否   | 外键(Posts)    | 无       | 点赞的帖子 ID |
| CommentId | BIGINT UNSIGNED | 是       | 否   | 外键(Comments) | 无       | 点赞的评论 ID |
| CreatedAt | DATETIME        | 否       | 否   | 否             | 当前时间 | 点赞时间      |

**备注**：
- 对于 `PostId` 和 `CommentId`，只有一个字段能有值，用于区分是给帖子点赞还是评论点赞。

---

## 5. 图片表（Images）
| 字段名    | 数据类型        | 是否为空 | 主键 | 外键        | 默认值   | 说明           |
| --------- | --------------- | -------- | ---- | ----------- | -------- | -------------- |
| Id        | BIGINT UNSIGNED | 否       | 是   | 否          | 无       | 自增           |
| PostId    | BIGINT UNSIGNED | 否       | 否   | 外键(Posts) | 无       | 所属帖子 ID    |
| Url       | TEXT            | 否       | 否   | 否          | 无       | 图片的存储路径 |
| CreatedAt | DATETIME        | 否       | 否   | 否          | 当前时间 | 创建时间       |

---

## 6. 分类表（Categories）
| 字段名      | 数据类型        | 是否为空 | 主键 | 外键 | 默认值 | 说明         |
| ----------- | --------------- | -------- | ---- | ---- | ------ | ------------ |
| Id          | BIGINT UNSIGNED | 否       | 是   | 否   | 自增   | 分类唯一标识 |
| Name        | VARCHAR(50)     | 否       | 否   | 否   | 无     | 分类名称     |
| Description | VARCHAR(255)    | 是       | 否   | 否   | 无     | 分类描述     |

---

## 7. 帖子分类关联表（PostCategories）
| 字段名     | 数据类型        | 是否为空 | 主键 | 外键             | 默认值 | 说明    |
| ---------- | --------------- | -------- | ---- | ---------------- | ------ | ------- |
| PostId     | BIGINT UNSIGNED | 否       | 是   | 外键(Posts)      | 无     | 帖子 ID |
| CategoryId | BIGINT UNSIGNED | 否       | 是   | 外键(Categories) | 无     | 分类 ID |

---

## 8. 收藏表（Favorites）
| 字段名    | 数据类型        | 是否为空 | 主键 | 外键        | 默认值   | 说明          |
| --------- | --------------- | -------- | ---- | ----------- | -------- | ------------- |
| Id        | BIGINT UNSIGNED | 否       | 是   | 否          | 无       | 自增          |
| UserId    | BIGINT UNSIGNED | 否       | 否   | 外键(Users) | 无       | 收藏用户 ID   |
| PostId    | BIGINT UNSIGNED | 否       | 否   | 外键(Posts) | 无       | 收藏的帖子 ID |
| CreatedAt | DATETIME        | 否       | 否   | 否          | 当前时间 | 收藏时间      |

## 9.消息表（Messages）

| 字段名     | 数据类型        | 是否为空 | 主键 | 外键        | 默认值   | 说明     |
| ---------- | --------------- | -------- | ---- | ----------- | -------- | -------- |
| Id         | BIGINT UNSIGNED | 否       | 是   | 否          | 无       | 自增     |
| SenderId   | BIGINT UNSIGNED | 否       | 否   | 外键(Users) | 无       | 发送者id |
| ReceiverId | BIGINT UNSIGNED | 否       | 否   | 外键(Users) | 无       | 接收者id |
| Content    | TEXT            | 否       | 否   | 否          | 无       | 消息内容 |
| CreatedAt  | DATETIME        | 否       | 否   | 否          | 当前时间 | 发送时间 |

---

## 外键约束
- `Likes.UserId` → `Users.Id`
- `Likes.PostId` → `Posts.Id`
- `Likes.CommentId` → `Comments.Id`
- `Posts.UserId` → `Users.Id`
- `Comments.PostId` → `Posts.Id`
- `Comments.UserId` → `Users.Id`
- `Images.PostId` → `Posts.Id`
- `PostCategories.PostId` → `Posts.Id`
- `PostCategories.CategoryId` → `Categories.Id`
- `Favorites.UserId` → `Users.Id`
- `Favorites.PostId` → `Posts.Id`
- `Messages.SenderId` → `Users.Id`
- `Messages.ReceiveId` → `Users.Id`
