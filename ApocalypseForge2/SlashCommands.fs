module ApocalypseForge2.SlashCommands

open System.Threading.Tasks
open DSharpPlus
open DSharpPlus.Entities
open DSharpPlus.SlashCommands
open ApocalypseForge2.Moves
open ApocalypseForge2.DicePools

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
        
        
    [<SlashCommand("act", "Rolls an action")>]
     member self.acts(ctx:InteractionContext,[<Option("name","action name")>]action,
                      [<Option("pool","dice pool in form Nd6+M")>] pool) =
 
            let do_move_embed_func (themove:MovesProvider.Move) (rollResult:ProcessedRoll) =
                DiscordEmbedBuilder()
                    .WithTitle(themove.Name+" "+rollResult.resultName.ToString())
                    .WithDescription(themove.Description.XElement.Value)
                    .WithColor(rollResult.resultColor)
                    .AddField("Rolls", arrayToCSV(rollResult.rolls))
                    .AddField("Kept", arrayToCSV(rollResult.kept))
                    .AddField("Result",$"{rollResult.result}")
                    .AddField("Result Description:",
                          match rollResult.resultName with
                          | RollResult.CriticalFailure ->
                              themove.CriticalFailure.XElement.Value
                          | RollResult.Failure ->
                              themove.Failure.XElement.Value
                          | RollResult.PartialSuccess ->
                              themove.PartialSuccess.XElement.Value
                          | RollResult.Success ->
                              themove.FullSuccess.XElement.Value
                          | RollResult.CriticalSuccess ->
                              themove.CriticalSuccess.XElement.Value
                          )
                    .Build()
           
            InteractionResponseType.DeferredChannelMessageWithSource
            |> ctx.CreateResponseAsync
            |> Async.AwaitTask
            |> ignore
            
            match parse_pool(pool) with
            | Some dicePool ->
               match find_move(action) with
               |Some themove ->
                   let embed =
                       dicePool
                       |> do_roll
                       |> do_move_embed_func themove
                       
                   DiscordWebhookBuilder().WithContent("Rolled move "+themove.Name)
                       .AddEmbed(embed)
                   |> ctx.EditResponseAsync
                   |> Task.WaitAll
               | None ->
                    DiscordWebhookBuilder().WithContent( "ApocalypseForge Error: could not match move: "+action)
                    |> ctx.EditResponseAsync
                    |> Task.WaitAll                      
            | None ->
               DiscordWebhookBuilder().WithContent("ApocalypseForge Error: could not parse pool expression")
               |> ctx.EditResponseAsync
               |> Task.WaitAll 
     
     [<SlashCommand("pool", "Rolls a dice pool")>]
     member self.pool(ctx:InteractionContext, [<Option("pool","dice pool in form Nd6+M")>] pool) =
         let do_pool_embed_func (rollResult:ProcessedRoll) =
                DiscordEmbedBuilder()
                    .WithTitle("Rolled pool: "+rollResult.resultName.ToString())
                    .WithColor(rollResult.resultColor)
                    .AddField("Rolls", arrayToCSV(rollResult.rolls))
                    .AddField("Kept", arrayToCSV(rollResult.kept))
                    .AddField("Result",$"{rollResult.result}")
                    .Build()
                    
         InteractionResponseType.DeferredChannelMessageWithSource
            |> ctx.CreateResponseAsync
            |> Async.AwaitTask
            |> ignore
            
         match parse_pool(pool) with
         | Some dicePool ->
            let embed =
                   dicePool
                   |> do_roll
                   |> do_pool_embed_func
                   
            DiscordWebhookBuilder().WithContent("Rolled pool "+pool)
               .AddEmbed(embed)
            |> ctx.EditResponseAsync
            |> Task.WaitAll
                               
         | None ->
           DiscordWebhookBuilder().WithContent("ApocalypseForge Error: could not parse pool expression")
           |> ctx.EditResponseAsync
           |> Task.WaitAll            