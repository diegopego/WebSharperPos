## Functional, reactive Point of sale with WebSharper

Manny thanks to [Sergey Tihon](https://twitter.com/sergey_tihon) for organizing the [F# Advent](https://sergeytihon.com/category/f-advent/).

## Show me the code!
This is a simple application, ERP like application.
You can check out the code [here](https://github.com/diegopego/WebSharperPos) and a live deployment of the project [here](http://fsadvent2022.audisoft.com.br).

## Live article
This is a live article. The next thing I hope to implement is:
- Postgresql as storage
- Some kind of local storage on the browser, possibly localStorage, sessionStorage and sqlite
- A new article is being prepared: a step-by-step guide based on this project.

## Motivation
The goal of this project is to be a comprehensive example of the most common features used in "enterprise" applications.

It's meant to help all the Clipper (currently Harbour), Delphi, Visual Basic and C# (win forms) developers.

Back in the 80's-90's, there were a mythological creature: The über Full Stack Developer.

Yes. It was common to see a one-man software house with up to hundreds of customers.

With the advent of the browsers and, oh lord... JavaScript, these creatures have faded into the mist of oblivion.

I recently discovered FSharp, and even more recently, WebSharper. A solid project that promise, among other things to let you write your application using one single language again.

Yes, no JavaScript, no DTOs, no NodeJs gazillion files for a single hello world.

Only FSharp. This pure, simple and beautiful piece of art.

### Cross-platform for development and deployment

A [WebSharper](https://websharper.com/) application may be developed and deployed using Linux or Windows.

You'll be aided by great tools like Jetbrains Rider and Visual Studio Code.

### Look Ma! No JavaScript! Oh wait, how about that fancy JS Widget?
You can use external JavaScript libraries. In fact WebSharper have extensions to several including: ExtJS, Highcharts, KendoUI, Sencha, MathJS, JsPDF.

By using the [WebSharper Interface Generator](https://developers.websharper.com/docs/v4.x/fs/wig) you can write your own bindings.

### You may use decimal
It seems like a silly think to say out loud, but there are some languages that make the use of decimal a pain. Not saying names.

## The point of sale
Point of Sale application is not a shopping cart.

The customer goes to the physical store. Real deal. No zoom calls here.

The store attendant welcomes the customer and use the POS to sell.

## Setup
This project is based on the WebSharper client/server template:

`dotnet new websharper-web -lang f# -n ClientServer`

[How to setup your environment](https://websharper.com/downloads)

## Running this project
### Ubuntu 22.04 instructions

I suggest a clean build for the first time:
`dotnet clean`

compiles and run on Debug configuration. Listens on localhost only:

`dotnet run`

Or run on Release configuration. Listens on all interfaces:

`sudo dotnet run --configuration Release --urls http://0.0.0.0:80`

## Creating a Point of Sale Single Page Application
### Application modules
Each URL is an endpoint, which is an application module on a desktop app.
This shows how to setup the EndPoints to a SPA.
Differently of the client/server EndPoints, the SPA EndPoints are handled within the SAP main function: Client.PointOfSaleMain

```fsharp
type SPA =
    | [<EndPoint "/point-of-sale">] PointOfSale
type EndPoint =
    | [<EndPoint "/">] Home
    | [<EndPoint "/spa">] SPA of SPA

let PointOfSale ctx =
    Templating.Main ctx (EndPoint.SPA SPA.PointOfSale) "Point of sale" [
        div [] [client <@ Client.PointOfSaleMain () @> ]
    ]

[<Website>]
let Main =
    Application.MultiPage (fun ctx endpoint ->
        match endpoint with
        | EndPoint.CashFlow -> CashFlowReportPage ctx
        | EndPoint.SPA _ -> PointOfSale ctx // the _ means that all routes e.g. "/spa/*" will be handed to PointOfSale function. The SPA takes care it's own routes.
    )

let PointOfSaleMain () =
    StartSaleTransaction ()
    let router = Router.Infer<EndPoint>()
    let routerLocation =
        router
        |> Router.Slice (function | SPA spa -> Some spa | _ -> None) EndPoint.SPA
        |> Router.Install SPA.PointOfSale
    routerLocation.View.Doc(function
        // SPA EndPoints handlers go here
        )
```

## POS main EndPoint

![alt text](https://audisoft.com.br/diego/fsadvent2022-websharper/pos-main.png "alt text")

Most of the complexity is in the main screen.
It needs to:
- registers items.
- display registered items after registration.
- allow the user to remove registered items from the current sale transaction.
- update the amount to be paid as the items are registered or removed.

This form will be installed in the EndPoint SPA.PointOfSale which handles the "/point-of-sale" URL

```fsharp
let PointOfSaleMain () =
    ...
    routerLocation.View.Doc(function
        | SPA.PointOfSale ->
            Doc.Concat [
                h1 [] [text "SPA point of sale"]
                TransactionArea (routerLocation)]     
```
TransactionArea uses a template.
WebSharper attributes can be identified by the "ws-" prefix.

WebSharper makes them available in the fsharp code automatically as if they were functions or variables. This includes their types.

If you rename any ws- attribute on your template or use a different type, the compiler will give you an error.

In WebSharper, a Doc is the building block for the WebSharper browser interface.

This creates a Doc from a template, by filling the template holes.

The StartPayment, a ws-onclick attribute, is linked to an event handler.

This particular handler redirects the browser to the SPA.CheckOut EndPoint. It's the equivalent to displaying a new form within a Windows Forms application.
```fsharp
let TransactionArea (routerLocation:Var<SPA>) =
    Templates.MainTemplate.TransactionArea()
        .RegisterItems(RegisterItemForm())
        .RegisteredItems(RegisteredItemsForm())
        .StartPayment(fun _ ->
            routerLocation.Set SPA.Checkout
            )
        .Doc()
```

Each ws-hole will be filled with a form.

```html
<div ws-template="TransactionArea" class="flex-container">
    <div class="registerItemsArea">
        <div ws-hole="RegisterItems"></div>
    </div>
    <div class="registeredItemsArea">
        <div> Registered items </div>
        <button ws-onclick="StartPayment">
            Checkout
        </button>
        <div style="margin-top: 15px;">
            <div class="items" ws-hole="RegisteredItems"></div>
        </div>
    </div>
</div>
```

### Register item form -

![alt text](https://audisoft.com.br/diego/fsadvent2022-websharper/pos-register-item-form.png "alt text")

![alt text](https://audisoft.com.br/diego/fsadvent2022-websharper/pos-register-item-form-validation.png "alt text")

Don't be scared with all the html divs on the code that you will be facing. Most of them can and should be replaced with templates.

Unfortunately I did not have the time to do it.

By using `|> Form.WithSubmit`, the form will execute `|> Form.Run` when the submit button is pressed.

On this `Form.Run` block, notice the transactionItemsVar.

This is a reactive var. It can be reactively observed or two-way bound to HTML input elements.

A reactive var is available to all forms. You may think of it as a public or private variable

As you will see in "Registered items form" section, the `CartForm` is bound to `transactionItemsVar` in order to render the registered items list each time it's content is updated.

You don't have to write a single line of JavaScript. You may do so if you wish. I don't. Even when communicating to the server.

```fsharp
let RegisterItemForm () =
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
```

`UpdateAmountDueVar` updates the two reactive Var that, guess what? will be updated automatically in any form bounded to it.

I used two variables on purpose to demonstrate a couple of ways they can be used.

The `amountDueVarTxt` is bounded in the `RegisteredItemsForm`.

It updates the amount due text on the fly, after you register or remove an item. Look for `textView AmountDueRv`.

```fsharp
let UpdateAmountDueVar () =
    ...    
    let total = transactionItemsVar.Value
                |> List.map (fun v -> PriceToFloat v.TotaPrice ) // convert List<TransactionItem> to List<float>
                |> List.sumBy id // id is shorthand to (fun v -> v)
    
    amountDueVar.Value <- total
    amountDueVarTxt.Value <- $"{total}"
```

A Form have it's logic separated from the rendering.

The CartForm is used in two different endpoints by two distinct render functions. It uses Form.YieldVar instead of Yield to use a reactive Var.

On `|> Form.Render`, itemsInCart refers to the transactionItemsVar bound in CartForm function

The lambda function passed on this binding install the function that will be used to render whenever the transactionItemsVar is updated.

```fsharp
itemsInCart.View
|> Doc.BindView (fun items ->
```

This is the "remove registered item from sale" event handler. The ```items``` refers to ```transactionItemsVar``` contents.

```fsharp
on.click (fun _ _ ->
    itemsInCart.Update(fun items -> items |> List.filter (fun i -> i <> item))
```

### Registered items form

![alt text](https://audisoft.com.br/diego/fsadvent2022-websharper/pos-registered-items-form.png "alt text")

```fsharp
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
```

When you hit the "Checkout" Button, the event handler defined on the `TransitionArea().StartPayment()` is triggered
Here is a refresher:
```fsharp
let TransactionArea (routerLocation:Var<SPA>) =
    Templates.MainTemplate.TransactionArea()
        .RegisterItems(RegisterItemForm())
        .RegisteredItems(RegisteredItemsForm())
        .StartPayment(fun _ ->
            routerLocation.Set SPA.Checkout
            )
        .Doc()
```

## Checkout EndPoint

![alt text](https://audisoft.com.br/diego/fsadvent2022-websharper/pos-checkout.png "alt text")

This form does not provide any functionality.
I used it to demonstrate the EndPoint/Form transition, and a how a Form can be rendered using a different function.

```fsharp
routerLocation.View.Doc(function
    ...
    | SPA.Checkout ->
        Doc.Concat [
            h1 [] [text $"SPA checkout"]
            // link equivalent to the back button
            // a [attr.href (router.Link (EndPoint.SPA SPA.PointOfSale))] [text "Back"]
            
            // renders a button that, when clicked, change the browser to the PointOfSale EndPoint ("/") 
            button [
                on.click (fun _ _ ->
                    routerLocation.Set SPA.PointOfSale
                )
            ] [text "Back"]
            
            // renders a button that, when clicked, change the browser to the PointOfSale EndPoint ("/payment")
            button [
                on.click (fun _ _ ->
                    routerLocation.Set SPA.Payment
                )
            ] [text "Proceed to Payment"]
            
            ItemsToCheckoutForm()
        ]
```

Here, `CartForm` is rendered in a different way. The `items` refers to `transactionItemsVar` contents.
```fsharp
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
```
## Payment EndPoint

![alt text](https://audisoft.com.br/diego/fsadvent2022-websharper/pos-payment.png "alt text")

Dynamic forms: It may seem overwhelmingly difficult, but once you got the grasp of it, they're a lot of fun to work with.

One more EndPoints to our collection.
`PaymentForm` third's argument is a sequence of one `CreditCardFormFields`. It's used to initialize the Multiple Credit Cards form. Had I passed two items, there would be two "Pay with Credit Card" lines.

`routerLocation` is responsible to tell the browser to change to an EndPoint. As the code show, it handles this SPA EndPoints.

```fsharp
type CreditCardFormFields = {
    Type : CreditCardType
    Flag : string
    Value : CheckedInput<float>
}

let PointOfSaleMain () =
    StartSaleTransaction ()
    let router = Router.Infer<EndPoint>()
    let routerLocation =
        router
        |> Router.Slice (function | SPA spa -> Some spa | _ -> None) EndPoint.SPA
        |> Router.Install SPA.PointOfSale
    routerLocation.View.Doc(function
        | SPA.PointOfSale -> ...
        | SPA.Checkout -> ...
        | SPA.Payment ->
            Doc.Concat [
                h1 [] [text $"SPA payment"]
                PaymentForm (routerLocation, SPA.Checkout, [|{ Type=Debit; Flag= "MasterCard"; Value= CheckedInput.Make(0.0) }|])
                    ]
```

The Payment Form arguments are:
`backLocation` is used in the Back button event handler.

The user is able to add as many Credit Cars as needed thanks to `Form.Many`.

Let's break down this line:
`<*> Form.Many creditCards { Type=Debit; Flag="Visa"; Value=CheckedInput.Make(0.0) } CreditCardPaymentForm`

`crediCards` The initial collection of values. Check out the `| SPA.Payment` EndPoint for a refresher.

`{ Type=Debit; Flag="Visa"; Value=CheckedInput.Make(0.0) }` The value of type CreditCardFormFields with which the new sub-form should be initialized when the user adds a new Credit Card.

`CreditCardPaymentForm` Is the form that will be rendered when `creditCards.Render (fun ops cardType cardFlag cardValue ->`

```fsharp
let PaymentForm (routerLocation:Var<SPA>, backLocation, creditCards:seq<CreditCardFormFields>) =
        Form.Return (fun moneyAmount creditCards -> moneyAmount, creditCards)
        <*> (Form.Yield (CheckedInput.Make amountDueVar.Value)
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
```
While other forms use reactive Var to share state, this one use EndPoint Argument.
One of the benefits of this is that you can share the url.

EndPoints be of GET or POST in case you're wondering.

## Receipt EndPoint
![alt text](https://audisoft.com.br/diego/fsadvent2022-websharper/pos-receipt.png "alt text")

The Receipt EndPoint definition:

`Receipt of uid: string` defines that this EndPoint have an argument of type string.

The URL will have this format: "https://localhost:5001/spa/point-of-sale/receipt/string-containing-the-sale-uid"

```fsharp
type SPA =
    ...
    | [<EndPoint "/point-of-sale/receipt">] Receipt of saleUid: string
```

The Receipt EndPoint Handler:

```fsharp
let PointOfSaleMain () =
    ...
    routerLocation.View.Doc(function
        ...
        | SPA.Receipt saleUid ->
            Doc.Concat [
                h1 [] [text $"SPA receipt"]
                ReceiptForm (saleUid, routerLocation)
            ]
        )
```

Finally e have some Server action! Brace yourself and prepare to write a bit of JavaScript and some DTOs.

### Just kidding.
I know that wasn't funny.

But writing client/server applications in WebSharper is! It takes care of it all. You write FSharp all the way down.

The serialization is all done for you. You just need to call an RPC basically the same way you would call a local function.

In `Server.SaleReceipt` I chose to let the rendering work to the server, and passing a simple list of strings to the client.

The cash flow Report, on the other hand, the server will deliver a complex type.

Client side:

```fsharp
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
```

Server side:

```fsharp
[<Rpc>]
let SaleReceipt (saleUid:SaleTransactionUid.T)=
    async {
            return
                match saleTransactions.TryGetValue(saleUid) with
                    | true, sale -> RenderSaleTransactionReceiptTxt sale
                    | _ -> [$"UID not found: {saleUid.ToString()}"]
        }
```

## Cash flow report
![alt text](https://audisoft.com.br/diego/fsadvent2022-websharper/pos-cashflow-report.png "alt text")

We exited the SPA realm and are back to the client/server.
The cash flow report EndPoint is defined here:

```fsharp
[<Website>]
let Main =
    Application.MultiPage (fun ctx endpoint ->
        match endpoint with
        | EndPoint.Home -> HomePage ctx
        | EndPoint.About -> AboutPage ctx
        | EndPoint.CashFlow -> CashFlowReportPage ctx
        | EndPoint.SPA _ -> PointOfSale ctx // the _ means that all routes e.g. "/spa/*" will be handed to PointOfSale function. The SPA takes care it's own routes.
    )
```

Here is the cash flow report handler. It is marked to run on the client. Don't worry, we're not back to SPA again.

This EndPoint is intended to open up the cash flow report page.

The user clicks on the Report button.

It then calls the server asynchronously, and receives a list of `<SaleTransaction>`
Finally, the rendering occurs on the client side, thanks to that `client (`

```fsharp
  let CashFlowReportPage ctx =
      let title = $"Cash Flow Report"
      Templating.Main ctx EndPoint.CashFlow title [
          div [] [client (Client.RetrieveCashFlowReport())]
      ]
```

Client side:

```fsharp
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
```

Server side:

```fsharp
[<Rpc>]
let GenerateCashFlowReport (date:DateTime): Async<SaleTransaction list> =
    async {
        // returns complex data on purpose to demonstrate that you can pass complex data and treat it on the client.
        return GetSalesTransactions()
    }
```

The types that are being sent over the RPC call:

```fsharp
type TransactionItem = {
    Uid: TransactionItemUid.T
    Sku: string
    Description: string
    Price: decimal<Money>
    TotaPrice: decimal<Money Quantity> // Unit of Measure that accepts (<Price> times <Money>)
    Quantity: decimal<Quantity>
    }

type SaleTransaction = {
    Uid: SaleTransactionUid.T
    Datetime: System.DateTime
    Items: TransactionItem list
    Payments: PaymentMethod list
    }
```

```fsharp
(SaleTransactionUid.create (Guid.Parse("cca24efe-fffa-4a7f-86fc-38ea41016926"))),
{ Uid = SaleTransactionUid.create (Guid.Parse("cca24efe-ffff-4a7f-86fc-38ea41016926"))
  Datetime = DateTime.Now
  Items =
    [ { Uid = TransactionItemUid.create (Guid.Parse("3f57720d-4b16-4911-88ed-e6d266c72e4a"))
        Sku = "1"
        Description = "Bolt"
        Price = 1.5m<Money>
        TotaPrice = 3.0m<Money Quantity>
        Quantity = 2m<Quantity> }
      { Uid = TransactionItemUid.create (Guid.Parse("1fde5c26-3f00-4916-a5f4-3456bd0b93f2"))
        Sku = "2"
        Description = "Blue Wire"
        Price = 2.0m<Money>
        TotaPrice = 3m<Money Quantity>
        Quantity = 1.5m<Quantity> } ]
  Payments = [ PaymentMethodsDomain.Money 13m<Money> ] }
```

## Resources
- Video: [Introduction to F# web programming with WebSharper by Adam Granicz](https://www.youtube.com/watch?v=CeMq9Fg-HME)
- Video: [Reactive forms and validation with WebSharper](https://skillsmatter.com/skillscasts/17278-lightning-talk-reactive-forms-and-validation-with-websharper)
- [Introduction to Forms](https://github.com/dotnet-websharper/forms/blob/master/docs/Introduction.md)
- [Serving SPAs](https://intellifactory.com/user/granicz/20171229-serving-spas)
- [Reactive forms with WebSharper.Forms](https://intellifactory.com/user/jooseppi12/20211224-reactive-forms-with-websharper-forms)
- [F# for Fun and Profit - Units of measure](https://fsharpforfunandprofit.com/posts/units-of-measure/) 
- [WebSharper CRUD API Sample](https://github.com/websharper-samples/PeopleClient)

## How to proceed from here?
I intend to grow this project so, keep an eye on this article and the github project.

### What if I need some library that are not supported?
- If you mean javascript libraries:
    - https://developers.websharper.com/docs/v4.x/fs/wig
    - also, take a look into some existing lib bindings on https://github.com/dotnet-websharper
- For .net libraries:
    - https://developers.websharper.com/docs/v4.x/fs/proxying
    - https://www.yvesdennels.com/posts/websharper-proxy-project/
    - https://github.com/dotnet-websharper/core/issues/1067 Fear not! Thsi issue is already closed!

You can also ping me on Twitter [Diego Pego](https://twitter.com/sergey_tihon)