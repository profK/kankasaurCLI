// This application reads a JSON file containing map markers and groups
// and uploads them to Kanka as map markers
// Note that it has some odd hacks, like reversing of the Y coordinates specific to the
// size of the Waterdeep map
// This could be made more generic by fetching the map size from Kanka
module JBulkLoader

open System.Drawing
open System.IO
open System.Text.Json
open Kanka.NET
open FSharp.Data
open Kanka.Net.Campaigns.Campaigns
open kankasaur.MapMarkers
open kankasaur.Maps
open kankasaur.MarkerGroup

let mutable offsets = (0, 0)

let DoMarker campaignID mapID groupID (markerInput: JsonElement) =
    let name = markerInput.GetProperty("name").GetString()
    let color = Color.White // Default color, can be changed later
    let x = markerInput.GetProperty("x").GetInt32()
    let y = (9000-markerInput.GetProperty("y").GetInt32()) 
    let text = markerInput.TryGetProperty("txt")
               |> function
                   |tple when fst tple = true ->
                       (snd tple).GetString()
                   | _ ->  ""  
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
    group.GetProperty("pages").EnumerateArray()
    |> Seq.iter (fun marker -> 
                      DoMarker campaignID mapID groupID marker
                      |> ignore)
        
let GetCampaignID (campaignName: string) =
    ListCampaigns()
    |> fun (jel:JsonElement) ->
        let data = jel.GetProperty("data")
        data.EnumerateArray()
        |> Seq.tryFind (fun campaign -> campaign.GetProperty("name").GetString() = campaignName)
        |> function
            | Some campaign ->
                campaign.GetProperty("id").GetInt32().ToString()
            | None -> 
                printfn $"Campaign {campaignName} not found. "
                printfn "Campaigns must be created in Kanka first."
                failwith "Campaign not found"
                
let rec GetMapID campaignID mapName mapUrl =
    ListMaps campaignID
    |> fun (jel:JsonElement) ->
        jel.EnumerateArray()
        |> Seq.tryFind (fun map -> map.GetProperty("name").GetString() = mapName)
        |> function
            | Some map ->
                map.GetProperty("id").GetInt32().ToString()
            | None ->
                NewMapJson mapName mapUrl  
                |>fun newMap ->
                    let createdMap = CreateMap campaignID newMap
                    GetMapID campaignID mapName mapUrl
              
    
                
let DoDocument (JsonDocument: JsonDocument) =
    let rootElement = JsonDocument.RootElement
    let campaignName = rootElement.GetProperty("campaignName").GetString()
    let campaignID = GetCampaignID (campaignName)
    let mapName = rootElement.GetProperty("mapName").GetString()
    let mapUrl = rootElement.GetProperty("mapURL").GetString()
    let mapID = GetMapID campaignID mapName mapUrl
   
    let groups = rootElement.GetProperty("groups")
    let docOffsets = rootElement.GetProperty("offsets")
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