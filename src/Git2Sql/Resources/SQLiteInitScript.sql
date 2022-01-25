CREATE TABLE "Commits" (
    "Id" TEXT PRIMARY KEY,
    "Message" TEXT,
    "Encoding" TEXT,
    "AuthorName" TEXT,
    "AuthorEmail" TEXT,
    "AuthorDateTime" DATETIME,
    "CommitterName" TEXT,
    "CommitterEmail" TEXT,
    "CommitterDateTime" DATETIME
) ;

CREATE TABLE "CommitParents" (
    "Id" TEXT NOT NULL,
    "ParentId" TEXT NOT NULL,
    CONSTRAINT "Unique_Id_ParentId" UNIQUE ("Id", "ParentId")
) ;

CREATE TABLE "Branches" (
    "CanonicalName" TEXT NOT NULL,
    "FriendlyName" TEXT NOT NULL,
    "IsRemote" BOOLEAN NOT NULL,
    "IsTracking" BOOLEAN NOT NULL,
    "IsCurrentRepositoryHead" BOOLEAN NOT NULL,
    "CommitId" TEXT NOT NULL REFERENCES "Commits" ("Id"),
    "RemoteBranchCanonicalName" TEXT NULL,
    "RemoteBranchFriendlyName" TEXT NULL
) ;

CREATE TABLE "Tags" (
    "CanonicalName" TEXT NOT NULL,
    "FriendlyName" TEXT NOT  NULL,
    "IsAnnotated" BOOLEAN NOT NULL,
    "AnnotatedName" TEXT NULL,
    "AnnotatedMessage" TEXT NULL,
    "AnnotatedTaggerName" TEXT NULL,
    "AnnotatedTaggerEmail" TEXT NULL,
    "AnnotatedTaggerDateTime" DATETIME NULL,
    "TargetId" TEXT NOT NULL
) ;
