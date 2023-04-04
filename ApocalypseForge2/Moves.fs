module ApocalypseForge2.Moves
open FSharp.Data

type MovesProvider = XmlProvider<Schema="Moves.xsd">
let moves = MovesProvider.Load("Moves.xml")

let moveNames =
    moves.Moves
    |> Seq.map (fun move -> move.Name)

let find_move (inp:string) =
    moves.Moves
    |> Seq.tryFind(fun move -> move.Name.ToLower().StartsWith(inp.ToLower()))
