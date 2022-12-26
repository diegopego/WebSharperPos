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
    type PaymentMethod =
        | Money of decimal<Money>
        | CreditCard of CreditCard
[<JavaScript>]        
module PaymentsRender=
    open PaymentMethodsDomain
    let RenderPaymentTxt (method:PaymentMethod) =
        match method with
        | Money x -> $"Money Value {x}"
        | CreditCard x -> $"Credit Card flag:{x.Flag} Value:{x.Value}"