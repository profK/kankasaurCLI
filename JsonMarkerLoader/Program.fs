// This application reads a JSON file containing map markers and groups
// and uploads them to Kanka as map markers
// Note that it has some odd hacks, like reversing of the Y coordinates specific to the
// size of the Waterdeep map
// This could be made more generic by fetching the map size from Kanka
module JBulkLoader

open System.IO
open System.Text.Json
open Kanka.NET
open FSharp.Data
open kankasaur.MapMarkers
open kankasaur.MarkerGroup

let mutable offsets = (0, 0)

let DoMarker campaignID mapID groupID (markerInput: JsonElement) =
    let name = markerInput.GetProperty("name").GetString()
    let color = markerInput.GetProperty("color").GetString()
    let x = markerInput.GetProperty("x").GetInt32()
    let y = (9000-markerInput.GetProperty("y").GetInt32()) 
    let text = markerInput.GetProperty("txt").GetString()
    sprintf $" Marker {name} at {x}, {y} with color {color} "
    
    NewMarkerJson campaignID mapID groupID name x y 1 1
    |> CreateMapMarker campaignID mapID 
    

let MakeGroup campaignID mapID groupName : JsonElement=
    CreateMarkerGroup campaignID mapID groupName
         
   

let GetGroupID campaignID mapID groupName =
    ListMarkerGroups campaignID mapID
    |> fun (jel:JsonElement) ->
        let data = jel.GetProperty("data")
        data.EnumerateArray()
        |> Seq.tryFind (fun group -> group.GetProperty("name").GetString() = groupName)
        |> function
            | Some group ->
                group.GetProperty("id").GetInt32().ToString()
            | None -> 
                printfn "Group %s not found" groupName
                CreateMarkerGroup campaignID mapID groupName
                 |> fun retValue ->
                    let retGroup = retValue.GetProperty("data")
                    retGroup.GetProperty("id").GetInt32().ToString()

    
let DoGroup campaignID mapID (group: JsonElement) =
    let groupName = group.GetProperty("name").GetString()
    let groupID = GetGroupID campaignID mapID groupName 
    group.GetProperty("markers").EnumerateArray()
    |> Seq.iter (fun marker -> 
                      DoMarker campaignID mapID groupID marker
                      |> ignore)
        

let DoDocument (JsonDocument: JsonDocument) =
    let docElement = JsonDocument.RootElement
    let mapID = docElement.GetProperty("mapID").GetString()
    let campaignID = docElement.GetProperty("campaignID").GetString()
    let groups = docElement.GetProperty("groups")
    let docOffsets = docElement.GetProperty("offsets")
    let offsetX = docOffsets.GetProperty("x").GetInt32()
    let offsetY = docOffsets.GetProperty("y").GetInt32()
    offsets <- (offsetX, offsetY)
    groups.EnumerateArray()
    |>Seq.iter (fun group -> DoGroup campaignID mapID group)
    

[<EntryPoint>]
let main argv =
    let infile = argv[0]
    use fileStream = new FileStream(infile, FileMode.Open, FileAccess.Read)
    use reader = new StreamReader(fileStream)
    let content = reader.ReadToEnd()
    let options = JsonDocumentOptions()
    JsonDocument.Parse(content)
    |> DoDocument
    0