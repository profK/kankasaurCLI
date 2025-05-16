module kankasaur.JSONIO

open System.Text.Json

let formatJsonElement (jsonElement: JsonElement) =
    let options = JsonSerializerOptions(WriteIndented = true)
    JsonSerializer.Serialize(jsonElement, options)