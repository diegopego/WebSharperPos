namespace WebSharperTest

open System
open WebSharper
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Templating
open WebSharper.UI.Notation
open WebSharper.UI.Html
open WebSharper.Forms
open WebSharper.JavaScript
open WebSharperTest.Domain
open WebSharper.MathJS
open WebSharperTest.PaymentFormsDomain

[<JavaScript>]
module Templates =

    type MainTemplate = Templating.Template<"Main.html", ClientLoad.FromDocument, ServerLoad.WhenChanged>
    type ReportTemplate = Template<"Report.html", ClientLoad.FromDocument, ServerLoad.WhenChanged>

[<JavaScript>]
module Client =

    let Main () =
        let rvReversed = Var.Create ""
        Templates.MainTemplate.MainForm()
            .OnSend(fun e ->
                async {
                    let! res = Server.DoSomething e.Vars.TextToReverse.Value
                    rvReversed := res
                }
                |> Async.StartImmediate
            )
            .Reversed(rvReversed.View)
            .Doc()

    let RetrieveCashFlowReport () =
        Templates.MainTemplate.ReportForm()
            .OnSend(fun e ->
                async {
                    let! res = Server.GenerateCashFlowReport System.DateTime.Now
                    let renderItem (payment:PaymentForm) = tr [] [ td [] [text (PaymentsTxtRenderer.renderPaymentInTxt payment) ] ]
                    Templates.MainTemplate.ReportTable().ReportRows(
                            List.map renderItem res |> Doc.Concat
                            ).Doc()
                    |> Client.Doc.RunById "report-container"
                } |> Async.StartImmediate
            )
            .Doc()
        
        // render the reults directly 
        // async {
        //     let! res = Server.GenerateCashFlowReport System.DateTime.Now
        //     let renderItem (payment:PaymentForm) = tr [] [ td [] [text (PaymentsTxtRenderer.renderPaymentInTxt payment) ] ]
        //     return Templates.MainTemplate.ReportTable().ReportRows(
        //             List.map renderItem res |> Doc.Concat
        //             ).Doc()
        // }
        // |> Client.Doc.Async
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