USE master;
GO

IF DB_ID('RaceDayDb') IS NOT NULL
BEGIN
    ALTER DATABASE RaceDayDb
    SET SINGLE_USER
    WITH ROLLBACK IMMEDIATE;

    DROP DATABASE RaceDayDb;
END;
GO

CREATE DATABASE RaceDayDb;
GO

USE RaceDayDb;
GO

USE master;
GO

IF DB_ID('RaceDayDb') IS NOT NULL
BEGIN
    ALTER DATABASE RaceDayDb
    SET SINGLE_USER
    WITH ROLLBACK IMMEDIATE;

    DROP DATABASE RaceDayDb;
END;
GO

CREATE DATABASE RaceDayDb;
GO

USE RaceDayDb;
GO

SELECT DB_NAME() AS CurrentDatabase;

SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE';

CREATE TABLE Users
(
    UserId INT IDENTITY(1,1) NOT NULL,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Email NVARCHAR(120) NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(20) NOT NULL,

    CreatedAt DATETIME2 NOT NULL
        CONSTRAINT DF_Users_CreatedAt
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Users
        PRIMARY KEY (UserId),

    CONSTRAINT UQ_Users_Email
        UNIQUE (Email),

    CONSTRAINT CK_Users_Role
        CHECK (Role IN ('Organiser', 'Participant'))
);
GO

SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE';

CREATE TABLE Events
(
    EventId INT IDENTITY(1,1) NOT NULL,
    OrganiserId INT NOT NULL,
    EventName NVARCHAR(150) NOT NULL,
    Description NVARCHAR(1000) NULL,
    EventDate DATE NOT NULL,
    Venue NVARCHAR(200) NOT NULL,
    Province NVARCHAR(80) NOT NULL,

    Status NVARCHAR(20) NOT NULL
        CONSTRAINT DF_Events_Status
        DEFAULT 'Upcoming',

    CreatedAt DATETIME2 NOT NULL
        CONSTRAINT DF_Events_CreatedAt
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Events
        PRIMARY KEY (EventId),

    CONSTRAINT FK_Events_Organiser
        FOREIGN KEY (OrganiserId)
        REFERENCES Users(UserId),

    CONSTRAINT CK_Events_Status
        CHECK (Status IN
        ('Upcoming', 'Completed', 'Cancelled'))
);
GO

CREATE TABLE Categories
(
    CategoryId INT IDENTITY(1,1) NOT NULL,
    EventId INT NOT NULL,
    CategoryName NVARCHAR(100) NOT NULL,
    DistanceKm DECIMAL(6,2) NOT NULL,
    MaxParticipants INT NOT NULL,
    EntryFee DECIMAL(10,2) NOT NULL,

    CONSTRAINT PK_Categories
        PRIMARY KEY (CategoryId),

    CONSTRAINT FK_Categories_Event
        FOREIGN KEY (EventId)
        REFERENCES Events(EventId)
        ON DELETE CASCADE,

    CONSTRAINT UQ_Categories_Event_Name
        UNIQUE (EventId, CategoryName),

    CONSTRAINT CK_Categories_Distance
        CHECK (DistanceKm > 0),

    CONSTRAINT CK_Categories_MaxParticipants
        CHECK (MaxParticipants > 0),

    CONSTRAINT CK_Categories_EntryFee
        CHECK (EntryFee >= 0)
);
GO

CREATE TABLE Routes
(
    RouteId INT IDENTITY(1,1) NOT NULL,
    EventId INT NOT NULL,
    RouteName NVARCHAR(150) NOT NULL,
    DistanceKm DECIMAL(6,2) NOT NULL,
    StartLocation NVARCHAR(200) NOT NULL,
    FinishLocation NVARCHAR(200) NOT NULL,
    RouteDescription NVARCHAR(1000) NULL,
    MapUrl NVARCHAR(500) NULL,

    CONSTRAINT PK_Routes
        PRIMARY KEY (RouteId),

    CONSTRAINT FK_Routes_Event
        FOREIGN KEY (EventId)
        REFERENCES Events(EventId)
        ON DELETE CASCADE,

    CONSTRAINT CK_Routes_Distance
        CHECK (DistanceKm > 0)
);
GO

CREATE TABLE Enrollments
(
    EnrollmentId INT IDENTITY(1,1) NOT NULL,
    ParticipantId INT NOT NULL,
    CategoryId INT NOT NULL,

    EnrollmentDate DATETIME2 NOT NULL
        CONSTRAINT DF_Enrollments_EnrollmentDate
        DEFAULT SYSUTCDATETIME(),

    Status NVARCHAR(20) NOT NULL
        CONSTRAINT DF_Enrollments_Status
        DEFAULT 'Confirmed',

    BibNumber INT NULL,

    CONSTRAINT PK_Enrollments
        PRIMARY KEY (EnrollmentId),

    CONSTRAINT FK_Enrollments_Participant
        FOREIGN KEY (ParticipantId)
        REFERENCES Users(UserId),

    CONSTRAINT FK_Enrollments_Category
        FOREIGN KEY (CategoryId)
        REFERENCES Categories(CategoryId),

    CONSTRAINT UQ_Enrollments_Participant_Category
        UNIQUE (ParticipantId, CategoryId),

    CONSTRAINT CK_Enrollments_Status
        CHECK (Status IN ('Confirmed', 'Cancelled')),

    CONSTRAINT CK_Enrollments_BibNumber
        CHECK (BibNumber IS NULL OR BibNumber > 0)
);
GO

CREATE TABLE Results
(
    ResultId INT IDENTITY(1,1) NOT NULL,
    EnrollmentId INT NOT NULL,
    FinishTime TIME(0) NOT NULL,
    Position INT NOT NULL,
    PaceMinutesPerKm DECIMAL(6,2) NULL,

    ResultDate DATETIME2 NOT NULL
        CONSTRAINT DF_Results_ResultDate
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Results
        PRIMARY KEY (ResultId),

    CONSTRAINT FK_Results_Enrollment
        FOREIGN KEY (EnrollmentId)
        REFERENCES Enrollments(EnrollmentId)
        ON DELETE CASCADE,

    CONSTRAINT UQ_Results_Enrollment
        UNIQUE (EnrollmentId),

    CONSTRAINT CK_Results_Position
        CHECK (Position > 0),

    CONSTRAINT CK_Results_Pace
        CHECK (PaceMinutesPerKm IS NULL OR PaceMinutesPerKm > 0)
);
GO