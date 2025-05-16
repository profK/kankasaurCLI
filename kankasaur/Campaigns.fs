module kankasaur.Campaigns

open System.Reflection
open Kanka.NET
open kankasaur.JSONIO



module Campaigns =
    let ListCampaigns() =
        Kanka.GetCampaigns()
        |> fun jel ->
            jel.GetProperty "data"
            |> fun data ->
                data.EnumerateArray()
                |> Seq.iter (
                    fun camp ->
                        let name = camp.GetProperty("name").GetString()
                        let id = camp.GetProperty("id").GetInt32()
                        printfn $"{name} {id}"
                    )
                
    let GetCampaign (id: string) =
        Kanka.GetCampaign(id)
        |>  fun jel ->
                  formatJsonElement jel
                  |> printfn"%s"
               