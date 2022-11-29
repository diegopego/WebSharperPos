open System

let (|ValorMaiorQueZero|_|) input =
    if input > 0m then Some input else None
let ProcessarMaiorQueZero input =
    match input with
    | ValorMaiorQueZero x -> printfn $"{x} é positivo"
    | _ -> printfn $"{input} é negativo"

ProcessarMaiorQueZero -100m
ProcessarMaiorQueZero +100m
