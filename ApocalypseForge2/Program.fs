open System.Threading.Tasks
open ApocalypseForge2
open ApocalypseForge2.SlashCommands
open DSharpPlus
open DSharpPlus.Entities
open DSharpPlus.SlashCommands
open FSharp.Json
open Moves

        
[<EntryPoint>]
let main argv =
    printfn "Starting"
    let sekrets : Map<string,string> =
        System.IO.File.ReadAllText "sekrets.json"
        |>  Json.deserialize
        
    use client = new DiscordClient (
        DiscordConfiguration ( Token = sekrets.["test_token"],
                               TokenType = TokenType.Bot))
    let slashCmds = client.UseSlashCommands()
    slashCmds.RegisterCommands<SlashCommands>()
    
    client.ConnectAsync()
    |> Task.WaitAll

    Task.Delay(-1)
    |> Task.WaitAll
    
    0