-- 1. 用户表（Users）
CREATE TABLE IF NOT EXISTS Users (
    Id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    Username TEXT NOT NULL,
    Nickname TEXT NOT NULL,
    Avatar TEXT,
    Email TEXT NOT NULL,
    PasswordHash TEXT NOT NULL,
    Role TEXT NOT NULL DEFAULT 'user',
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Status INT NOT NULL DEFAULT 0,
    PRIMARY KEY (Id),
    UNIQUE (Username),
    UNIQUE (Email)
);

-- 2. 帖子表（Posts）
CREATE TABLE IF NOT EXISTS Posts (
    Id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    UserId BIGINT UNSIGNED NOT NULL,
    Title TEXT NOT NULL,
    Content TEXT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Status INT NOT NULL DEFAULT 0,
    PRIMARY KEY (Id),
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- 3. 评论表（Comments）
CREATE TABLE IF NOT EXISTS Comments (
    Id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    PostId BIGINT UNSIGNED NOT NULL,
    UserId BIGINT UNSIGNED NOT NULL,
    Content TEXT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (Id),
    FOREIGN KEY (PostId) REFERENCES Posts(Id),
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- 4. 点赞表（Likes）
CREATE TABLE IF NOT EXISTS Likes (
    Id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    UserId BIGINT UNSIGNED NOT NULL,
    PostId BIGINT UNSIGNED,
    CommentId BIGINT UNSIGNED,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (Id),
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (PostId) REFERENCES Posts(Id),
    FOREIGN KEY (CommentId) REFERENCES Comments(Id),
    CHECK (PostId IS NOT NULL OR CommentId IS NOT NULL)
);

-- 5. 图片表（Images）
CREATE TABLE IF NOT EXISTS Images (
    Id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    PostId BIGINT UNSIGNED NOT NULL,
    Url TEXT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (Id),
    FOREIGN KEY (PostId) REFERENCES Posts(Id)
);

-- 6. 分类表（Categories）
CREATE TABLE IF NOT EXISTS Categories (
    Id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    Name TEXT NOT NULL,
    Description TEXT,
    PRIMARY KEY (Id),
    UNIQUE (Name)
);

-- 7. 帖子分类关联表（PostCategories）
CREATE TABLE IF NOT EXISTS PostCategories (
    PostId BIGINT UNSIGNED NOT NULL,
    CategoryId BIGINT UNSIGNED NOT NULL,
    PRIMARY KEY (PostId, CategoryId),
    FOREIGN KEY (PostId) REFERENCES Posts(Id),
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);

-- 8. 收藏表（Favorites）
CREATE TABLE IF NOT EXISTS Favorites (
    Id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    UserId BIGINT UNSIGNED NOT NULL,
    PostId BIGINT UNSIGNED NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (Id),
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (PostId) REFERENCES Posts(Id)
);

-- 9. 消息表（Messages）
CREATE TABLE IF NOT EXISTS Messages (
    Id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    SenderId BIGINT UNSIGNED NOT NULL,
    ReceiverId BIGINT UNSIGNED NOT NULL,
    Content TEXT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (Id),
    FOREIGN KEY (SenderId) REFERENCES Users(Id),
    FOREIGN KEY (ReceiverId) REFERENCES Users(Id)
);