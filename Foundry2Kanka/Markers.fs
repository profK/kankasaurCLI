module Foundry2Kanka.Markers

open System.Drawing
open System.Text.Json.Nodes

let markerTemplateStr =
     """ 
     {
      "circle_radius": null,
      "colour": "#000000",
      "created_at": "2025-05-17T15:45:31.000000Z",
      "created_by": 310260,
      "custom_icon": null,
      "custom_shape": null,
      "entity_id": 7562534,
      "font_colour": null,
      "icon": "1",
      "id": 354741,
      "is_draggable": false,
      "is_popupless": false,
      "is_private": false,
      "latitude": 2504.483,
      "longitude": 3317.203,
      "map_id": 105967,
      "name": "The Rusty Anchor",
      "opacity": 100,
      "pin_size": null,
      "polygon_style": {
        "stroke": null,
        "stroke-width": null,
        "stroke-opacity": null
      },
      "shape_id": 1,
      "size_id": 1,
      "updated_at": "2025-05-17T15:45:31.000000Z",
      "updated_by": null,
      "visibility_id": 1
    }
    """

let createJsonObject (jsonStr: string) =
    // Parse the JSON string into a JsonNode and cast it to JsonObject
    match JsonNode.Parse(jsonStr) with
    | :? JsonObject as jsonObject -> jsonObject
    | _ -> failwith "Invalid JSON format"
 
let colorToHex (color: Color) =
    // Format the color's R, G, and B values into a hex string
    sprintf "#%02X%02X%02X" color.R color.G color.B
   
let makeMarkerObj (mapCoords: float * float) (name:string) (color:Color)
    (htmlText:string) =
    markerTemplateStr
    |> createJsonObject
    |> fun obj ->
        // Set the name and coordinates in the JSON document
        obj["name"] <- JsonValue.Create(name)
        obj["latitude"] <- JsonValue.Create(mapCoords.Item1)
        obj["longitude"] <- JsonValue.Create(mapCoords.Item2)
        obj["colour"] <- JsonValue.Create(colorToHex color)
        

        
        

