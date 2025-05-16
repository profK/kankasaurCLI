open System
open System.IO
open System.CommandLine
open FSCli
open FSCli.FSCli
open Kanka
open Kanka.NET
open Kanka.NET.Kanka
open kankasaur.Campaigns.Campaigns

// actions

type ListCmds =
    | Campaigns
    | Maps of campaignID:string
    interface ICommandList with
        member this.Description() =
            match this with
            | Campaigns -> "List all campaigns"
            | Maps campaignID -> $"List all maps for campaign {campaignID}"
        member this.Execute()  =  
            match this with
            | Campaigns -> ListCampaigns()
            | Maps campaign -> printfn "list-maps"
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