// JournalToLoader.fsx
// This script reads journal and scene files,
// extracts pin data, and outputs a JSON representation of the map.
// To use this you must have one scene and all its associated journal
// files in a directory. Only 1 scene can be processed per directory
// It is designed to be run from the command line with two arguments:
// 1. The path to the directory containing the scene and journal files.
// 2. The output file path where the JSON will be saved.
// Usage: JournalToLoader <path to journal and scene files> <output file>
// Example: JournalToLoader "C:\Path\To\FileDirectory" "C:\Path\To\Output\map.json"


open System.Drawing
open System.IO
open System.Text.Json
open JournalToLoader.JsonFormatting


type PinRec= {
    entryId: string
    pageId: string
    x: float
    y: float
    imgPath: string option
}



let ScanPins (notes:JsonElement) =
    notes.EnumerateArray()
    |> Seq.fold (fun (map:Map<string,PinRec>) note ->
        let entryId = note.GetProperty("entryId").GetString()
        let pageId = note.GetProperty("pageId").GetString()
        let x = note.GetProperty("x").GetDouble()
        let y = note.GetProperty("y").GetDouble()
        let entry = {
            entryId = entryId
            pageId = pageId
            x = x; y = y
            imgPath = 
               note.TryGetProperty("texture")
               |> function 
                  | tple when (fst tple) = true ->
                    (snd tple).TryGetProperty("path")
                     |> function 
                          | path when (fst path)= true->
                              Some ((snd path).GetString())
                          | _ -> None
                  | _ ->None
               
        }
        map.Add(pageId, entry)
    ) Map.empty
 

let DoPages (pages:JsonElement) (pinMap: Map<string,PinRec>) =
    pages.EnumerateArray()
    |> Seq.map(fun pageElement ->
                  let pinRec =
                      pinMap.TryFind(pageElement.GetProperty("_id").GetString())
                      |> function
                        | Some pin -> pin
                        |None -> failwith $"""Pin not found for page {pageElement.GetProperty("id").GetString()}"""
                  let name = pageElement.GetProperty("name").GetString()
                  let id = pageElement.GetProperty("_id").GetString()
                  let pageId = pinRec.pageId
                  let iconPath =
                      match pinRec.imgPath with
                        | Some path -> path
                        | None -> ""
                  let x = pinRec.x
                  let y = pinRec.y    
                  {
                    name = name
                    id = id
                    pageId = pageId
                    //color = color.
                    iconPath = iconPath
                    x = x
                    y = y
                  }
        )
let DoGroups pinMap (jsonFiles:string array)=
    jsonFiles
    |> Array.filter (fun file -> file.Contains "JournalEntry")
    |> Array.map (fun file ->
        use reader = new StreamReader(file)
        let json = reader.ReadToEnd()
        let jsonDoc = JsonDocument.Parse(json)
        let root = jsonDoc.RootElement
        let name = root.GetProperty("name").GetString()
        let pages = root.GetProperty("pages")
        { name = name
          pages = DoPages pages pinMap
        }
    )
let ReadScene (scenePath:string)  =
    use reader = new StreamReader(scenePath)
    let json = reader.ReadToEnd()
    let jsonDoc = JsonDocument.Parse(json)
    let root = jsonDoc.RootElement
    let name= root.GetProperty("name").GetString()
    let pinMap = ScanPins (root.GetProperty("notes"))
    let mapname = name
    let mapURL = 
        root.TryGetProperty("background")
        |> function
            | tpl when fst tpl = true ->
                 (snd tpl).TryGetProperty("src")
                |> function
                    | path when fst path = true ->
                        (snd path).GetString()
                        | _ -> ""
            | _ -> ""
    (pinMap,mapname,mapURL)
   
    


        
        

[<EntryPoint>]
let main argv =
    if argv.Length <> 2 then
        printfn "Usage: JournalToLoader <path to journal and scene files> <output file>"
        1
    else
        let filePath = argv.[0]
        let outputFile = argv.[1]
        if not (Directory.Exists filePath) then
            printfn "Directory %s does not exist." filePath
            1
        else
            let jsonFiles= Directory.GetFiles (filePath, "*.json")
            jsonFiles
            |> Array.tryFind (fun file ->
                file.Contains "Scene")
            |> function
                | None -> 
                    printfn "No scene file found in the specified directory."
                    1
                | Some sceneFile ->
                    sceneFile
                    |>ReadScene 
                    |> fun (pinMap, mapName, mapURL) ->
                            { name = mapName
                              mapURL = mapURL
                              groups =
                                  DoGroups pinMap jsonFiles 
                            }
                    |> fun mapRec ->
                        let options = JsonSerializerOptions(PropertyNamingPolicy = JsonNamingPolicy.CamelCase)
                        let jsonString = JsonSerializer.Serialize(mapRec, options)
                        File.WriteAllText(outputFile, jsonString)
                        printfn "Map data written to %s" outputFile
                        0  
                                    
                                    
                            
                        
                
            