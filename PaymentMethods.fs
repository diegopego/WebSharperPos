namespace WebSharperTest

open System
open WebSharper
open WebSharperTest.Domain

[<JavaScript>]
module PaymentMethodsDomain=
    type CreditCardType =
        |Debit
        |Credit    
    type CreditCard = {
        Type : CreditCardType
        Flag : string
        TransactionId : string
        Value : decimal<Money>
    }
    type PaymentForm =
        | Money of decimal<Money>
        | CreditCard of CreditCard
module PaymentsReporting=
    open PaymentMethodsDomain
    let GetPaymentsByDate date =
        [
            Money 10.0m<Money>
            CreditCard {Type = Debit; Flag = "Mastercard"; TransactionId = ""; Value = 110m<Money>}
            Money 21.0m<Money>
            CreditCard {Type = Credit; Flag = "Visa"; TransactionId = ""; Value = 18.19m<Money>}
        ]
    let GenerateCashFlowReport (date:DateTime)=
        GetPaymentsByDate date |> List.sort
[<JavaScript>]        
module PaymentsTxtRenderer=
    open PaymentMethodsDomain
    let renderPaymentInTxt (forma:PaymentForm) =
        match forma with
        | Money x -> $"Money Value {x}"
        | CreditCard x -> $"Credit Card flag:{x.Flag} Value:{x.Value}"
        
    let renderizarFluxoDeCaixaAnalíticoTxt (pagamentos:PaymentForm list) =
        pagamentos
        |> List.sort
        |> List.map renderPaymentInTxt
[<JavaScript>]
module PaymentsHtmlRenderer=
    open PaymentMethodsDomain
    open WebSharper.UI.Html

    let renderPaymentInHtml (pagamento:PaymentForm)=
        match pagamento with
        | Money x -> tr [] [ td [] [text "Money"]; td [] [ text $"{x}" ] ]
        | CreditCard x -> tr [] [ td [] [text $"Credit Card {x.Flag}"]; td [] [ text $"{x.Value}" ] ]
        
    let renderCashFlowInHtml (pagamentos:PaymentForm list) =
        pagamentos
        |> List.sort
        |> List.map renderPaymentInHtml