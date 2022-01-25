module Git2Sql.Program

open Argu
open LibGit2Sharp

open SqliteDatabase

type CLIArguments =
    | GitPath of string
    | [<Mandatory>] DbPath of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | GitPath _ -> "Path to git repository. If not specified current directory will be used intead."
            | DbPath _ -> "Filename of resulting SQLite file. If file already exists program will exit."

type ProgramArguments =
    { GitPath: string
      DbPath: string }

let cliArgsToProgramArgs (result: ParseResults<CLIArguments>) =
    let gitPath =
        match result.TryGetResult GitPath with
        | Some gitPath -> gitPath
        | None -> System.IO.Directory.GetCurrentDirectory ()

    let dbPath = result.GetResult DbPath

    { GitPath = gitPath; DbPath = dbPath }

let validateProgramArguments args =
    match Repository.IsValid args.GitPath with
    | false -> failwithf $"\"%s{args.GitPath}\" is not valid Git repository."
    | true -> ()

    match System.IO.File.Exists args.DbPath with
    | true -> failwithf $"\"%s{args.DbPath}\" already exists. Delete or rename it."
    | false -> ()

[<EntryPoint>]
let main argv =
    try
        let programArguments =
            (ArgumentParser.Create<CLIArguments> ()).Parse argv
            |> cliArgsToProgramArgs

        validateProgramArguments programArguments

        use connection = getConnection programArguments.DbPath
        use repository = new Repository (programArguments.GitPath)

        connection.Open ()
        initializeDatabase connection

        let commits =
            repository.Branches
            |> Seq.collect (fun b -> b.Commits)

        commits
        |> Seq.iter (insertCommit connection)

        commits
        |> Seq.iter (fun c -> c.Parents |> Seq.iter (fun p -> insertCommitParent connection (c.Id.ToString()) (p.Id.ToString())))

        repository.Branches
        |> Seq.iter (insertBranch connection)

        repository.Tags
        |> Seq.iter (insertTag connection)


        0
    with
    | :? ArguParseException as e ->
        eprintfn $"%s{e.Message}"
        1
    | exc ->
        eprintfn $"%O{exc}"
        1
