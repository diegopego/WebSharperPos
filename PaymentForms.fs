namespace WebSharperTest

open System
open WebSharper
[<JavaScript>]
module PaymentFormsDomain=
    [<Measure>] type Money
    type Data = System.DateOnly

    type Pix = {
        FinancialInstitution : string
        TransactionId : string
        Value : decimal<Money>
    }
    
    type CreditCard = {
        Flag : string
        TransactionId : string
        Value : decimal<Money>
    }

    type Cheque = {
        Cnpj : string
        Nome : string
        DataDeVencimento : Data
        Valor : decimal<Money>
    }

    type TiposDeCrediario =
        |Promissoria
        |Boleto
        |Avulso    

    type Crediario = {
        TipoDeDocumento : TiposDeCrediario
        Nome : string
        DataDeVencimento : Data
        Valor : decimal<Money>
    }

    type PaymentForm =
        | Money of decimal<Money>
        | Pix of Pix
        | Cheque of Cheque
        | Crediario of Crediario
        | CreditCard of CreditCard
module PaymentsReporting=
    open PaymentFormsDomain
    let GetPaymentsByDate date =
        [
            Money 10.0m<Money>
            Pix { FinancialInstitution = "Banco do Brasil"; TransactionId = "abc"; Value = 16.0m<Money> }
            CreditCard {Flag = "Mastercard"; TransactionId = ""; Value = 110m<Money>}
            Money 21.0m<Money>
            Pix { FinancialInstitution = "Bradesco"; TransactionId = "dfe"; Value = 27.0m<Money> }
            CreditCard {Flag = "Visa"; TransactionId = ""; Value = 18.19m<Money>}
        ]
    let GenerateCashFlowReport (date:DateTime)=
        GetPaymentsByDate date |> List.sort
[<JavaScript>]        
module PaymentsTxtRenderer=
    open PaymentFormsDomain
    let renderPaymentInTxt (forma:PaymentForm) =
        match forma with
        | Money x -> $"Money Value {x}"
        | Pix x -> $"Pix Value {x.Value}"
        | Cheque x -> $"Cheque Value {x.Valor}"
        | Crediario x -> $"Crediário Value {x.Valor}"
        | CreditCard x -> $"Credit Card flag:{x.Flag} Value:{x.Value}"
        
    let renderizarFluxoDeCaixaAnalíticoTxt (pagamentos:PaymentForm list) =
        pagamentos
        |> List.sort
        |> List.map renderPaymentInTxt
[<JavaScript>]
module PaymentsHtmlRenderer=
    open PaymentFormsDomain
    open WebSharper.UI.Html

    let renderPaymentInHtml (pagamento:PaymentForm)=
        match pagamento with
        | Money x -> tr [] [ td [] [text "Money"]; td [] [ text $"{x}" ] ]
        | Pix x -> tr [] [ td [] [text $"Pix {x.FinancialInstitution}"]; td [] [ text $"{x.Value}" ] ]
        | Cheque x -> tr [] [ td [] [text $"Cheque {x.Cnpj}"]; td [] [ text $"{x.Valor}" ] ]
        | Crediario x -> tr [] [ td [] [text $"Crediário {x.TipoDeDocumento}"]; td [] [ text $"{x.Valor}" ] ]
        | CreditCard x -> tr [] [ td [] [text $"Credit Card {x.Flag}"]; td [] [ text $"{x.Value}" ] ]
        
    let renderCashFlowInHtml (pagamentos:PaymentForm list) =
        pagamentos
        |> List.sort
        |> List.map renderPaymentInHtml