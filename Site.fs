namespace WebSharperTest

open System
open WebSharper
open WebSharper.Forms
open WebSharper.JavaScript
open WebSharper.Sitelets
open WebSharper.UI
open WebSharper.UI.Server
open WebSharperTest.Domain
open WebSharper.MathJS

type EndPoint =
    | [<EndPoint "/">] Home
    | [<EndPoint "/about">] About
    | [<EndPoint "/point-of-sale">] PointOfSale
    | [<EndPoint "/cash-flow">] CashFlow

module Templating =
    open WebSharper.UI.Html

    // Compute a menubar where the menu item for the given endpoint is active
    let MenuBar (ctx: Context<EndPoint>) endpoint : Doc list =
        let ( => ) txt act =
             li [if endpoint = act then yield attr.``class`` "active"] [
                a [attr.href (ctx.Link act)] [text txt]
             ]
        [
            "Home" => EndPoint.Home
            "About" => EndPoint.About
            "Point of sale" => EndPoint.PointOfSale
            "Cash Flow Report" => EndPoint.CashFlow
        ]

    let Main ctx action (title: string) (body: Doc list) =
        Content.Page(
            Templates.MainTemplate()
                .Title(title)
                .MenuBar(MenuBar ctx action)
                .Body(body)
                .Doc()
        )
module Site =
    open WebSharper.UI.Html
    open WebSharper.UI.Client

    open type WebSharper.UI.ClientServer

    let HomePage ctx =
        Templating.Main ctx EndPoint.Home "Home" [
            h1 [] [text "Say Hi to the server!"]
            div [] [client (Client.Main())]
        ]

    let AboutPage ctx =
        Templating.Main ctx EndPoint.About "About" [
            h1 [] [text "About"]
            p [] [text "This is a template WebSharper client-server application."]
        ]
        
    let CashFlowReportPage ctx =
        let title = $"Cash Flow Report"
        Templating.Main ctx EndPoint.CashFlow title [
            div [] [client (Client.RetrieveCashFlowReport())]
        ]
    type TransactionItem = {
        Sku: string
        Description: string
        Price: decimal<Money>
        Quantity: decimal<Quantity>
        }
    type Transaction = {
        Uid: System.Guid
        Datetime: DateTime
        Items: TransactionItem list
        }    
    [<JavaScript>]
    let TransactionForm () =
        let priceVar = Var.Create (CheckedInput.Make 0.0)
        let quantityVar = Var.Create (CheckedInput.Make 0.0)
        // <*> compose must be in the same order of the arguments (fun user pass -> user, pass)
        Form.Return (fun transactionUid sku description price quantity -> transactionUid, sku, description, price, quantity)
        <*> (Form.Yield "" // transactionUid
            |> Validation.IsNotEmpty "Must enter a username")
        <*> (Form.Yield "" // sku
            |> Validation.IsNotEmpty "Must enter a SKU")
        <*> (Form.Yield "") // description
        <*> (Form.YieldVar priceVar
            |> Validation.Is (fun x -> float x.Input > 0.0) "Price must be positive number"
            |> Validation.Is (fun x -> Math.Round(float x.Input, 2) = float x.Input) "Price must have up to two decimal places"
            )
        <*> (Form.YieldVar quantityVar
            |> Validation.Is (fun x -> float x.Input > 0.0) "Quantity must be positive number"
            |> Validation.Is (fun x -> Math.Round(float x.Input, 2) = float x.Input) "Quantity must have up to two decimal places"
            )
        |> Form.WithSubmit // without this, the validation will run at each keystroke. add the submit button
        |> Form.Run (fun (transactionUid, sku, description, price, quantity) ->
            let priceToPersist:decimal<Money> = (decimal price.Input) * 1.0m<Money>
            JS.Alert($"Item price {priceToPersist}")
        )
        |> Form.Render (fun transactionUid sku description price quantity submit ->
            // visual representation. fun user pass must be in the same order as Form.Return (fun user pass -> user, pass)
            // inside the representation, the order is meaningless.
            div [] [
                div [] [label [] [text "transactionUid: "]; Doc.Input [] transactionUid]
                div [] [label [] [text "sku: "]; Doc.Input [] sku]
                div [] [label [] [text "description: "]; Doc.Input [] description]
                div [] [label [] [text "price: "]; Doc.FloatInput [attr.``step`` "0.01"; attr.``min`` "0"] price]
                div [] [label [] [text "quantity: "]; Doc.FloatInput [attr.``step`` "0.01"; attr.``min`` "0"] quantity]
                Doc.Button "Ok" [] submit.Trigger
                div [] [
                    Doc.ShowErrors submit.View (fun errors ->
                        errors
                        |> Seq.map (fun m -> p [] [text m.Text])
                        |> Doc.Concat)
                ]
            ]
        )
    let PointOfSale ctx =
        Templating.Main ctx EndPoint.PointOfSale "Point of sale" [
            h1 [] [text "Point of sale"]
            client (TransactionForm())
        ]

    [<Website>]
    let Main =
        Application.MultiPage (fun ctx endpoint ->
            match endpoint with
            | EndPoint.Home -> HomePage ctx
            | EndPoint.About -> AboutPage ctx
            | EndPoint.PointOfSale -> PointOfSale ctx
            | EndPoint.CashFlow -> CashFlowReportPage ctx
        )
