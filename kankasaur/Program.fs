
open System.IO
open Argu
open Kanka.NET
open Kanka.NET.argu

open Kanka.Net.Campaigns.Campaigns
open kankasaur.MapMarkers
open kankasaur.Maps
open kankasaur.MarkerGroup

type fileInfo = {
    filename:string option;
    filepath:string option;
}
let mutable fileInfo = {
    filename = None;
    filepath = None;
}
let mutable groupID: string option = None
let mutable outputStream: Stream option = None
let UpdateOutputStream () =
    match fileInfo.filepath with
    | Some path -> path
    | None -> ""
    |> fun path ->
            printfn "Output path: %s" path
            match fileInfo.filename with
            | Some name ->
                printfn "Output filename: %s" name
                Path.Combine(path, name)
                outputStream <- Some (File.OpenWrite (Path.Combine(path, name)) :> Stream)
            | None ->
                outputStream <- None

let GetOutputStream() =
    match outputStream with
    | Some stream -> stream
    | None -> System.Console.OpenStandardOutput()
    

    
[<EntryPoint>]
let main argv =
    let parser = ArgumentParser.Create<RootCommands>(programName = "kankasaur")
    let results = parser.ParseCommandLine(argv, raiseOnUsage = true)
    results.TryGetResult F |> Option.iter (fun fileName ->
        printfn "File name: %s" fileName
        fileInfo <- { fileInfo with filename = Some fileName }
        UpdateOutputStream ()
    )
    results.TryGetResult D |> Option.iter (fun filePath ->
        printfn "File path: %s" filePath
        fileInfo <-
            { fileInfo with filepath = Some filePath }
        UpdateOutputStream ()
    )
    results.TryGetResult G |> Option.iter (fun groupid ->
        groupID <- Some groupid
    )
    match results.GetSubCommand() with
    | List subCommand ->
        match subCommand.GetAllResults() with
        | [ ListSubCommands.Campaigns ] -> ListCampaigns (GetOutputStream())
        | [ ListSubCommands.Maps campaignID ] -> ListMaps campaignID (GetOutputStream())
        | [ ListSubCommands.MarkerGroups (campaignID, mapID)] ->
            ListMarkerGroups campaignID mapID (GetOutputStream())
        | _ -> printfn "Invalid list command."
    | Get subCommand ->   
        match subCommand.GetAllResults() with
        | [ GetSubCommands.Campaign id ] -> GetCampaign id (GetOutputStream())
        | [ GetSubCommands.Map  (campaignID ,  mapID) ] ->
                    printfn "Output stream: %A" (GetOutputStream())
                    GetMap campaignID mapID (GetOutputStream())
        | [ GetSubCommands.MapMarkers (campaignID ,  mapID) ] -> GetMapMarkers campaignID mapID (GetOutputStream())
        | _ -> printfn "Invalid list command."
        
    | Create subCommand ->
        match subCommand.GetAllResults() with
        | [ CreateSubCommands.MapMarker (campaignID, mapID,name, x, y )] ->
            CreateMapMarker campaignID mapID  name (x, y) 1 1 (GetOutputStream())
        | [ CreateSubCommands.MarkerGroup (campaignID, mapID,name)] ->
            CreateMarkerGroup campaignID mapID name  (GetOutputStream())
        |_ -> printfn "Invalid create command."
    | Update -> printfn "Update command not implemented."
    | Delete -> printfn "Delete command not implemented."
    | F fileName ->
        printfn "File name: %s" fileName
        fileInfo <- { fileInfo with filename = Some fileName }
    | D filePath->
        printfn "File path: %s" filePath
        fileInfo <- { fileInfo with filepath = Some filePath }
    | _ -> printfn "Unknown command."
    0