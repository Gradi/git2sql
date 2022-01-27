module Git2Sql.Program

open Argu
open LibGit2Sharp

open SqliteDatabase

type CLIArguments =
    | GitPath of string
    | [<Mandatory>] DbPath of string
    | Verbose of bool

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | GitPath _ -> "Path to git repository. If not specified current directory will be used intead."
            | DbPath _ -> "Filename of resulting SQLite file. If file already exists program will exit."
            | Verbose _ -> "Log progress to stdout"

type ProgramArguments =
    { GitPath: string
      DbPath: string
      Verbose: bool }

let cliArgsToProgramArgs (result: ParseResults<CLIArguments>) =
    let gitPath =
        match result.TryGetResult GitPath with
        | Some gitPath -> gitPath
        | None -> System.IO.Directory.GetCurrentDirectory ()

    let dbPath = result.GetResult DbPath

    let verbose =
        match result.TryGetResult Verbose with
        | Some verbose -> verbose
        | None -> false

    { GitPath = gitPath; DbPath = dbPath; Verbose = verbose }

let validateProgramArguments args =
    match Repository.IsValid args.GitPath with
    | false -> failwithf $"\"%s{args.GitPath}\" is not valid Git repository."
    | true -> ()

    match System.IO.File.Exists args.DbPath with
    | true -> failwithf $"\"%s{args.DbPath}\" already exists. Delete or rename it."
    | false -> ()

let log enabled (format: Printf.TextWriterFormat<unit>) =
    if enabled then
        let time = System.DateTime.Now.ToString "HH:mm:ss"
        printfn $"[%s{time}]: {format}"

let wrapSeq enabled prefix sequence =
    match enabled with
    | true ->
        seq {
            let mutable count = 0
            let print () = printf $"\r%s{prefix}%d{count}"

            for element in sequence do
                if count % 200 = 0 then print ()
                count <- count + 1
                yield element

            print ()
            printfn ""
        }
    | false -> sequence

[<EntryPoint>]
let main argv =
    try
        let programArguments =
            (ArgumentParser.Create<CLIArguments> ()).Parse argv
            |> cliArgsToProgramArgs

        validateProgramArguments programArguments

        let log = log programArguments.Verbose

        use connection = getConnection programArguments.DbPath
        use repository = new Repository (programArguments.GitPath)

        connection.Open ()
        log "Initializing database"
        initializeDatabase connection

        let commits =
            repository.Branches
            |> Seq.collect (fun b -> b.Commits)
            |> wrapSeq programArguments.Verbose "Commits: "

        log "Dumping commits"
        commits
        |> Seq.iter (insertCommit connection)

        log "Dumping commit parents"
        commits
        |> Seq.iter (fun c -> c.Parents |> Seq.iter (fun p -> insertCommitParent connection (c.Id.ToString()) (p.Id.ToString())))

        log "Dumping branches"
        repository.Branches
        |> wrapSeq programArguments.Verbose "Branches: "
        |> Seq.iter (insertBranch connection)

        log "Dumping tags"
        repository.Tags
        |> wrapSeq programArguments.Verbose "Tags: "
        |> Seq.iter (insertTag connection)

        0
    with
    | :? ArguParseException as e ->
        eprintfn $"%s{e.Message}"
        1
    | exc ->
        eprintfn $"%O{exc}"
        1
