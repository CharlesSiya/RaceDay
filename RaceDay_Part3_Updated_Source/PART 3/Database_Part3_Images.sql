/* RaceDay Part 3 schema extension.
   Run this once against the existing RaceDayDB database after Part 2.
   It adds the Azure Blob Storage URLs returned by the API upload endpoints.
*/
IF COL_LENGTH('dbo.Users', 'ProfileImageUrl') IS NULL
    ALTER TABLE dbo.Users ADD ProfileImageUrl NVARCHAR(1000) NULL;
GO

IF COL_LENGTH('dbo.Events', 'BannerImageUrl') IS NULL
    ALTER TABLE dbo.Events ADD BannerImageUrl NVARCHAR(1000) NULL;
GO
