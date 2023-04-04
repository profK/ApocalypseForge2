module ApocalypseForge2.Moves
open FSharp.Data

type MovesProvider = XmlProvider<Schema="Moves.xsd">
let moves = MovesProvider.Load("Moves.xml")

let moveNames =
    moves.Moves
    |> Seq.map (fun move -> move.Name)

let getMove movename =
    moves.Moves
    |> Seq.find (fun move -> move.Name=movename)


