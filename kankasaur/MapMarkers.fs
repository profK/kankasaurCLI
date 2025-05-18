module kankasaur.MapMarkers

open System.IO
open System.Text.Json
open Kanka.NET
open kankasaur.JSONIO
type CreateMarkerRec = {
    name: string
    map_id: string
    lattitude: float
    longitude: float
    icon: int
    shape_id: int
}

let GetMapMarkers (campaignID:string) (mapID:string) (outStream:Stream)=
    use writer = StreamWriter (outStream, leaveOpen = true)
    // Assuming GetMap is a function that fetches a specific map for a given campaign ID
    Kanka.GetMapMarkers campaignID mapID
    |> fun jel ->
        formatJsonElement jel
        |> writer.WriteLine
        ()
        
let CreateMapMarker (name:string) (mapID:string) (location: float * float)
    (iconType:int) (shapeType:int)=
    let newMarker = {
        name = name
        map_id = mapID
        lattitude = fst location |> float
        longitude = snd location |> float
        icon = iconType
        shape_id = shapeType
    }
    // for testing purposes
    let options = JsonSerializerOptions(PropertyNamingPolicy = JsonNamingPolicy.CamelCase)
    let jsonWithCamelCase = JsonSerializer.Serialize(newMarker, options)
    printfn "CamelCase JSON: %s" jsonWithCamelCase
    
    
    