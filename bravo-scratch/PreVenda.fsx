open System

type Orcamento = {
    Valor: decimal
    Cliente: string
}

type Pedido = {
    Valor: decimal
    Cliente: string
}

type PedidoComNfce = {
    Valor: decimal
    Cliente: string
    ChaveNfce: string
}

type PedidoComNfe = {
    Valor: decimal
    Cliente: string
    ChaveNfe: string
}

type PedidoComNfceNfe = {
    Valor: decimal
    Cliente: string
    ChaveNfce: string
    ChaveNfe: string
}

type Venda =
    | Orcamento of Orcamento
    | Pedido of Pedido
    | PedidoComNfce of PedidoComNfce
    | PedidoComNfe of PedidoComNfe
    | PedidoComNfceNfe of PedidoComNfceNfe


let EmitirNfce (venda) =
    match venda with
    | Orcamento _ -> venda
    | Pedido v -> PedidoComNfce {Valor = v.Valor; Cliente = v.Cliente; ChaveNfce = "12a2a5as619"}
    | PedidoComNfce _ -> venda
    | PedidoComNfe _ -> venda
    | PedidoComNfceNfe _ -> venda

let EmitirNfe (venda) =
    match venda with
    | Orcamento _ -> venda
    | Pedido v -> PedidoComNfe {Valor = v.Valor; Cliente = v.Cliente; ChaveNfe = "nf-e fdseq2487rwe"}
    | PedidoComNfce v -> PedidoComNfceNfe {Valor = v.Valor; Cliente = v.Cliente; ChaveNfce = v.ChaveNfce; ChaveNfe = "nf-e fw8q49561a69"}
    | PedidoComNfe _ -> venda
    | PedidoComNfceNfe _ -> venda

let GerarPedido (venda) =
    match venda with
    | Orcamento v -> Pedido {Valor = v.Valor; Cliente = v.Cliente;}
    | Pedido _ -> venda
    | PedidoComNfce _ -> venda
    | PedidoComNfe _ -> venda
    | PedidoComNfceNfe _ -> venda

let ImprimirVenda (venda) =
    match venda with
    | Orcamento v -> printfn $"orçamento de {v.Cliente} valor {v.Valor}"
    | Pedido v -> printfn $"venda de {v.Cliente} valor {v.Valor}"
    | PedidoComNfce v -> printfn $"pedido com nfc-e de {v.Cliente} valor {v.Valor} nfc-e {v.ChaveNfce}"
    | PedidoComNfe v -> printfn $"pedido com nf-e de {v.Cliente} valor {v.Valor} nf-e {v.ChaveNfe}"
    | PedidoComNfceNfe v -> printfn $"pedido com nfc-e e nf-e de {v.Cliente} valor {v.Valor} nfc-e {v.ChaveNfce} nf-e {v.ChaveNfe}"
    venda

let orc = Orcamento {Valor=1M; Cliente="diego"}
let ped = Pedido {Valor=100M; Cliente="Aurelio"}
orc 
|> EmitirNfce 
|> GerarPedido
|> ImprimirVenda

orc 
|> EmitirNfce 
|> GerarPedido
|> EmitirNfe 
|> ImprimirVenda

ped 
|> ImprimirVenda
|> EmitirNfce 

orc |> ImprimirVenda
ped |> ImprimirVenda