module ApocalypseForge2.DicePools

open System
open System.Drawing
open System.Text.RegularExpressions
open DSharpPlus.Entities

type DicePool =
    {
        d6:int
        plus:int
    }
type RollResult =
    | CriticalFailure
    | Failure 
    | PartialSuccess
    | Success
    | CriticalSuccess
    
type ProcessedRoll =
    struct
        val pool:DicePool
        val resultName:RollResult
        val resultColor:DiscordColor
        val rolls: int array
        val kept:int array
        val result:int
        new (Pool,ResultName,ResultColor,Rolls,Kept,Result) =
            {pool=Pool;resultName=ResultName;resultColor=ResultColor
             rolls=Rolls;kept=Kept;result=Result}
    end
    
type Random with
    // Generates an infinite sequence of random numbers within the given range.
    member this.GetValues(minValue, maxValue) =
        Seq.initInfinite (fun _ -> this.Next(minValue, maxValue))
        
let infiniRand = Random()

let arrayToCSV numarray =
        let str = $"%A{numarray}"
        str.Substring(2,str.Length-4).Replace(";",",")

        
let poolParser = Regex("(\d+)d6([+,-]\d+)?",RegexOptions.Compiled)
let parse_pool instr:DicePool option =
   let matches:Match = poolParser.Match(instr)
   match matches.Success with
   | false -> None
   | true ->
       let groups = matches.Groups
       let poolDice = int(groups[1].Value)
       match groups[2].Success with
       | false ->
           Some({
            d6=poolDice
            plus=0
           })
       | true ->
           let plus = int(groups[2].Value)
           Some({
            d6=poolDice
            plus=plus
           })
 
let do_roll pool  =
    let numd6 = if pool.d6<2 then 3 else pool.d6
    let rolls =
        infiniRand.GetValues(1,7) |> Seq.take numd6 |> Seq.toArray   
    let kept =
        if pool.d6<2 then
            rolls |> Array.sort |> Array.take 2 
        else
            rolls |> Array.sortDescending |> Array.take 2
    let result =
        kept |> Seq.sum |> fun tot -> tot + pool.plus
    let resultName =
         if (kept[0]=6)&&(kept[1]=6) then
            CriticalSuccess
         else if (kept[0]=1)&&(kept[1]=1) then
             CriticalFailure
         else match result with
            | n when  n<7 -> Failure
            | n when n>6 && n<10 -> PartialSuccess
            | n when n>9 -> Success
    let resultColor =
        match resultName with
        | CriticalFailure -> DiscordColor.Red
        | Failure ->DiscordColor.DarkRed
        | PartialSuccess -> DiscordColor.DarkGreen
        | Success -> DiscordColor.Green
        | CriticalSuccess -> DiscordColor.Gold
    ProcessedRoll(pool,resultName,resultColor,rolls,kept,result)   