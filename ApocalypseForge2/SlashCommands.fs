module ApocalypseForge2.SlashCommands

open DSharpPlus
open DSharpPlus.Entities
open DSharpPlus.SlashCommands
open ApocalypseForge2.Moves

type SlashCommands() =
    inherit ApplicationCommandModule()
    
    [<SlashCommand("acts", "Lists the available actions and their default stat")>]
    member self.acts(ctx:InteractionContext) =
        InteractionResponseType.DeferredChannelMessageWithSource
        |> ctx.CreateResponseAsync
        |> Async.AwaitTask
        |> ignore
        
        let content =
             moves.Moves
             |> Seq.fold (fun str move -> 
                            str+
                            $"{move.Name.ToLower()} ({move.Statistic.ToLower()})\n"
                         ) ""
        DiscordWebhookBuilder().WithContent(content)
        |> ctx.EditResponseAsync
        |> Async.AwaitTask
        |> ignore

        