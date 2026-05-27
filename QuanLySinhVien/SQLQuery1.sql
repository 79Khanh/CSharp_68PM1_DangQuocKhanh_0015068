USE master;
GO

IF DB_ID(N'QLSV') IS NULL
BEGIN
    CREATE DATABASE QLSV;
END;
GO

USE QLSV;
GO

IF OBJECT_ID(N'dbo.Classrooms', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Classrooms
    (
        Id INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_Classrooms PRIMARY KEY,
        ClassCode NVARCHAR(50) NOT NULL CONSTRAINT UQ_Classrooms_ClassCode UNIQUE,
        ClassName NVARCHAR(255) NOT NULL,
        Notes NVARCHAR(MAX) NULL
    );
END;
GO

IF OBJECT_ID(N'dbo.Students', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Students
    (
        Id INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_Students PRIMARY KEY,
        StudentCode NVARCHAR(50) NOT NULL CONSTRAINT UQ_Students_StudentCode UNIQUE,
        FullName NVARCHAR(255) NOT NULL,
        BirthDate DATE NULL,
        Gender NVARCHAR(10) NULL,
        ClassId INT NOT NULL,
        Notes NVARCHAR(MAX) NULL,
        CONSTRAINT FK_Students_Classrooms
            FOREIGN KEY (ClassId) REFERENCES dbo.Classrooms(Id)
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Classrooms WHERE ClassCode = N'68PM1')
BEGIN
    INSERT INTO dbo.Classrooms (ClassCode, ClassName, Notes)
    VALUES (N'68PM1', N'Phần mềm 1 - K68', N'Lớp dữ liệu mẫu');
END;
ELSE
BEGIN
    UPDATE dbo.Classrooms
    SET ClassName = N'Phần mềm 1 - K68',
        Notes = N'Lớp dữ liệu mẫu'
    WHERE ClassCode = N'68PM1';
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Students WHERE StudentCode = N'0015068')
BEGIN
    INSERT INTO dbo.Students (StudentCode, FullName, BirthDate, Gender, ClassId, Notes)
    SELECT N'0015068', N'Đặng Quốc Khánh', '2005-01-01', N'Nam', Id, N'Dữ liệu mẫu'
    FROM dbo.Classrooms
    WHERE ClassCode = N'68PM1';
END;
ELSE
BEGIN
    UPDATE dbo.Students
    SET FullName = N'Đặng Quốc Khánh',
        BirthDate = '2005-01-01',
        Gender = N'Nam',
        ClassId = (SELECT Id FROM dbo.Classrooms WHERE ClassCode = N'68PM1'),
        Notes = N'Dữ liệu mẫu'
    WHERE StudentCode = N'0015068';
END;
GO
