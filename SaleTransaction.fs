namespace WebSharperTest
open WebSharperTest.Domain
open WebSharperTest.PaymentMethodsDomain
open System
module SalesTransactionDomain=
    
    type SaleTransactionUid = SaleTransactionUid of Guid
    
    type TransactionItem = {
        Sku: string
        Description: string
        Price: decimal<Money>
        Quantity: decimal<Quantity>
        }
    
    type SaleTransaction = {
        Uid: SaleTransactionUid
        Datetime: System.DateTime
        Items: TransactionItem list
        Payments: PaymentMethod list
        }