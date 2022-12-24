namespace WebSharperTest

open System
open WebSharper
open WebSharperTest.SalesTransactionDomain

module Server =

    [<Rpc>]
    let DoSomething input =
        let R (s: string) = System.String(Array.rev(s.ToCharArray()))
        async {
            return R input
        }
    [<Rpc>]
    let GenerateCashFlowReport (date:DateTime) =
        async {
            return PaymentsReporting.GenerateCashFlowReport date
            // return ["Credit Card 100.00"; "Cash 50.00"]
        }
        
    [<Rpc>]
    let SaleReceipt (saleUid:SaleTransactionUid)=
        async {
            return [
                $"Transaction UID: {saleUid}"
                "customer name"
                ]
            }