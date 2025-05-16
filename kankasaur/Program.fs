
open Argu
open Kanka.NET.argu
open kankasaur.Campaigns.Campaigns
open kankasaur.Maps

[<EntryPoint>]
let main argv =
    let parser = ArgumentParser.Create<RootCommands>(programName = "kankasaur")
    let results = parser.ParseCommandLine(argv, raiseOnUsage = true)

    match results.GetSubCommand() with
    | List subCommand ->
        match subCommand.GetAllResults() with
        | [ Campaigns ] -> ListCampaigns()
        | [ Maps campaignID ] -> ListMaps campaignID
        | _ -> printfn "Invalid list command."
    | Get  subCommand ->
        match subCommand.GetAllResults() with
        | [ Campaign id ] -> GetCampaign id
        | [ Map  (campaignID ,  mapID) ] ->
                    GetMap campaignID mapID
        | _ -> printfn "Invalid list command."
        
    | Create -> printfn "Create command not implemented."
    | Update -> printfn "Update command not implemented."
    | Delete -> printfn "Delete command not implemented."
    | _ -> printfn "Unknown command."
    0