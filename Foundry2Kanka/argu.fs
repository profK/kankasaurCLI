module Kanka.NET.argu

open System.Text.Json
open Argu



// Define subcommands for "merge"
type MergeSubCommands =
    | [<CliPrefix(CliPrefix.None)>] Markers of source:string * destination:string
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Markers _-> "Merge markers from <source> into <destination>."
            

// Define root commands
type RootCommands =
    | [<CliPrefix(CliPrefix.None)>] Merge of ParseResults<MergeSubCommands>

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Merge _ -> "Merge foundry data into kankasaur data."
            
            


