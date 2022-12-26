namespace WebSharperTest
open WebSharper
open WebSharperTest.Domain
open WebSharperTest.PaymentMethodsDomain
open WebSharperTest.PaymentsRender
open System
[<JavaScript>]
module SaleTransactionDomain=
    
    // this is a case of wrapping and wrapping single case unions
    // this code would be as simple as
    // type SaleTransactionUid = SaleTransactionUid of Guid
    // let uid = (SaleTransactionUid (Guid.NewGuid()))
    // let uidAsString = $"{uid}" // on the server side it renders something like "SaleTransactionUid cb970d83-43d9-4f1c-a530-1b692a36a018". on the client side, it renders "obj obj"
    // In order to render it correctly on the client side, you must unwrap the single case union:
    // let unwrap (SaleTransactionUid v) = v
    // let uidAsString = $"{unwrap uid}"
    // This module encapsulates this functionality
    // read more on https://fsharpforfunandprofit.com/posts/designing-with-types-single-case-dus/
    module SaleTransactionUid =
        type T = SaleTransactionUid of Guid

        // wrap
        let create (guid:Guid) =
            SaleTransactionUid guid

        // unwrap
        let value (SaleTransactionUid guid) = guid
    module TransactionItemUid =
        type T = TransactionItemUid of Guid

        let create (guid:Guid) =
            TransactionItemUid guid

        let value (TransactionItemUid guid) = guid
    
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

[<JavaScript>]        
module SaleTransactionRender=
    open SaleTransactionDomain
    let RenderSaleTransactionReceiptTxt sale = List.concat [
        [
            $"Transaction UID: {sale.Uid}"
            $"Date: {sale.Datetime}"
        ]
        sale.Items |> List.map (fun item -> $"%A{item}")
        sale.Payments |> List.map RenderPaymentTxt
        [
            $"sale object used to render this receipt:"
            $"%A{sale}"
        ]
    ]