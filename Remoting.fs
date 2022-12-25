namespace WebSharperTest

open System
open System.Collections.Generic
open WebSharper
open WebSharperTest.Domain
open WebSharperTest.SalesTransactionDomain

module Server =
    // use a Dictionary to fake a database
    let saleTransactions = Dictionary<SaleTransactionUid.T, SaleTransaction>()
    // initial data 
    saleTransactions.Add((SaleTransactionUid.create (Guid.Parse("b3d8fe04-d8bc-40a8-8044-f06f34f40b84"))), { Uid = SaleTransactionUid.create (Guid.Parse("b3d8fe04-d8bc-40a8-8044-f06f34f40b84")); Datetime = DateTime.Now; Items = [{ Sku = "1"; Description = "Bolt"; Price = 1.5m<Money>; Quantity = 1m<Quantity> }]; Payments = [PaymentMethodsDomain.Money 1.5m<Money>] })
    saleTransactions.Add((SaleTransactionUid.create (Guid.Parse("cca24efe-fffa-4a7f-86fc-38ea41016926"))), { Uid = SaleTransactionUid.create (Guid.Parse("cca24efe-ffff-4a7f-86fc-38ea41016926")); Datetime = DateTime.Now; Items = [{ Sku = "1"; Description = "Bolt"; Price = 1.5m<Money>; Quantity = 2m<Quantity> }; { Sku = "2"; Description = "Blue Wire"; Price = 10m<Money>; Quantity = 1.5m<Quantity> }]; Payments = [PaymentMethodsDomain.Money 13m<Money>] })
    
    let GetSalesTransactions () =
        // get a seq of key-value pairs for easy iteration
        seq {
            for kv in saleTransactions do
                yield (kv.Value)
        }
        |> Seq.toList

    [<Rpc>]
    let DoSomething input =
        let R (s: string) = System.String(Array.rev(s.ToCharArray()))
        async {
            return R input
        }
    [<Rpc>]
    let GenerateCashFlowReport (date:DateTime) =
        async {
            // returns complex data on purpose to demonstrate that you can pass complex data and treat it on the client.
            return GetSalesTransactions()
        }
        
    [<Rpc>]
    let SaleReceipt (saleUid:SaleTransactionUid.T)=
        async {
                return
                    match saleTransactions.TryGetValue(saleUid) with
                        | true, sale -> RenderSaleTransactionReceiptTxt sale
                        | _ -> [$"UID not found: {saleUid.ToString()}"]
            }
    [<Rpc>]
    let PerformSaleTransaction (sale:SaleTransaction)=
        async {
            return
                // What would you return in case of a duplication, or error in you database?
                match saleTransactions.TryAdd(sale.Uid, sale) with
                    | true -> sale.Uid
                    | false -> sale.Uid
        }