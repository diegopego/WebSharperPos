namespace WebSharperTest

open System
open WebSharper
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Templating
open WebSharper.UI.Notation
open WebSharper.UI.Html
open WebSharper.Sitelets
open WebSharper.UI.Server
open WebSharper.Forms
open WebSharperTest.Domain
open WebSharper.MathJS
open WebSharperTest.PaymentFormsDomain
open WebSharperTest.EndPoints

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
    let ShowErrorsFor v =
        v
        |> View.Map (function
            | Success _ -> Doc.Empty
            | Failure errors ->
                Doc.Concat [
                    for error in errors do
                        yield b [attr.style "color:red"] [text error.Text] ] )
        |> Doc.EmbedView
    let transactionItems: Var<List<TransactionItem>> = Var.Create List.empty
    let CartForm () =
        // Form.Return id // id function is equivalent to (fun x -> x)
        // <*> Form.YieldVar transactionItems
        Form.YieldVar transactionItems
        |> Form.WithSubmit
        
    let RegisteredItemsForm () =
        CartForm ()
        |> Form.Render (fun itemsInCart _ ->
            itemsInCart.View
            |> Doc.BindView (fun items ->
                items
                |> List.map (fun item ->
                    div [attr.``class`` "item"] [text item.Description]
                    )
                |> Doc.Concat
                )
            )
        
    let TransactionForm () =
        let transactionUidVar = Var.Create (System.Guid.NewGuid().ToString())
        let priceVar = Var.Create (CheckedInput.Make 0.0)
        let quantityVar = Var.Create (CheckedInput.Make 0.0)
        // <*> compose must be in the same order of the arguments (fun user pass -> user, pass)
        Form.Return (fun transactionUid sku description price quantity -> transactionUid, sku, description, price, quantity)
        <*> (Form.YieldVar transactionUidVar) // transactionUid
        <*> (Form.Yield "" // sku
            |> Validation.IsNotEmpty "Must enter a SKU")
        <*> (Form.Yield "" // description
            |> Validation.IsNotEmpty "Must enter a description")
        <*> (Form.YieldVar priceVar
            |> Validation.Is (fun x -> float x.Input > 0.0) "Price must be positive number"
            |> Validation.Is (fun x -> Math.Round(float x.Input, 2) = float x.Input) "Price must have up to two decimal places"
            )
        <*> (Form.YieldVar quantityVar
            |> Validation.Is (fun x -> float x.Input > 0.0) "Quantity must be positive number"
            |> Validation.Is (fun x -> Math.Round(float x.Input, 2) = float x.Input) "Quantity must have up to two decimal places"
            )
        |> Form.WithSubmit // without this, the validation will run at each keystroke. add the submit button
        // ordem dos argumentos deve ser igual
        |> Form.Run (fun (transactionUid, sku, description, price, quantity) ->
            let priceToPersist:decimal<Money> = (decimal price.Input) * 1.0m<Money>
            let quantityToPersist:decimal<Quantity> = (decimal quantity.Input) * 1.0m<Quantity>
            // // ponto de esclarecimento da unidade de medida:
            // // let total = priceToPersist + quantityToPersist
            // // let total = priceToPersist * quantityToPersist
            // // ponto de esclarecimento. dentro do formulário usa um tipo intermediário, mais relaxado, e aqui colhe os benefícios do tipo do "Domínio"
            let transactionItem:TransactionItem = {Sku=sku; Description=description; Price=priceToPersist; Quantity=quantityToPersist}
            transactionItems.Update(fun items -> List.append items [transactionItem])
            //JS.Alert($"Transaction UID: {transactionUid} Item: {transactionItem.Description} Price: {transactionItem.Price}")
        )
        |> Form.Render (fun transactionUid sku description price quantity submit ->
            // visual representation. fun user pass must be in the same order as Form.Return (fun user pass -> user, pass)
            // inside the representation, the order is meaningless.
            div [] [
                div [] [
                    label [] [text "transactionUid: "]; label [] [text transactionUid.Value]
                ]
                div [] [
                    label [] [text "sku: "]; Doc.Input [] sku
                    ShowErrorsFor (submit.View.Through sku)
                ]
                div [] [
                    label [] [text "description: "]; Doc.Input [] description
                    ShowErrorsFor (submit.View.Through description)
                ]
                div [] [
                    label [] [text "price: "]; Doc.FloatInput [attr.``step`` "0.01"; attr.``min`` "0"] price
                    ShowErrorsFor (submit.View.Through price)
                ]
                div [] [
                    label [] [text "quantity: "]; Doc.FloatInput [attr.``step`` "0.01"; attr.``min`` "0"] quantity
                    ShowErrorsFor (submit.View.Through quantity)
                ]
                Doc.Button "Ok" [] submit.Trigger
            ]
        )
    
    let TransactionArea () =
        Templates.MainTemplate.TransactionArea()
            .RegisterItems(TransactionForm())
            .RegisteredItems(RegisteredItemsForm())
            .Doc()
            
    let PointOfSaleMain () =
        let router = Router.Infer<EndPoint>()
        let location =
            router
            |> Router.Slice (function | SPA spa -> Some spa | _ -> None) EndPoint.SPA
            |> Router.Install SPA.PointOfSale
        location.View.Doc(function
            | SPA.PointOfSale ->
                Doc.Concat [
                    h1 [] [text "SPA point of sale"]
                    a [attr.href (router.Link (EndPoint.SPA (SPA.Point "POS 001")))] [text "link to POS 001"]
                    TransactionArea ()
                ]
            | SPA.Point point ->
                Doc.Concat [
                    h1 [] [text $"SPA point {point}"]
                    a [attr.href (router.Link (EndPoint.SPA (SPA.PointOfSale)))] [text "Back to POS"]
                ]
            )