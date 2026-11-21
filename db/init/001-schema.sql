-- TaskFlow initial PostgreSQL schema. Mounted by docker-compose on first volume initialization.
CREATE TABLE IF NOT EXISTS "Users" (
  "Id" uuid PRIMARY KEY,
  "Name" varchar(120) NOT NULL,
  "Email" varchar(254) NOT NULL UNIQUE,
  "PasswordHash" text NOT NULL,
  "Role" varchar(24) NOT NULL DEFAULT 'Member' CHECK ("Role" IN ('Admin', 'Manager', 'Member')),
  "CreatedAt" timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS "Projects" (
  "Id" uuid PRIMARY KEY,
  "Name" varchar(160) NOT NULL,
  "Description" varchar(4000) NOT NULL DEFAULT '',
  "OwnerId" uuid NOT NULL REFERENCES "Users"("Id") ON DELETE RESTRICT,
  "Status" varchar(24) NOT NULL DEFAULT 'Active' CHECK ("Status" IN ('Active', 'Archived')),
  "StartDate" date NULL,
  "DueDate" date NULL,
  "CreatedAt" timestamptz NOT NULL DEFAULT now(),
  "UpdatedAt" timestamptz NOT NULL DEFAULT now(),
  CHECK ("StartDate" IS NULL OR "DueDate" IS NULL OR "DueDate" >= "StartDate")
);
CREATE INDEX IF NOT EXISTS "IX_Projects_OwnerId_Status" ON "Projects"("OwnerId", "Status");

CREATE TABLE IF NOT EXISTS "ProjectMembers" (
  "ProjectId" uuid NOT NULL REFERENCES "Projects"("Id") ON DELETE CASCADE,
  "UserId" uuid NOT NULL REFERENCES "Users"("Id") ON DELETE CASCADE,
  "Role" varchar(24) NOT NULL DEFAULT 'Member' CHECK ("Role" IN ('Manager', 'Member')),
  "JoinedAt" timestamptz NOT NULL DEFAULT now(),
  CONSTRAINT "PK_ProjectMembers" PRIMARY KEY ("ProjectId", "UserId")
);
CREATE INDEX IF NOT EXISTS "IX_ProjectMembers_UserId" ON "ProjectMembers"("UserId");

CREATE TABLE IF NOT EXISTS "WorkTasks" (
  "Id" uuid PRIMARY KEY,
  "ProjectId" uuid NOT NULL REFERENCES "Projects"("Id") ON DELETE CASCADE,
  "Title" varchar(200) NOT NULL,
  "Description" varchar(8000) NOT NULL DEFAULT '',
  "Status" varchar(24) NOT NULL DEFAULT 'Todo' CHECK ("Status" IN ('Todo', 'InProgress', 'InReview', 'Done', 'Blocked')),
  "Priority" varchar(24) NOT NULL DEFAULT 'Medium' CHECK ("Priority" IN ('Low', 'Medium', 'High', 'Urgent')),
  "AssigneeId" uuid NULL REFERENCES "Users"("Id") ON DELETE SET NULL,
  "CreatedById" uuid NOT NULL REFERENCES "Users"("Id") ON DELETE RESTRICT,
  "DueDate" date NULL,
  "CreatedAt" timestamptz NOT NULL DEFAULT now(),
  "UpdatedAt" timestamptz NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS "IX_WorkTasks_ProjectId_Status" ON "WorkTasks"("ProjectId", "Status");
CREATE INDEX IF NOT EXISTS "IX_WorkTasks_AssigneeId_DueDate" ON "WorkTasks"("AssigneeId", "DueDate");

CREATE TABLE IF NOT EXISTS "TaskComments" (
  "Id" uuid PRIMARY KEY,
  "TaskId" uuid NOT NULL REFERENCES "WorkTasks"("Id") ON DELETE CASCADE,
  "AuthorId" uuid NOT NULL REFERENCES "Users"("Id") ON DELETE RESTRICT,
  "Body" varchar(4000) NOT NULL,
  "CreatedAt" timestamptz NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS "IX_TaskComments_TaskId_CreatedAt" ON "TaskComments"("TaskId", "CreatedAt");
