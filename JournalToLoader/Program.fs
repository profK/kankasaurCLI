// For more information see https://aka.ms/fsharp-console-apps

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



let ScanPins(filepath:string) =
    use reader = new StreamReader(filepath)
    let json = reader.ReadToEnd()
    let jsonDoc = JsonDocument.Parse(json)
    let root = jsonDoc.RootElement
    root.GetProperty("notes").EnumerateArray()
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
let DoGroups pinMap (markers:JsonElement)=
    markers.EnumerateArray()
    |> Seq.map(fun groupElement ->
            {
              name = groupElement.GetProperty("name").GetString()
              pages = DoPages (
                        groupElement.GetProperty("pages")) pinMap
            }
        )

let DoFile (pinMap: Map<string,PinRec>) (filePath:string) =
    use reader = new StreamReader(filePath)
    let json = reader.ReadToEnd()
    let jsonDoc = JsonDocument.Parse(json)
    let root = jsonDoc.RootElement
    let name= root.GetProperty("name").GetString()
    let pages = root.GetProperty("pages")
    {name = name
     pages = DoPages pages pinMap }
    


        
        

[<EntryPoint>]
let main argv =
    if argv.Length <> 2 then
        printfn "Usage: JournalToLoader <path to journal and scene files> <output file>"
        1
    else
        let filePath = argv.[0]
        let outputFile = argv.[1]
        if not (Directory.Exists filePath) then
            printfn "File %s does not exist." filePath
            1
        else
            try
                let jsonFiles= Directory.GetFiles (filePath, "*.json")
                let pinMap =
                        jsonFiles
                        |> Array.filter (fun file ->
                            file.Contains "Scene")
                        |> Array.fold (fun acc file ->
                            ScanPins file
                            |> Map.fold (fun (acc:Map<string,PinRec>) key value ->
                                acc.Add(key, value)) acc
                            ) Map.empty
                printfn $"PinMap:\n {pinMap}"        
                jsonFiles
                |> Array.filter (fun file ->
                    file.Contains "JournalEntry")
                |> Array.map (fun fname ->
                    DoFile pinMap fname)
                |> Seq.iter (fun mapRec ->
                    let options = JsonSerializerOptions(PropertyNamingPolicy = JsonNamingPolicy.CamelCase)
                    let json = JsonSerializer.Serialize(mapRec, options)
                    File.AppendAllText(outputFile, json + "\n")
                )
                    
           
                0
            with
            | ex ->
                printfn "An error occurred: %s" ex.Message
                1