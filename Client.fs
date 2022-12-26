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
open WebSharperTest.SaleTransactionDomain
open WebSharper.MathJS

[<JavaScript>]
module Templates =

    type MainTemplate = Templating.Template<"Main.html", ClientLoad.FromDocument, ServerLoad.WhenChanged>
    type ReportTemplate = Template<"Report.html", ClientLoad.FromDocument, ServerLoad.WhenChanged>

[<JavaScript>]
module Client =
    
    let transactionItemsVar: Var<List<TransactionItem>> = Var.Create List.empty
    let transactionUidVar = Var.Create ""
    let StartSaleTransaction ()=
        transactionUidVar.Update(fun _ -> Guid.NewGuid().ToString())
        transactionItemsVar.Update(fun _ -> List.empty)
    
    type CreditCardFormFields = {
        Type : CreditCardType
        Flag : string
        Value : CheckedInput<float>
    }
    let ValidateCheckedFloatPositive (f:CheckedInput<float>)=
        match f with
        | Valid(value, inputText) -> value > 0
        | Invalid _ -> false
        | Blank _ -> false
    let ValidateCheckedFloatDecimalPlaces places (f:CheckedInput<float>) =
        match f with
        | Valid(value, inputText) -> Math.Round(value, places) = value
        | Invalid _ -> false
        | Blank _ -> false
    let MoneyFromCheckedInput (x:CheckedInput<float>)=
        match x with
        | Valid(value, inputText) -> decimal value * 1.0m<Money>
        | Invalid _ -> 0m<Money>
        | Blank _ -> 0m<Money>
    let QuantityFromCheckedInput (x:CheckedInput<float>)=
        match x with
        | Valid(value, inputText) -> decimal value * 1.0m<Quantity>
        | Invalid _ -> 0m<Quantity>
        | Blank _ -> 0m<Quantity>
    let ShowErrorsFor v =
        v
        |> View.Map (function
            | Success _ -> Doc.Empty
            | Failure errors ->
                Doc.Concat [
                    for error in errors do
                        yield b [attr.style "color:red"] [text error.Text] ] )
        |> Doc.EmbedView
    
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
                    let RenderSaleTransaction (sale:SaleTransaction) = tr [] [
                        td [] [text $"{sale.Datetime.ToShortDateString()} - {sale.Datetime.ToShortTimeString()}"]
                        td [] [text $"Transaction UID: {SaleTransactionUid.value sale.Uid}"]
                        td [] [text $"%A{sale.Items}"]
                        td [] [text $"%A{sale.Payments}"]
                    ]
                    Templates.MainTemplate.ReportTable().ReportRows(
                            List.map RenderSaleTransaction res |> Doc.Concat
                            ).Doc()
                    |> Client.Doc.RunById "report-container"
                } |> Async.StartImmediate
            )
            .Doc()

    let CartForm () =
        // simplified way for a form with a single yield
        // Use YieldVar when using a shared reactive Var.
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
                        text $"UID: {TransactionItemUid.value item.Uid} Product {item.Description} Price {item.Price} Quantity {item.Quantity} TotalPrice {item.TotaPrice}"
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
                            text $"Product {item.Description} Price {item.Price} TotalPrice {item.TotaPrice}"
                        ]
                        )
                    |> Doc.Concat
                    )
            ]
            )
        
    let TransactionForm () =
        Form.Return (fun sku description price quantity -> sku, description, price, quantity)
        // Compose the fields by using the <*> function. Make sure to compose them in the same order as Form.Return arguments
        <*> (Form.Yield "" // sku
            |> Validation.IsNotEmpty "Must enter a SKU")
        <*> (Form.Yield "" // description
            |> Validation.IsNotEmpty "Must enter a description")
        <*> (Form.Yield (CheckedInput.Make 0.0)
            |> Validation.Is (fun x -> ValidateCheckedFloatPositive x) "Price must be positive number" // This could be simplified to |> Validation.Is ValidateCheckedFloatPositive "Quantity must be positive number"
            |> Validation.Is (fun x -> ValidateCheckedFloatDecimalPlaces 2 x) "Price must have up to two decimal places" // This could be simplified to |> Validation.Is (ValidateCheckedFloatDecimalPlaces 2) "Quantity must have up to two decimal places"
            )
        <*> (Form.Yield (CheckedInput.Make 0.0)
            |> Validation.Is ValidateCheckedFloatPositive "Quantity must be positive number"
            |> Validation.Is (ValidateCheckedFloatDecimalPlaces 2) "Quantity must have up to two decimal places"
            )
        |> Form.WithSubmit // without this, the validation will run at each keystroke. add the submit button
        |> Form.Run (fun (sku, description, price, quantity) ->
            // Form.Run arguments must be in the same order as Form.Return
            // At the time this was written, there is no support decimal input.
            // Here is the place to convert CheckedInput<float> to the type needed by the RPC (Remote Procedure Call)
            let transactionItem:TransactionItem = {Uid=TransactionItemUid.create (Guid.NewGuid()) ; Sku=sku; Description=description; Price=MoneyFromCheckedInput price; TotaPrice=((MoneyFromCheckedInput price) * (QuantityFromCheckedInput quantity)); Quantity=(QuantityFromCheckedInput quantity)}
            transactionItemsVar.Update(fun items -> List.append items [transactionItem])
            //JS.Alert($"Transaction UID: {transactionUid} Item: {transactionItem.Description} Price: {transactionItem.Price}")
        )
        |> Form.Render (fun sku description price quantity submit ->
            // Form.Render arguments musb be in the same order as Form.Return
            // The submit argument is passed by the Form.WithSubmit
            div [] [
                div [] [
                    label [] [text "transactionUid: "]; label [] [text transactionUidVar.Value]
                ]
                div [] [
                    label [] [text "sku: "]; Doc.InputType.Text [] sku
                    ShowErrorsFor (submit.View.Through sku)
                ]
                div [] [
                    label [] [text "description: "]; Doc.InputType.Text [] description
                    ShowErrorsFor (submit.View.Through description)
                ]
                div [] [
                    label [] [text "price: "]; Doc.InputType.Float [attr.``step`` "0.01"; attr.``min`` "0"] price
                    ShowErrorsFor (submit.View.Through price)
                ]
                div [] [
                    label [] [text "quantity: "]; Doc.InputType.Float [attr.``step`` "0.01"; attr.``min`` "0"] quantity
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
    let CreditCardPaymentForm (init:CreditCardFormFields) =
        Form.Return (fun cardType cardFlag (cardValue:CheckedInput<float>) -> {Type = cardType; Flag=cardFlag; Value=cardValue})
        <*> Form.Yield init.Type
        <*> Form.Yield init.Flag
        <*> (Form.Yield (CheckedInput.Make 0.0)
            |> Validation.Is ValidateCheckedFloatPositive "Card value must be positive number"
            |> Validation.Is (ValidateCheckedFloatDecimalPlaces 2) "Card value must have up to two decimal places"
            )
        
    let RenderCreditCardPaymentForm cardType cardFlag cardValue=
        let RenderCardType (p:CreditCardType) =
            $"%A{p}"
        Doc.Concat [
            label [] [text "Pay with Credit Card: "]
            Doc.InputType.Select [] RenderCardType [ Debit; Credit ] cardType
            label [] [text "Card Flag: "]; Doc.InputType.Text [] cardFlag
            label [] [text "Value: "]; Doc.InputType.Float [attr.``step`` "0.01"; attr.``min`` "0"] cardValue
    ]
    
    let receiptVar = Var.Create ""
    let PaymentForm (routerLocation:Var<SPA>, backLocation, creditCards:seq<CreditCardFormFields>) =
        let PriceToFloat (p:decimal<Money Quantity>) =
            // unwrap to decimal, getting rid of unit of measure, then convert to float. because at this time, decimal is not supported by WebSharper.Forms
            p |> decimal |> float
            
        let CalculateAmountDue =
            transactionItemsVar.Value
            |> List.map (fun v -> PriceToFloat v.TotaPrice ) // convert List<TransactionItem> to List<float>
            |> List.sumBy id // id is shorthand to (fun v -> v)
        
        Form.Return (fun moneyAmount creditCards -> moneyAmount, creditCards)
        <*> (Form.Yield (CheckedInput.Make CalculateAmountDue)
            |> Validation.Is (ValidateCheckedFloatPositive) "Money must be positive number"
            |> Validation.Is (ValidateCheckedFloatDecimalPlaces 2) "Money must have up to two decimal places"
            )
        <*> Form.Many creditCards { Type=Debit; Flag="Visa"; Value=CheckedInput.Make(0.0) } CreditCardPaymentForm
        |> Form.WithSubmit
        |> Form.Run (fun (moneyAmount, creditCards) ->
            let moneyPayment:list<PaymentMethod> =
                if (MoneyFromCheckedInput moneyAmount) > 0m<Money> then
                    [PaymentMethod.Money (MoneyFromCheckedInput moneyAmount)]
                else
                    []
            let creditCardPayments =
                creditCards
                |> Seq.toList
                |> List.map (fun x -> PaymentMethod.CreditCard {Type = x.Type; Flag = x.Flag; TransactionId = Guid.NewGuid().ToString(); Value = MoneyFromCheckedInput x.Value})
            let payments =
                List.concat [
                    moneyPayment
                    creditCardPayments
                ] |> Seq.toList
            let transaction:SaleTransaction = {Uid = (SaleTransactionUid.create (Guid.Parse(transactionUidVar.Value))); Datetime=System.DateTime.Now; Items = transactionItemsVar.Value; Payments = payments}
            async {
                let! res = Server.PerformSaleTransaction transaction
                // JS.Alert($"Transaction performed: {SaleTransactionUid.value res} %A{transaction}")
                routerLocation.Set SPA.Receipt
            } |> Async.StartImmediate
        )
        |> Form.Render (fun paymentMethodAmount creditCards submit->
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
                    label [] [text "Money"]
                    Doc.InputType.Float [attr.``step`` "0.01"; attr.``min`` "0"] paymentMethodAmount
                    ShowErrorsFor (submit.View.Through paymentMethodAmount)
                ]
                div [] [
                    creditCards.Render (fun ops cardType cardFlag cardValue ->
                        div [] [
                            RenderCreditCardPaymentForm cardType cardFlag cardValue
                            Doc.Button "Delete" [] ops.Delete
                            ShowErrorsFor (submit.View.Through cardValue)
                        ]
                        )
                    Doc.Button "Add Payment Form" [] creditCards.Add
                ]
            ]
        )
        
    let ReceiptForm (routerLocation:Var<SPA>) =
        div [] [
            div [] [
                button [
                    on.click (fun _ _ ->
                        StartSaleTransaction ()
                        routerLocation.Set SPA.PointOfSale
                    )
                ] [text "New"]
            ]
            async {
                let currentTransactionUid = (SaleTransactionUid.create (Guid.Parse(transactionUidVar.Value)))
                let! res = Server.SaleReceipt currentTransactionUid
                let render line =
                    tr [] [ td [] [text line ] ]
                return Templates.MainTemplate.ReportTable().ReportRows(
                    List.map render res |> Doc.Concat
                    ).Doc()
               }
            |> Client.Doc.Async
        ]
            
    let PointOfSaleMain () =
        StartSaleTransaction ()
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
                    PaymentForm (routerLocation, SPA.Checkout, [|{ Type=Debit; Flag= "MasterCard"; Value= CheckedInput.Make(0.0) }|])
                ]
            | SPA.Receipt ->
                Doc.Concat [
                    h1 [] [text $"SPA receipt"]
                    ReceiptForm (routerLocation)
                ]
            )