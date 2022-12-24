namespace WebSharperTest

open System // the order matters. MathJS supersedes some functions.
open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Templating
open WebSharper.UI.Notation
open WebSharper.UI.Html
open WebSharper.Sitelets
open WebSharper.UI.Server
open WebSharper.Forms
open WebSharperTest.Domain
open WebSharperTest.PaymentMethodsDomain
open WebSharperTest.EndPoints
open WebSharperTest.SalesTransactionDomain
open WebSharper.MathJS

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
                    let renderItem (payment:PaymentMethod) = tr [] [ td [] [text (PaymentsTxtRenderer.renderPaymentInTxt payment) ] ]
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
    let ShowErrorsFor v =
        v
        |> View.Map (function
            | Success _ -> Doc.Empty
            | Failure errors ->
                Doc.Concat [
                    for error in errors do
                        yield b [attr.style "color:red"] [text error.Text] ] )
        |> Doc.EmbedView
    let transactionItemsVar: Var<List<TransactionItem>> = Var.Create List.empty
    let transactionUidVar = Var.Create ""
    let CartForm () =
        // simplified way for a form with a single yield
        Form.YieldVar transactionItemsVar
        |> Form.WithSubmit
        
    let RegisteredItemsForm () =
        CartForm ()
        |> Form.Render (fun itemsInCart _ ->
            itemsInCart.View
            |> Doc.BindView (fun items ->
                items
                |> List.map (fun item ->
                    div [attr.``class`` "item"] [
                        text $"Descrição {item.Description} Preço {item.Price}"
                        button [
                            on.click (fun _ _ ->
                                itemsInCart.Update(fun items -> items |> List.filter (fun i -> i <> item))
                            )
                        ] [text "Remove"]
                    ]
                    )
                |> Doc.Concat
                )
            )
    let ItemsToCheckoutForm () =
        CartForm ()
        |> Form.Render (fun itemsInCart _ ->
            // gotcha if you pass this into render, only the last will be rendered, and no error wil be thrown. 
            // label [] [text "first label"]
            // label [] [text "second label"]
            // this renders correctly:
            // div [] [
            //     label [] [text "first label"]
            //     label [] [text "second label"]
            //     ]
            div [] [
                div [] [
                    label [] [text "transactionUid: "]; label [] [text transactionUidVar.Value]
                ]
                itemsInCart.View
                |> Doc.BindView (fun items ->
                    items
                    |> List.map (fun item ->
                        div [attr.``class`` "item"] [
                            text $"Descrição {item.Description} Preço {item.Price}"
                        ]
                        )
                    |> Doc.Concat
                    )
            ]
            )
        
    let TransactionForm () =
        transactionUidVar.Update(fun _ -> System.Guid.NewGuid().ToString())
        let priceVar = Var.Create (CheckedInput.Make 0.0)
        let quantityVar = Var.Create (CheckedInput.Make 0.0)
        // <*> compose must be in the same order of the arguments (fun user pass -> user, pass)
        Form.Return (fun sku description price quantity -> sku, description, price, quantity)
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
        |> Form.Run (fun (sku, description, price, quantity) ->
            let priceToPersist:decimal<Money> = (decimal price.Input) * 1.0m<Money>
            let quantityToPersist:decimal<Quantity> = (decimal quantity.Input) * 1.0m<Quantity>
            // // ponto de esclarecimento da unidade de medida:
            // // let total = priceToPersist + quantityToPersist
            // // let total = priceToPersist * quantityToPersist
            // // ponto de esclarecimento. dentro do formulário usa um tipo intermediário, mais relaxado, e aqui colhe os benefícios do tipo do "Domínio"
            let transactionItem:TransactionItem = {Sku=sku; Description=description; Price=priceToPersist; Quantity=quantityToPersist}
            transactionItemsVar.Update(fun items -> List.append items [transactionItem])
            //JS.Alert($"Transaction UID: {transactionUid} Item: {transactionItem.Description} Price: {transactionItem.Price}")
        )
        |> Form.Render (fun sku description price quantity submit ->
            // visual representation. fun user pass must be in the same order as Form.Return (fun user pass -> user, pass)
            // inside the representation, the order is meaningless.
            div [] [
                div [] [
                    label [] [text "transactionUid: "]; label [] [text transactionUidVar.Value]
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
                Doc.Button "Register" [] submit.Trigger
            ]
        )
    
    let TransactionArea (routerLocation:Var<SPA>) =
        Templates.MainTemplate.TransactionArea()
            .RegisterItems(TransactionForm())
            .RegisteredItems(RegisteredItemsForm())
            .StartPayment(fun _ ->
                routerLocation.Set SPA.Checkout
                )
            .Doc()
    
    type AvailablePaymentMethods =
        | Money
        | CreditCard
    let receiptVar = Var.Create ""
    let PaymentForm (routerLocation:Var<SPA>, backLocation) =
        let paymentMethodVar = Var.Create Money
        let showPaymentMethod (p:AvailablePaymentMethods) =
            $"%A{p}"
        let paymentMethodAmountVar = Var.Create (CheckedInput.Make 0.0)
        Form.Return (fun paymentMethod paymentMethodAmount -> paymentMethod, paymentMethodAmount)
        <*> (Form.YieldVar paymentMethodVar)
        <*> (Form.YieldVar paymentMethodAmountVar
            |> Validation.Is (fun x -> float x.Input > 0.0) "Quantity must be positive number"
            |> Validation.Is (fun x -> Math.Round(float x.Input, 2) = float x.Input) "Quantity must have up to two decimal places"
            )
        |> Form.WithSubmit
        |> Form.Run (fun (paymentMethod, paymentMethodAmount) ->
            let amountToPersist:decimal<Money> = (decimal paymentMethodAmount.Input) * 1.0m<Money>
            let payment =
                match paymentMethod with
                | Money -> PaymentMethod.Money amountToPersist
                | CreditCard -> PaymentMethod.CreditCard {Type = Credit; Flag = "Mastercard"; TransactionId = ""; Value = amountToPersist}
            let transaction:SaleTransaction = {Uid = (SaleTransactionUid (Guid.Parse(transactionUidVar.Value))); Datetime=System.DateTime.Now; Items = transactionItemsVar.Value; Payments = [payment]}
            async {
                let! res = Server.DoSomething "recibo"
                receiptVar := res
                //JS.Alert($"payment done: %A{transaction}")
                routerLocation.Set SPA.Receipt
            } |> Async.StartImmediate
        )
        |> Form.Render (fun paymentMethod paymentMethodAmount submit->
            div [] [
                button [
                        on.click (fun _ _ ->
                            routerLocation.Set backLocation
                        )
                    ] [text "Back"]
                Doc.Button "End transaction" [] submit.Trigger
                div [] [
                    label [] [text "transactionUid: "]; label [] [text transactionUidVar.Value]
                ]
                div [] [
                    Doc.Select [] showPaymentMethod [ Money; CreditCard ]  paymentMethod
                    Doc.FloatInput [attr.``step`` "0.01"; attr.``min`` "0"] paymentMethodAmount
                    ShowErrorsFor (submit.View.Through paymentMethodAmount)
                ]
            ]
        )
        
    let ReceiptForm (routerLocation:Var<SPA>) =
        div [] [
            div [] [
                button [
                    on.click (fun _ _ ->
                        routerLocation.Set SPA.PointOfSale
                    )
                ] [text "New"]
            ]
            async {
                let! res = Server.SaleReceipt (SaleTransactionUid (Guid.Parse(transactionUidVar.Value)))
                let render (receipt) =
                    tr [] [ td [] [text receipt ] ]
                return Templates.MainTemplate.ReportTable().ReportRows(
                    List.map render res |> Doc.Concat
                    ).Doc()
               }
            |> Client.Doc.Async
        ]
            
    let PointOfSaleMain () =
        let router = Router.Infer<EndPoint>()
        let routerLocation =
            router
            |> Router.Slice (function | SPA spa -> Some spa | _ -> None) EndPoint.SPA
            |> Router.Install SPA.PointOfSale
        routerLocation.View.Doc(function
            | SPA.PointOfSale ->
                Doc.Concat [
                    h1 [] [text "SPA point of sale"]
                    TransactionArea (routerLocation)
                ]
            | SPA.Checkout ->
                Doc.Concat [
                    h1 [] [text $"SPA checkout"]
                    // link equivalent to the back button
                    // a [attr.href (router.Link (EndPoint.SPA SPA.PointOfSale))] [text "Back"]
                    button [
                        on.click (fun _ _ ->
                            routerLocation.Set SPA.PointOfSale
                        )
                    ] [text "Back"]
                    button [
                        on.click (fun _ _ ->
                            routerLocation.Set SPA.Payment
                        )
                    ] [text "Proceed to Payment"]
                    ItemsToCheckoutForm()
                ]
            | SPA.Payment ->
                Doc.Concat [
                    h1 [] [text $"SPA payment"]
                    PaymentForm (routerLocation, SPA.Checkout)
                ]
            | SPA.Receipt ->
                Doc.Concat [
                    h1 [] [text $"SPA receipt"]
                    ReceiptForm (routerLocation)
                ]
            )