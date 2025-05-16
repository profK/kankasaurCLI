module Kanka.NET.argu

open System.Text.Json
open Argu
open kankasaur.Campaigns.Campaigns


// Define subcommands for "list"
type ListSubCommands =
    | [<CliPrefix(CliPrefix.None)>] Campaigns
    | [<CliPrefix(CliPrefix.None)>] Maps of campaignID: string
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Campaigns -> "List all campaigns."
            | Maps _ -> "List all maps for a specific campaign."
            
type GetSubCommands =
    | [<CliPrefix(CliPrefix.None)>] Campaign of campaignID: string
    | [<CliPrefix(CliPrefix.None)>] Map of campaignID: string * mapID: string 
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Campaign _ -> "Get a campaign as a JSON file."
            | Map _ -> "Get a map as a JSON file."

// Define root commands
type RootCommands =
    | [<CliPrefix(CliPrefix.None)>] List of ParseResults<ListSubCommands>
    | [<CliPrefix(CliPrefix.None)>] Get of ParseResults<GetSubCommands>
    | [<CliPrefix(CliPrefix.None)>] Create
    | [<CliPrefix(CliPrefix.None)>] Update
    | [<CliPrefix(CliPrefix.None)>] Delete
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | List   _ -> "List resources (e.g., campaigns, maps)."
            | Create  -> "Create a new resource."
            | Update   -> "Update an existing resource."
            | Delete  -> "Delete a resource."
            | Get _ -> "Get a resource."




