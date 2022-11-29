open System
module FormasDePagamentoDomain=
    // https://fsharpforfunandprofit.com/posts/units-of-measure/
    // https://markheath.net/post/avoid-silly-mistakes-fsharp-units-of-measure
    [<Measure>] type Dinheiro
    type Data = DateOnly

    type Pix = {
        InstituicaoFinanceira : string
        Transacao : string
        Valor : decimal<Dinheiro>
    }
    
    type CartaoDeCredito = {
        InstituicaoFinanceira : string
        Bandeira : string
        Transacao : string
        Valor : decimal<Dinheiro>
    }

    type Cheque = {
        Cnpj : string
        Nome : string
        DataDeVencimento : Data
        Valor : decimal<Dinheiro>
    }

    type TiposDeCrediario =
        |Promissoria
        |Boleto
        |Avulso    

    type Crediario = {
        TipoDeDocumento : TiposDeCrediario
        Nome : string
        DataDeVencimento : Data
        Valor : decimal<Dinheiro>
    }

    type FormaDePagamento =
        | Dinheiro of decimal<Dinheiro>
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
        Dinheiro 10.0m<Dinheiro>
        Pix { InstituicaoFinanceira = "Banco do Brasil"; Transacao = "abc"; Valor = 16.0m<Dinheiro> }
        Crediario {TipoDeDocumento = Promissoria;  Nome = "diego"; DataDeVencimento = DateOnly.FromDateTime(DateTime.UtcNow); Valor = 110m<Dinheiro>}
        Dinheiro 21.0m<Dinheiro>
        Pix { InstituicaoFinanceira = "Bradesco"; Transacao = "dfe"; Valor = 27.0m<Dinheiro> }
        Crediario {TipoDeDocumento = Promissoria;  Nome = "diego"; DataDeVencimento = DateOnly.FromDateTime(DateTime.UtcNow); Valor = 110m<Dinheiro>}
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
