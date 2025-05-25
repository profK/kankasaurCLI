module Kanka.NET.argu

open System.Text.Json
open Argu



// Define subcommands for "list"
module ListSubCommands =
    open System.IO
    open Kanka.NET
    open kankasaur.JSONIO

    let ListCampaigns outStream =
        let writer = StreamWriter (outStream, leaveOpen = true)
        writer.AutoFlush <- true
        Kanka.GetCampaigns()
        |> fun jel ->
            jel.GetProperty "data"
            |> fun data ->
                data.EnumerateArray()
                |> Seq.iter (
                    fun camp ->
                        let name = camp.GetProperty("name").GetString()
                        let id = camp.GetProperty("id").GetInt32()                        
                        writer.WriteLine $"{name} {id}"
                    )
        //writer.Flush()
    type ListSubCommands =
        | [<CliPrefix(CliPrefix.None)>] Campaigns
        | [<CliPrefix(CliPrefix.None)>] Maps of campaignID: string
        | [<CliPrefix(CliPrefix.None)>] MarkerGroups of campaignID: string * mapID: string
        | [<CliPrefix(CliPrefix.None)>] MapMarkers of campaignID: string * mapID: string 
        
        interface IArgParserTemplate with
            member s.Usage =
                
                match s with
                | Campaigns -> "List all campaigns."
                | Maps _ -> "List all maps for a specific campaign."
                | MarkerGroups _ -> "List all marker groups for a map."
                | MapMarkers _ -> "List all map markers for a specific map."
module GetSubCommands     =       
    open System.IO
    open Kanka.NET
    open kankasaur.JSONIO

    let GetCampaign (id: string) (outStream:Stream) =
        use writer = StreamWriter (outStream, leaveOpen = true)
        writer.AutoFlush <- true
        Kanka.GetCampaign(id)
        |>  fun jel ->
                  formatJsonElement jel
                  |> writer.WriteLine

    let GetMap (campaignID: string) (mapID:string) (outStream:Stream)=
        use writer = StreamWriter (outStream, leaveOpen = true)
        writer.AutoFlush <- true
        // Assuming GetMap is a function that fetches a specific map for a given campaign ID
        Kanka.GetMap campaignID mapID
        |> fun jel ->
            formatJsonElement jel
            |> writer.WriteLine
    type GetSubCommands =
        | [<CliPrefix(CliPrefix.None)>] Campaign of campaignID: string
        | [<CliPrefix(CliPrefix.None)>] Map of campaignID: string * mapID: string
        | [<CliPrefix(CliPrefix.None)>] MapMarkers of campaignID: string * mapID: string
        interface IArgParserTemplate with
            member s.Usage =
                match s with
                | Campaign _ -> "Get a campaign as a JSON file."
                | Map _ -> "Get a map as a JSON file."
                | MapMarkers _ -> "Get map markers as a JSON file."
 module CreateSubCommands       =    
    open System.IO
    open Kanka.NET
    open kankasaur.JSONIO

    
    type CreateSubCommands =
        | [<CliPrefix(CliPrefix.None)>] Campaign of campaignID: string
        | [<CliPrefix(CliPrefix.None)>] Map of campaignID: string * mapID: string
        | [<CliPrefix(CliPrefix.None)>] MapMarker of 
            campaignID:string * mapID: string * groupID:string * name:string * x:int32 * y:int32
        | [<CliPrefix(CliPrefix.None)>] MarkerGroup of 
            campaignID:string * mapID: string * name:string 
        interface IArgParserTemplate with
            member s.Usage =
                match s with
                | Campaign _ -> "Create a new campaign."
                | Map _ ->  "Create a new map."
                | MapMarker _ -> "Create a new map marker."
                | MarkerGroup _ -> "Create a new marker group."

// Define root commands
type RootCommands =
    | [<CliPrefix(CliPrefix.None)>] List of
        ParseResults<ListSubCommands.ListSubCommands>
    | [<CliPrefix(CliPrefix.None)>] Get of
        ParseResults<GetSubCommands.GetSubCommands>
    | [<CliPrefix(CliPrefix.None)>] Create of
        ParseResults<CreateSubCommands.CreateSubCommands>
    | [<CliPrefix(CliPrefix.None)>] Update
    | [<CliPrefix(CliPrefix.None)>] Delete
    | [<AltCommandLine("-f")>] F of fileName: string
    | [<AltCommandLine("-d")>] D of filePath: string
    | [<AltCommandLine("-g")>] G of groupID: string

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | List   _ -> "List resources: kankasaur list <type> <type params>."
            | Create _ -> "Create a new resource: kankasaur create <type> <type params>."
            | Update   -> "Update a resource: kankasaur update <type> <type params>."
            | Delete  -> "Delete a resource: kankasaur delete <type> <type params>."
            | Get _ -> "Get a resource: kankasaur get <type> <resource id>."
            | F _ -> "Specify the output file name."
            | D _ -> "Specify the output file path."
            | G _ -> "Specify a group ID"
            


