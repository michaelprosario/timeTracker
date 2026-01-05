-- Migration: 20260105051257_InitialCreate
-- Description: Initial database schema creation for Time Tracker application
-- Target Database: PostgreSQL
-- Date: 2026-01-05

-- Create Projects table
CREATE TABLE "Projects" (
    "Code" VARCHAR(50) NOT NULL,
    "Name" VARCHAR(200) NOT NULL,
    "Description" VARCHAR(1000) NOT NULL,
    "IsActive" BOOLEAN NOT NULL,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    CONSTRAINT "PK_Projects" PRIMARY KEY ("Code")
);

-- Create Users table
CREATE TABLE "Users" (
    "Id" UUID NOT NULL,
    "Email" VARCHAR(255) NOT NULL,
    "PasswordHash" VARCHAR(500) NOT NULL,
    "FirstName" VARCHAR(100) NOT NULL,
    "LastName" VARCHAR(100) NOT NULL,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    "IsActive" BOOLEAN NOT NULL,
    CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
);

-- Create WorkTypes table
CREATE TABLE "WorkTypes" (
    "Code" VARCHAR(50) NOT NULL,
    "Name" VARCHAR(200) NOT NULL,
    "Description" VARCHAR(1000) NOT NULL,
    "IsActive" BOOLEAN NOT NULL,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    CONSTRAINT "PK_WorkTypes" PRIMARY KEY ("Code")
);

-- Create TimeSheets table
CREATE TABLE "TimeSheets" (
    "Id" UUID NOT NULL,
    "UserId" UUID NOT NULL,
    "StartDate" TIMESTAMP WITH TIME ZONE NOT NULL,
    "EndDate" TIMESTAMP WITH TIME ZONE NOT NULL,
    "Status" INTEGER NOT NULL,
    "TotalHours" NUMERIC(18,2) NOT NULL,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    "LastModifiedAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    CONSTRAINT "PK_TimeSheets" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_TimeSheets_Users_UserId" FOREIGN KEY ("UserId") 
        REFERENCES "Users" ("Id") ON DELETE CASCADE
);

-- Create TimeEntries table
CREATE TABLE "TimeEntries" (
    "Id" UUID NOT NULL,
    "TimeSheetId" UUID NOT NULL,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    "CreatedBy" UUID NOT NULL,
    "ProjectCode" VARCHAR(50) NOT NULL,
    "WorkTypeCode" VARCHAR(50) NOT NULL,
    "StartTime" TIMESTAMP WITH TIME ZONE NULL,
    "EndTime" TIMESTAMP WITH TIME ZONE NULL,
    "Hours" NUMERIC(18,2) NOT NULL,
    "EntryDate" TIMESTAMP WITH TIME ZONE NOT NULL,
    "Notes" VARCHAR(1000) NOT NULL,
    "LastModifiedAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    CONSTRAINT "PK_TimeEntries" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_TimeEntries_Projects_ProjectCode" FOREIGN KEY ("ProjectCode") 
        REFERENCES "Projects" ("Code") ON DELETE RESTRICT,
    CONSTRAINT "FK_TimeEntries_TimeSheets_TimeSheetId" FOREIGN KEY ("TimeSheetId") 
        REFERENCES "TimeSheets" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_TimeEntries_WorkTypes_WorkTypeCode" FOREIGN KEY ("WorkTypeCode") 
        REFERENCES "WorkTypes" ("Code") ON DELETE RESTRICT
);

-- Insert seed data for Projects
INSERT INTO "Projects" ("Code", "CreatedAt", "Description", "IsActive", "Name")
VALUES 
    ('INTERNAL', '2026-01-01 00:00:00+00', 'Internal company tasks', true, 'Internal Tasks'),
    ('PROJECT-A', '2026-01-01 00:00:00+00', 'Main product development', true, 'Project Alpha'),
    ('TRAINING', '2026-01-01 00:00:00+00', 'Learning and training activities', true, 'Training & Development');

-- Insert seed data for WorkTypes
INSERT INTO "WorkTypes" ("Code", "CreatedAt", "Description", "IsActive", "Name")
VALUES 
    ('ADMIN', '2026-01-01 00:00:00+00', 'Administrative tasks', true, 'Administration'),
    ('DEV', '2026-01-01 00:00:00+00', 'Software development work', true, 'Development'),
    ('MEET', '2026-01-01 00:00:00+00', 'Meetings and discussions', true, 'Meetings'),
    ('SUPPORT', '2026-01-01 00:00:00+00', 'Customer support', true, 'Support'),
    ('DEVOPS', '2026-01-01 00:00:00+00', 'DevOps and infrastructure management', true, 'DevOps'),
    ('TEST', '2026-01-01 00:00:00+00', 'Quality assurance and testing', true, 'Testing'),
    ('TRAIN', '2026-01-01 00:00:00+00', 'Training and learning', true, 'Training');

-- Create indexes
CREATE INDEX "IX_TimeEntries_EntryDate" ON "TimeEntries" ("EntryDate");
CREATE INDEX "IX_TimeEntries_ProjectCode" ON "TimeEntries" ("ProjectCode");
CREATE INDEX "IX_TimeEntries_TimeSheetId" ON "TimeEntries" ("TimeSheetId");
CREATE INDEX "IX_TimeEntries_WorkTypeCode" ON "TimeEntries" ("WorkTypeCode");
CREATE INDEX "IX_TimeSheets_UserId_StartDate" ON "TimeSheets" ("UserId", "StartDate");
CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");
