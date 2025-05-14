module FSCli
open System
open System.CommandLine
open Microsoft.FSharp.Reflection

module FSCli =
    let root = RootCommand("Kanka.Net CLI")
    let rec SubParseDU (duType: Type) (parentCMD: Command) : unit =
        if FSharpType.IsUnion(duType) then
            let unionCases = FSharpType.GetUnionCases(duType)
            for case in unionCases do
                let name = case.Name
                let subCommand = Command(name, "tbd")
                subCommand.SetHandler(fun () -> printfn "Executing %s..." name)
                parentCMD.Add(subCommand)

                // Process fields of the union case
                let fields = case.GetFields()
                for field in fields do
                    let fieldName = field.Name
                    let fieldType = field.PropertyType
                    if FSharpType.IsUnion(fieldType) then
                        // If the field is a union type, recursively parse it
                        let subSubCommand = Command(fieldName, "tbd")
                        subCommand.Add(subSubCommand)
                        SubParseDU fieldType subSubCommand
                    else
                        // Otherwise, add the field as an option
                        let option = Option<string>(fieldName, description = "tbd")
                        subCommand.Add(option)

    let ParseDU (duType: Type) : Command =
        let rootCommand = Command("root", "tbd")
        SubParseDU duType rootCommand
        rootCommand