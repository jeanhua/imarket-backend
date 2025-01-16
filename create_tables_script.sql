CREATE TABLE Users (
    Id CHAR(36) NOT NULL PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Nickname NVARCHAR(50) NOT NULL,
    Avatar NVARCHAR(255),
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(20) NOT NULL DEFAULT 'user',
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Status INT NOT NULL DEFAULT 0
);

CREATE TABLE Posts (
    Id CHAR(36) NOT NULL PRIMARY KEY,
    UserId CHAR(36) NOT NULL,
    Title NVARCHAR(100) NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Status INT NOT NULL DEFAULT 0,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

CREATE TABLE Comments (
    Id CHAR(36) NOT NULL PRIMARY KEY,
    PostId CHAR(36) NOT NULL,
    UserId CHAR(36) NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (PostId) REFERENCES Posts(Id),
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

CREATE TABLE Likes (
    Id CHAR(36) NOT NULL PRIMARY KEY,
    UserId CHAR(36) NOT NULL,
    PostId CHAR(36),
    CommentId CHAR(36),
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (PostId) REFERENCES Posts(Id),
    FOREIGN KEY (CommentId) REFERENCES Comments(Id)
);

CREATE TABLE Images (
    Id CHAR(36) NOT NULL PRIMARY KEY,
    PostId CHAR(36) NOT NULL,
    Url NVARCHAR(255) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (PostId) REFERENCES Posts(Id)
);

CREATE TABLE Categories (
    Id CHAR(36) NOT NULL AUTO_INCREMENT PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL,
    Description NVARCHAR(255)
);

CREATE TABLE PostCategories (
    PostId CHAR(36) NOT NULL,
    CategoryId CHAR(36) NOT NULL,
    PRIMARY KEY (PostId, CategoryId),
    FOREIGN KEY (PostId) REFERENCES Posts(Id),
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);

CREATE TABLE Favorites (
    Id CHAR(36) NOT NULL PRIMARY KEY,
    UserId CHAR(36) NOT NULL,
    PostId CHAR(36) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (PostId) REFERENCES Posts(Id)
);

CREATE TABLE Messages (
    Id CHAR(36) NOT NULL PRIMARY KEY,
    SenderId CHAR(36) NOT NULL,
    ReceiverId CHAR(36) NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (SenderId) REFERENCES Users(Id),
    FOREIGN KEY (ReceiveId) REFERENCES Users(Id)
);
