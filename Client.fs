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
    // Thre transaction uid is shared among the different forms using two techniques: a reactive var, and endpoint argument
    // The endpoint argument allows to share the form link and restore the state from it. e.g.:
    // https://localhost:5001/spa/point-of-sale/receipt/5735514d-d544-4890-bd74-b763d025160e
    // You may use both simultaneously. 
    let amountDueVar = Var.Create 0.0m
    let amountDueVarTxt = Var.Create ""
    let UpdateAmountDueVar () =
        let itemsSum = transactionItemsVar.Value
                    |> List.map (fun v -> v.TotaPrice)
                    |> List.sumBy id // id is shorthand to (fun v -> v)
        let total = Math.Round(decimal itemsSum, 2m) // decimal unwraps to decimal, getting rid of the unit of measure
        amountDueVar.Value <- total
        amountDueVarTxt.Value <- $"{total}"
    let transactionUidVar = Var.Create ""
    let StartSaleTransaction ()=
        transactionUidVar.Update(fun _ -> Guid.NewGuid().ToString())
        transactionItemsVar.Update(fun _ -> List.empty)
        UpdateAmountDueVar ()
        
    type CreditCardFormFields = {
        Type : CreditCardType
        Flag : string
        Value : CheckedInput<decimal>
    }
    
    let ValidateCheckedDecimalPositive (f:CheckedInput<decimal>)=
        match f with
        // This should work
        | Valid(value, inputText) -> value > 0.0m
        // This works:
        // | Valid(value, inputText) -> MathJS.Math.Larger(value, 0.0m) |> As<bool>
        //| Valid(value, inputText) -> 
        //    Console.Log("value", value)
        //    let x = System.Decimal(0.0) < value
        //    Console.Log("x", x)
        //    x
        | Invalid _ -> 
            Console.Log("invalid", f)
            false
        | Blank _ -> false
    
    let ValidateCheckedDecimalPlaces places (f:CheckedInput<decimal>) =
        match f with
        // this should work
        | Valid(value, inputText) -> Math.Round(value, places) = value
        //| Valid(value, inputText) -> 
        //    Console.Log("value", value)
        //    let x = Math.Round(value, places)
        //    Console.Log("x", x)
        //    let y = x = value
        //    Console.Log("y", x)
        //    y
        | Invalid _ -> 
            Console.Log("invalid", f)
            false
        | Blank _ -> false
    
    let CheckedInputValue (x:CheckedInput<decimal>)=
        match x with
        | Valid(value, inputText) -> decimal value * 1.0m<Money>
        | Invalid _ -> 0m<Money>
        | Blank _ -> 0m<Money>
    
    let QuantityFromCheckedInput (x:CheckedInput<decimal>)=
        match x with
        | Valid(value, inputText) -> decimal value * 1.0m<Quantity>
        | Invalid _ -> 0m<Quantity>
        | Blank _ -> 0m<Quantity>

    let GuidHead (uid:Guid) =
        let result =
            uid.ToString().Split '-'
            |> Array.tryHead
        
        match result with
            |Some value ->  value
            |None -> ""
    
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

    let CartForm () =
        // simplified way for a form with a single yield
        // Use YieldVar when using a shared reactive Var.
        Form.YieldVar transactionItemsVar
        |> Form.WithSubmit
    
    let RegisteredItemsForm () =
        let AmountDueRv =
            amountDueVarTxt.View
            |> View.Map (fun x -> $"Amount due is {x}")
        
        div [] [
            div [] [
                // You could use textView amountDueVarTxt.View, but amountDueRv shows you how to manipulate the View
                textView AmountDueRv
            ]
            CartForm ()
            |> Form.Render (fun itemsInCart _ ->
                itemsInCart.View
                |> Doc.BindView (fun items ->
                    items
                    |> List.map (fun item ->
                        div [attr.``class`` "item"] [
                            text $"UID: {GuidHead (TransactionItemUid.value item.Uid)} Product {item.Description} Price {item.Price} Quantity {item.Quantity} TotalPrice {item.TotaPrice}"
                            button [
                                on.click (fun _ _ ->
                                    itemsInCart.Update(fun items -> items |> List.filter (fun i -> i <> item))
                                    UpdateAmountDueVar ()
                                )
                            ] [text "Remove"]
                        ]
                        )
                    |> Doc.Concat
                    )
                )
            ]

    let ItemsToCheckoutForm () =
        CartForm ()
        |> Form.Render (fun itemsInCart _ ->
            // gotcha: if you pass this code without the outer div [] [], only the last will be rendered, and no error wil be thrown. 
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
        
    let RegisterItemForm () =
        Form.Return (fun sku description price quantity -> sku, description, price, quantity)
        // Compose the fields by using the <*> function. Make sure to compose them in the same order as Form.Return arguments
        <*> (Form.Yield "" // sku
            |> Validation.IsNotEmpty "Must enter a SKU")
        <*> (Form.Yield "" // description
            |> Validation.IsNotEmpty "Must enter a description")
        <*> (Form.Yield (CheckedInput.Make 0.0m)
            |> Validation.Is (fun x -> ValidateCheckedDecimalPositive x) "Price must be positive number" // This could be simplified to |> Validation.Is ValidateCheckedDecimalPositive "Quantity must be positive number"
            )
        <*> (Form.Yield (CheckedInput.Make 0.0m)
            |> Validation.Is (ValidateCheckedDecimalPlaces 2m) "Quantity must have up to two decimal places"
            )
        |> Form.WithSubmit // without this, the validation will run at each keystroke. add the submit button
        |> Form.Run (fun (sku, description, price, quantity) ->
            // Form.Run arguments must be in the same order as Form.Return
            let transactionItem:TransactionItem = {Uid=TransactionItemUid.create (Guid.NewGuid()) ; Sku=sku; Description=description; Price=CheckedInputValue price; TotaPrice=((CheckedInputValue price) * (QuantityFromCheckedInput quantity)); Quantity=(QuantityFromCheckedInput quantity)}
            transactionItemsVar.Update(fun items -> List.append items [transactionItem])
            UpdateAmountDueVar ()
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
                    label [] [text "Must be positive number: "]; Doc.InputType.Decimal [] price 0.1m
                    ShowErrorsFor (submit.View.Through price)
                ]
                div [] [
                    label [] [text "Only two decimal places: "]; Doc.InputType.Decimal [] quantity 0.1m
                    ShowErrorsFor (submit.View.Through quantity)
                ]
                Doc.Button "Register" [] submit.Trigger
            ]
        )
    
    let TransactionArea (routerLocation:Var<SPA>) =
        Templates.MainTemplate.TransactionArea()
            .RegisterItems(RegisterItemForm())
            .RegisteredItems(RegisteredItemsForm())
            .StartPayment(fun _ ->
                routerLocation.Set SPA.Checkout
                )
            .Doc()

    let CreditCardPaymentForm (init:CreditCardFormFields) =
        Form.Return (fun cardType cardFlag (cardValue:CheckedInput<decimal>) -> {Type = cardType; Flag=cardFlag; Value=cardValue})
        <*> Form.Yield init.Type
        <*> Form.Yield init.Flag
        <*> (Form.Yield (CheckedInput.Make 0.0m)
            |> Validation.Is ValidateCheckedDecimalPositive "Card value must be positive number"
            |> Validation.Is (ValidateCheckedDecimalPlaces 2m) "Card value must have up to two decimal places"
            )
        
    let RenderCreditCardPaymentForm cardType cardFlag cardValue=
        let RenderCardType (p:CreditCardType) =
            $"%A{p}"
        Doc.Concat [
            label [] [text "Pay with Credit Card: "]
            Doc.InputType.Select [] RenderCardType [ Debit; Credit ] cardType
            label [] [text "Card Flag: "]; Doc.InputType.Text [] cardFlag
            label [] [text "Value: "]; Doc.InputType.Decimal [attr.``min`` "0"] cardValue 0.01m
    ]
    
    let receiptVar = Var.Create ""
    
    let PaymentForm (routerLocation:Var<SPA>, backLocation, creditCards:seq<CreditCardFormFields>) =
        Form.Return (fun moneyAmount creditCards -> moneyAmount, creditCards)
        <*> (Form.Yield (CheckedInput.Make amountDueVar.Value)
            |> Validation.Is (ValidateCheckedDecimalPlaces 2m) "Money must have up to two decimal places"
            )
        <*> Form.Many creditCards { Type=Debit; Flag="Visa"; Value=CheckedInput.Make(0.0m) } CreditCardPaymentForm
        |> Form.WithSubmit
        |> Form.Run (fun (moneyAmount, creditCards) ->
            let moneyPayment:list<PaymentMethod> =
                if (CheckedInputValue moneyAmount) > 0m<Money> then
                    [PaymentMethod.Money (CheckedInputValue moneyAmount)]
                else
                    []
            let creditCardPayments =
                creditCards
                |> Seq.toList
                |> List.map (fun x -> PaymentMethod.CreditCard {Type = x.Type; Flag = x.Flag; TransactionId = Guid.NewGuid().ToString(); Value = CheckedInputValue x.Value})
            let payments =
                List.concat [
                    moneyPayment
                    creditCardPayments
                ] |> Seq.toList
            let transaction:SaleTransaction = {Uid = (SaleTransactionUid.create (Guid.Parse(transactionUidVar.Value))); Datetime=System.DateTime.Now; Items = transactionItemsVar.Value; Payments = payments}
            async {
                let! res = Server.PerformSaleTransaction transaction
                // JS.Alert($"Transaction performed: {SaleTransactionUid.value res} %A{transaction}")
                routerLocation.Set (SPA.Receipt transactionUidVar.Value)
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
                    Doc.InputType.Decimal [attr.``min`` "0"] paymentMethodAmount 0.01m
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
        
    let ReceiptForm (uid:string, routerLocation:Var<SPA>) =
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
                let currentTransactionUid = (SaleTransactionUid.create (Guid.Parse(uid)))
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
                    PaymentForm (routerLocation, SPA.Checkout, [|{ Type=Debit; Flag= "MasterCard"; Value= CheckedInput.Make(0.0m) }|])
                ]
            | SPA.Receipt saleUid ->
                Doc.Concat [
                    h1 [] [text $"SPA receipt"]
                    ReceiptForm (saleUid, routerLocation)
                ]
            )
        
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