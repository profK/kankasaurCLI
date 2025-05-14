open System
open System.IO
open System.CommandLine
open FSCli

type ListCmds =
    | Campaigns
    | Maps
type rootCmds =
    | List of ListCmds
    | Create
    | Update
    | Delete

[<EntryPoint>]
let main argv =
    let rootCommand = FSCli.ParseDU typedefof<rootCmds>

    // Parse and invoke the command line
    rootCommand.InvokeAsync(argv)
    0