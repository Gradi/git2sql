# Git2Sql

Git2Sql is a command line utility that converts git repository to
SQLite database.

## Installation

- Install .NET 6
- Now you can run program with command

```
dotnet run --project src/Git2Sql/Git2Sql.fsproj -c Release -- --help
```

## Usage

```
USAGE: Git2Sql.exe [--help] [--gitpath <string>] --dbpath <string> [--verbose <bool>]

OPTIONS:

    --gitpath <string>    Path to git repository. If not specified current directory will be used intead.
    --dbpath <string>     Filename of resulting SQLite file. If file already exists program will exit.
    --verbose <bool>      Log progress to stdout
    --help                display this list of options.
```

- Download a fresh copy of some git repo (or fetch/pull existing one).
- Run command

```
dotnet run --project src/Git2Sql/Git2Sql.fsproj -c Release -- --gitpath <path> --dbpath <path-to-where-put-sqlite-result>
```

You can omit `--gitpath` if current working dir is repository.

PS. It may take some time.

## What is saved

- All commits
- ... and their parent ids
- Branches
- Tags
