
open System.IO
open System.Text.Json
open Argu
open Kanka.NET
open Kanka.NET.argu

open Kanka.Net.Campaigns.Campaigns
open kankasaur.JSONIO
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
    
let PrintToStream (json:JsonElement) =
    use writer = StreamWriter (GetOutputStream(), leaveOpen = true)
    writer.AutoFlush <- true
    formatJsonElement json
    |> writer.WriteLine
    
let PrintCampaigns  (json:JsonElement) =
    use writer = StreamWriter (GetOutputStream(), leaveOpen = true)
    writer.AutoFlush <- true
    json.GetProperty "data"
    |> fun data ->
        data.EnumerateArray()
        |> Seq.iter (fun camp ->
            let name = camp.GetProperty("name").GetString()
            let id = camp.GetProperty("id").GetInt32()                        
            writer.WriteLine $"{name} {id}"
        )
    //writer.Flush() 
let PrintGroups (json:JsonElement) =
    use writer = StreamWriter (GetOutputStream(), leaveOpen = true)
    writer.AutoFlush <- true
    json.GetProperty "data"
    |> fun data ->
        data.EnumerateArray()
        |> Seq.iter (fun group ->
            let name = group.GetProperty("name").GetString()
            let id = group.GetProperty("id").GetInt32()                        
            writer.WriteLine $"{name} {id}"
        )
    //writer.Flush()
let PrintMaps (json:JsonElement) =
    use writer = StreamWriter (GetOutputStream(), leaveOpen = true)
    writer.AutoFlush <- true
    json.EnumerateArray()
    |> Seq.iter (fun group ->
        let name = group.GetProperty("name").GetString()
        let id = group.GetProperty("id").GetInt32()                        
        writer.WriteLine $"{name} {id}"
    )
let PrintMarkers  (json:JsonElement) =
    use writer = StreamWriter (GetOutputStream(), leaveOpen = true)
    writer.AutoFlush <- true
    let data = json.GetProperty("data")
    data.EnumerateArray()
    |> Seq.iter (fun group ->
        let name = group.GetProperty("name").GetString()
        let id = group.GetProperty("id").GetInt32()
        writer.WriteLine $"{name} {id} "
        )
        
    //writer.Flush() 
    
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
        | [ ListSubCommands.Campaigns ] ->
            ListCampaigns ()
            |> PrintCampaigns
        | [ ListSubCommands.Maps campaignID ] ->
            ListMaps campaignID
            |> PrintMaps
        | [ ListSubCommands.MarkerGroups (campaignID, mapID)] ->
            ListMarkerGroups campaignID mapID
            |> PrintGroups
        | [ ListSubCommands.MapMarkers (campaignID, mapID) ] ->
            ListMapMarkers campaignID mapID 
            |> PrintMarkers
        | _ -> printfn "Invalid list command."
    | Get subCommand ->   
        match subCommand.GetAllResults() with
        | [ GetSubCommands.Campaign id ] -> GetCampaign id (GetOutputStream())
        | [ GetSubCommands.Map  (campaignID ,  mapID) ] ->
                    printfn "Output stream: %A" (GetOutputStream())
                    GetMap campaignID mapID 
        | [ GetSubCommands.MapMarkers (campaignID ,  mapID) ] -> 
            //GetMapMarker campaignID mapID (GetOutputStream())
            printfn "Unimplemented: GetMapMarkers"
        | _ -> printfn "Invalid list command."
        
    | Create subCommand ->
        match subCommand.GetAllResults() with
        | [ CreateSubCommands.MapMarker (campaignID, mapID,groupID,name, x, y )] ->
            NewMarkerJson campaignID mapID groupID name x y 1 1 
            |> CreateMapMarker campaignID mapID
            |> PrintToStream
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