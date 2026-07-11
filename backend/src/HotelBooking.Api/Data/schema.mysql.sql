CREATE TABLE IF NOT EXISTS Users (
    Id INT NOT NULL AUTO_INCREMENT,
    FullName VARCHAR(200) NOT NULL,
    Email VARCHAR(200) NOT NULL,
    PasswordHash VARCHAR(500) NOT NULL,
    Role VARCHAR(30) NOT NULL,
    AvatarUrl VARCHAR(500) NOT NULL DEFAULT '',
    Balance DECIMAL(18, 2) NOT NULL DEFAULT 100000000.00,
    CreatedAt DATETIME(6) NOT NULL,
    CONSTRAINT PK_Users PRIMARY KEY (Id),
    CONSTRAINT UX_Users_Email UNIQUE (Email)
);

CREATE TABLE IF NOT EXISTS Hotels (
    Id INT NOT NULL AUTO_INCREMENT,
    Name VARCHAR(200) NOT NULL,
    City VARCHAR(100) NOT NULL,
    Address VARCHAR(500) NOT NULL,
    Description VARCHAR(2000) NOT NULL,
    StarRating INT NOT NULL,
    MainImageUrl VARCHAR(500) NOT NULL,
    OwnerUserId INT NULL,
    CONSTRAINT PK_Hotels PRIMARY KEY (Id),
    CONSTRAINT FK_Hotels_Users_OwnerUserId FOREIGN KEY (OwnerUserId) REFERENCES Users (Id) ON DELETE SET NULL
);

CREATE INDEX IX_Hotels_City ON Hotels (City);
CREATE INDEX IX_Hotels_OwnerUserId ON Hotels (OwnerUserId);

CREATE TABLE IF NOT EXISTS RoomTypes (
    Id INT NOT NULL AUTO_INCREMENT,
    HotelId INT NOT NULL,
    Name VARCHAR(100) NOT NULL,
    Description VARCHAR(2000) NOT NULL,
    MaxGuests INT NOT NULL,
    PricePerNight DECIMAL(18, 2) NOT NULL,
    TotalRooms INT NOT NULL,
    ImageUrl VARCHAR(500) NOT NULL,
    IsHidden BOOLEAN NOT NULL DEFAULT FALSE,
    CONSTRAINT PK_RoomTypes PRIMARY KEY (Id),
    CONSTRAINT FK_RoomTypes_Hotels_HotelId FOREIGN KEY (HotelId) REFERENCES Hotels (Id)
);

CREATE INDEX IX_RoomTypes_HotelId ON RoomTypes (HotelId);

CREATE TABLE IF NOT EXISTS Bookings (
    Id INT NOT NULL AUTO_INCREMENT,
    BookingCode VARCHAR(30) NOT NULL,
    RoomTypeId INT NOT NULL,
    UserId INT NULL,
    GuestName VARCHAR(200) NOT NULL,
    GuestEmail VARCHAR(200) NOT NULL,
    CheckIn DATE NOT NULL,
    CheckOut DATE NOT NULL,
    Guests INT NOT NULL,
    TotalPrice DECIMAL(18, 2) NOT NULL,
    Status VARCHAR(30) NOT NULL,
    PaymentStatus VARCHAR(30) NOT NULL,
    PaidAt DATETIME(6) NULL,
    CreatedAt DATETIME(6) NOT NULL,
    CONSTRAINT PK_Bookings PRIMARY KEY (Id),
    CONSTRAINT UX_Bookings_BookingCode UNIQUE (BookingCode),
    CONSTRAINT FK_Bookings_RoomTypes_RoomTypeId FOREIGN KEY (RoomTypeId) REFERENCES RoomTypes (Id),
    CONSTRAINT FK_Bookings_Users_UserId FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE SET NULL
);

CREATE INDEX IX_Bookings_RoomTypeId_CheckIn_CheckOut ON Bookings (RoomTypeId, CheckIn, CheckOut);
CREATE INDEX IX_Bookings_UserId ON Bookings (UserId);

CREATE TABLE IF NOT EXISTS HotelReviews (
    Id INT NOT NULL AUTO_INCREMENT,
    HotelId INT NOT NULL,
    BookingId INT NOT NULL,
    UserId INT NOT NULL,
    Rating INT NOT NULL,
    Comment VARCHAR(2000) NOT NULL,
    CreatedAt DATETIME(6) NOT NULL,
    CONSTRAINT PK_HotelReviews PRIMARY KEY (Id),
    CONSTRAINT UX_HotelReviews_BookingId UNIQUE (BookingId),
    CONSTRAINT FK_HotelReviews_Hotels_HotelId FOREIGN KEY (HotelId) REFERENCES Hotels (Id) ON DELETE CASCADE,
    CONSTRAINT FK_HotelReviews_Bookings_BookingId FOREIGN KEY (BookingId) REFERENCES Bookings (Id) ON DELETE CASCADE,
    CONSTRAINT FK_HotelReviews_Users_UserId FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE CASCADE
);

CREATE INDEX IX_HotelReviews_HotelId ON HotelReviews (HotelId);
CREATE INDEX IX_HotelReviews_UserId ON HotelReviews (UserId);

CREATE TABLE IF NOT EXISTS Notifications (
    Id INT NOT NULL AUTO_INCREMENT,
    UserId INT NOT NULL,
    Type VARCHAR(50) NOT NULL,
    Title VARCHAR(200) NOT NULL,
    Message VARCHAR(1000) NOT NULL,
    LinkUrl VARCHAR(500) NOT NULL,
    IsRead BOOLEAN NOT NULL DEFAULT FALSE,
    CreatedAt DATETIME(6) NOT NULL,
    CONSTRAINT PK_Notifications PRIMARY KEY (Id),
    CONSTRAINT FK_Notifications_Users_UserId FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE CASCADE
);

CREATE INDEX IX_Notifications_UserId_IsRead_CreatedAt ON Notifications (UserId, IsRead, CreatedAt);

CREATE TABLE IF NOT EXISTS BalanceTransactions (
    Id INT NOT NULL AUTO_INCREMENT,
    UserId INT NOT NULL,
    BookingId INT NULL,
    Amount DECIMAL(18, 2) NOT NULL,
    BalanceAfter DECIMAL(18, 2) NOT NULL,
    Type VARCHAR(50) NOT NULL,
    Description VARCHAR(1000) NOT NULL,
    CreatedAt DATETIME(6) NOT NULL,
    CONSTRAINT PK_BalanceTransactions PRIMARY KEY (Id),
    CONSTRAINT FK_BalanceTransactions_Users_UserId FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE CASCADE,
    CONSTRAINT FK_BalanceTransactions_Bookings_BookingId FOREIGN KEY (BookingId) REFERENCES Bookings (Id) ON DELETE SET NULL
);

CREATE INDEX IX_BalanceTransactions_UserId_CreatedAt ON BalanceTransactions (UserId, CreatedAt);
CREATE INDEX IX_BalanceTransactions_BookingId ON BalanceTransactions (BookingId);
