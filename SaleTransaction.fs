namespace WebSharperTest
open WebSharperTest.Domain
open WebSharperTest.PaymentMethodsDomain
module SalesTransactionDomain=
    
    type TransactionItem = {
        Sku: string
        Description: string
        Price: decimal<Money>
        Quantity: decimal<Quantity>
        }
    
    type SaleTransaction = {
        Uid: System.Guid
        Datetime: System.DateTime
        Items: TransactionItem list
        Payments: PaymentMethod list
        }