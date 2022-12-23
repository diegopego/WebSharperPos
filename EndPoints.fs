namespace WebSharperTest
open WebSharper

module EndPoints=
    type SPA =
        | [<EndPoint "/point-of-sale">] PointOfSale
        | [<EndPoint "/point-of-sale/checkout">] Checkout
        | [<EndPoint "/point-of-sale/payment">] Payment
    type EndPoint =
        | [<EndPoint "/">] Home
        | [<EndPoint "/about">] About
        | [<EndPoint "/spa">] SPA of SPA
        | [<EndPoint "/cash-flow">] CashFlow