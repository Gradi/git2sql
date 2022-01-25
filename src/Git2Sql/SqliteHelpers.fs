module Git2Sql.SqliteHelpers

open System
open Microsoft.Data.Sqlite

let command commandText (connection: SqliteConnection)  =
    let command = connection.CreateCommand ()
    command.CommandText <- commandText
    command

let (/>) (command: SqliteCommand) (name, obj) =
    let param = command.Parameters.AddWithValue (name, obj)
    match box obj with
    | null -> param.Value <- DBNull.Value
    | _ -> ()

    command

let (/>?) (command: SqliteCommand) (name, obj, sqliteType) =
    let param = command.Parameters.AddWithValue(name, obj)
    param.SqliteType <- sqliteType
    match box obj with
    | null -> param.Value <- DBNull.Value
    | _ -> ()

    command

let exec (command: SqliteCommand) =
    use command = command
    command.ExecuteNonQuery () |> ignore

let unpackOption (value: 'a option) =
    match value with
    | Some value -> box value
    | None -> DBNull.Value
