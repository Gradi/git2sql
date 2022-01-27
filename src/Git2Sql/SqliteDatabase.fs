module Git2Sql.SqliteDatabase

open LibGit2Sharp
open Microsoft.Data.Sqlite

open SqliteHelpers

let readSqlScript scriptname =
    let fullPath = sprintf $"Git2Sql.Resources.%s{scriptname}.sql"
    use stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream fullPath
    use reader = new System.IO.StreamReader (stream, System.Text.Encoding.UTF8)
    reader.ReadToEnd ()

let sqlInit               = readSqlScript "SQLiteInitScript"
let sqlInsertBranch       = readSqlScript "InsertBranch"
let sqlInsertCommit       = readSqlScript "InsertCommit"
let sqlInsertCommitParent = readSqlScript "InsertCommitParent"
let sqlInsertTag          = readSqlScript "InsertTag"

let getConnection filename =
    let connBuilder = SqliteConnectionStringBuilder ()
    connBuilder.DataSource <- filename
    connBuilder.Mode <- SqliteOpenMode.ReadWriteCreate
    connBuilder.ForeignKeys <- true

    new SqliteConnection (connBuilder.ToString ())

let initializeDatabase (connection: SqliteConnection) =
    connection
    |> command sqlInit
    |> exec

let insertCommit (connection: SqliteConnection) (commit: Commit) =
    (connection |> command sqlInsertCommit)
    /> ("$id", commit.Id.ToString ())
    /> ("$message", commit.Message)
    /> ("$encoding", commit.Encoding)
    /> ("$authorName", commit.Author.Name)
    /> ("$authorEmail", commit.Author.Email)
    /> ("$authorDateTime", commit.Author.When)
    /> ("$committerName", commit.Committer.Name)
    /> ("$committerEmail", commit.Committer.Email)
    /> ("$committerDateTime", commit.Committer.When)
    |> exec

let insertCommitParent (connection: SqliteConnection) (id: string) (parentId: string) =
    (connection |> command sqlInsertCommitParent)
    /> ("$id", id)
    /> ("$parentId", parentId)
    |> exec

let insertBranch (connection: SqliteConnection) (branch: Branch) =
    (connection |> command sqlInsertBranch)
    /> ("$canonicalName", branch.CanonicalName)
    /> ("$friendlyName", branch.FriendlyName)
    /> ("$isRemote", branch.IsRemote)
    /> ("$isTracking", branch.IsTracking)
    /> ("$isCurrentRepositoryHead", branch.IsCurrentRepositoryHead)
    /> ("$commitId", branch.Tip.Id.ToString())
    /> ("$remoteBranchCanonicalName", branch.TrackedBranch |> Option.ofObj |> Option.map (fun t -> t.CanonicalName) |> unpackOption)
    /> ("$remoteBranchFriendlyName", branch.TrackedBranch |> Option.ofObj |> Option.map (fun t -> t.FriendlyName) |> unpackOption)
    |> exec

let insertTag (connection: SqliteConnection) (tag: Tag) =
    (connection |> command sqlInsertTag)
    /> ("$canonicalName", tag.CanonicalName)
    /> ("$friendlyName", tag.FriendlyName)
    /> ("$isAnnotated", tag.IsAnnotated)
    /> ("$annotatedName", tag.Annotation |> Option.ofObj |> Option.map (fun c -> c.Name) |> unpackOption)
    /> ("$annotatedMessage", tag.Annotation |> Option.ofObj |> Option.map (fun c -> c.Message) |> unpackOption)
    /> ("$annotatedTaggerName", tag.Annotation |> Option.ofObj |> Option.bind (fun c -> c.Tagger |> Option.ofObj) |> Option.map (fun c -> c.Name) |> unpackOption)
    /> ("$annotatedTaggerEmail", tag.Annotation |> Option.ofObj |> Option.bind (fun c -> c.Tagger |> Option.ofObj) |> Option.map (fun c -> c.Email) |> unpackOption)
    /> ("$annotatedTaggerDateTime", tag.Annotation |> Option.ofObj |> Option.bind (fun c -> c.Tagger |> Option.ofObj) |> Option.map (fun c -> c.When) |> unpackOption)
    /> ("$targetId", tag.Target.Id.ToString())
    |> exec

