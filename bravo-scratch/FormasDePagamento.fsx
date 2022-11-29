open System
module FormasDePagamentoDomain=
    // https://fsharpforfunandprofit.com/posts/units-of-measure/
    // https://markheath.net/post/avoid-silly-mistakes-fsharp-units-of-measure
    [<Measure>] type Moeda
    type Data = DateOnly

    type Pix = {
        InstituicaoFinanceira : string
        Transacao : string
        Valor : decimal<Moeda>
    }
    
    type CartaoDeCredito = {
        InstituicaoFinanceira : string
        Bandeira : string
        Transacao : string
        Valor : decimal<Moeda>
    }

    type Cheque = {
        Cnpj : string
        Nome : string
        DataDeVencimento : Data
        Valor : decimal<Moeda>
    }

    type TiposDeCrediario =
        |Promissoria
        |Boleto
        |Avulso    

    type Crediario = {
        TipoDeDocumento : TiposDeCrediario
        Nome : string
        DataDeVencimento : Data
        Valor : decimal<Moeda>
    }

    type FormaDePagamento =
        | Dinheiro of decimal<Moeda>
        | Pix of Pix
        | Cheque of Cheque
        | Crediario of Crediario
        | CartaoDeCredito of CartaoDeCredito
        
    type CategoriaDeFormasDePagamento = PagoEmDinheiro | PagoEmPix | PagoEmCheque | PagomEmCrediario | PagoEmCartaoDeCredito

open FormasDePagamentoDomain

let descrever (forma:FormaDePagamento):string =
    match forma with
    | Dinheiro x -> $"Dinheiro Valor {x}"
    | Pix x -> $"Pix Valor {x.Valor}"
    | Cheque x -> $"Cheque Valor {x.Valor}"
    | Crediario x -> $"Crediário Valor {x.Valor}"
    | CartaoDeCredito x -> $"Cartão de crédito Valor {x.Valor}"
let categorizar = function
    | Dinheiro _ -> PagoEmDinheiro
    | Pix _ -> PagoEmPix
    | Cheque _ -> PagoEmCheque
    | Crediario _ -> PagomEmCrediario
    | CartaoDeCredito _ -> PagoEmCartaoDeCredito
let imprimirBlocoFormaDePagamento cabecalho registros =
    printfn $"----\n{cabecalho}\n----"
    registros
    |> List.iter (fun x -> printfn $"linha {descrever x}")
let pagamentos: FormaDePagamento list=
    [
        Dinheiro 10.0m<Moeda>
        Pix { InstituicaoFinanceira = "Banco do Brasil"; Transacao = "abc"; Valor = 16.0m<Moeda> }
        Crediario {TipoDeDocumento = Promissoria;  Nome = "diego"; DataDeVencimento = DateOnly.FromDateTime(DateTime.UtcNow); Valor = 110m<Moeda>}
        Dinheiro 21.0m<Moeda>
        Pix { InstituicaoFinanceira = "Bradesco"; Transacao = "dfe"; Valor = 27.0m<Moeda> }
        Crediario {TipoDeDocumento = Promissoria;  Nome = "diego"; DataDeVencimento = DateOnly.FromDateTime(DateTime.UtcNow); Valor = 110m<Moeda>}
    ]
let imprimirFluxoDeCaixaSintético (categoriaDePagamento, pagamentos) =
    match categoriaDePagamento with
    | PagoEmDinheiro -> imprimirBlocoFormaDePagamento "Dinheiro" pagamentos
    | PagoEmPix -> imprimirBlocoFormaDePagamento "Pix" pagamentos
    | PagoEmCheque ->  imprimirBlocoFormaDePagamento "Cheque" pagamentos
    | PagomEmCrediario ->  imprimirBlocoFormaDePagamento "Crediário" pagamentos
    | PagoEmCartaoDeCredito ->  imprimirBlocoFormaDePagamento "Cartões de Crédito" pagamentos
    
pagamentos
|> List.groupBy (categorizar)
|> List.sortDescending
|> List.iter imprimirFluxoDeCaixaSintético
