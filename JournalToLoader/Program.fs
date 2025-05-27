// For more information see https://aka.ms/fsharp-console-apps

open System.Drawing
open System.IO
open System.Text.Json

type MarkerRec = {
    name : string
    id : string
    pageId : string
    color : Color
    iconPath : string
    x : int
    y : int    
}

type PageRec = {
    name : string
    pageId : string
    markers : MarkerRec list 
}

let mutable pages = Map.empty

let DoPage (filePath: string) =
    let jsonContent = File.ReadAllText filePath
    let jsonDocument = JsonDocument.Parse(jsonContent)
    let rootElement = jsonDocument.RootElement
    let name = rootElement.GetProperty("name").GetString()
    let pageId = rootElement.GetProperty("_id").GetString()
    pages <- pages.Add(pageId, { name = name; pageId = pageId; markers = [] })
    ()

let DoMarkers (filePath: string) =
    let jsonContent = File.ReadAllText filePath
    let jsonDocument = JsonDocument.Parse(jsonContent)
    let rootElement = jsonDocument.RootElement
    let pageId = rootElement.GetProperty("page_id").GetString()
    
    if pages.ContainsKey(pageId) then
        let name = rootElement.GetProperty("name").GetString()
        let id = rootElement.GetProperty("_id").GetString()
        let color = Color.FromArgb(
            rootElement.GetProperty("color").GetInt32())
        let iconPath = rootElement.GetProperty("icon").GetString()
        let x = rootElement.GetProperty("x").GetInt32()
        let y = rootElement.GetProperty("y").GetInt32()

        let markerRec = {
            name = name
            id = id
            pageId = pageId
            color = color
            iconPath = iconPath
            x = x
            y = y
        }
        pages <- pages.Add(pageId,
                           { pages.[pageId] with
                              markers = pages.[pageId].markers @ [markerRec] })
        ()

        let updatedPage =
            { pages.[pageId] with markers = markerRec :: pages.[pageId].markers }
        
        pages <- pages.Add(pageId, updatedPage)
    else
        printfn "Page ID %s not found for markers." pageId
 
type OutputMarker = {
    name: string
    color: string
    txt: string
    x: int
    y: int
}
type OutputGroup = {
    name: string
    markers: OutputMarker list
}
type OutputRec = {
     groups : OutputGroup  list
}

let GenerateLoaderData (outputFile: string) (pages: Map<string, PageRec>) =
        // "name":"Seaseyes Tower", "color":"#58ACFA", "x":338, "y":2172,
        // "txt":"<p>City building</p>", "ref":"$71"
        use writer = new StreamWriter(outputFile)
        //make the JSON template
        let outputData = 
            pages
            |> Map.toList
            |> List.map (fun (_, page) ->
                {
                    groups = 
                        page.markers
                        |> List.groupBy (fun m -> m.name)
                        |> List.map (fun (name, markers) ->
                            {
                                name = name
                                markers = 
                                    markers
                                    |> List.map (fun m ->
                                        {
                                            name = m.name
                                            color = m.color.ToArgb().ToString("X8")
                                            txt = sprintf "<p>%s</p>" page.name
                                            x = m.x
                                            y = m.y
                                        })
                            })
                })
        JsonSerializer.Serialize (outputData, 
            JsonSerializerOptions(PropertyNamingPolicy = JsonNamingPolicy.CamelCase))
        |> writer.WriteLine
        
        

[<EntryPoint>]
let main argv =
    
    if argv.Length <> 2 then
        printfn "Usage: JournalToLoader <path to journal and scene files> <output file>"
        1
    else
        let filePath = argv.[0]
        let outputFile = argv.[1]
        if not (File.Exists filePath) then
            printfn "File %s does not exist." filePath
            1
        else
            try
                let jsonFiles= Directory.GetFiles (Path.GetDirectoryName filePath, "*.json")
                jsonFiles
                |> Array.filter (fun file ->
                    file.Contains "JournalEntry")
                |> Array.iter DoPage
                 
                jsonFiles
                |> Array.filter (fun file ->
                    file.Contains "Scene")
                |> Array.iter DoMarkers
                    
                GenerateLoaderData outputFile pages
                0
            with
            | ex ->
                printfn "An error occurred: %s" ex.Message
                1