module kankasaur.Campaigns

open System.Reflection
open Kanka.NET.Kanka

module Campaigns =
    let ListCampaigns() =
        GetCampaigns()
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