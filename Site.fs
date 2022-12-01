namespace WebSharperTest

open System
open System.Transactions
open WebSharper
open WebSharper.JavaScript
open WebSharper.Sitelets
open WebSharper.UI
open WebSharper.UI.Server
open System

type EndPoint =
    | [<EndPoint "/">] Home
    | [<EndPoint "/about">] About
    | [<EndPoint "/cash-flow">] CashFlow of DateTime

module Templating =
    open WebSharper.UI.Html

    // Compute a menubar where the menu item for the given endpoint is active
    let MenuBar (ctx: Context<EndPoint>) endpoint : Doc list =
        let ( => ) txt act =
             li [if endpoint = act then yield attr.``class`` "active"] [
                a [attr.href (ctx.Link act)] [text txt]
             ]
        [
            "Home" => EndPoint.Home
            "About" => EndPoint.About
        ]

    let Main ctx action (title: string) (body: Doc list) =
        Content.Page(
            Templates.MainTemplate()
                .Title(title)
                .MenuBar(MenuBar ctx action)
                .Body(body)
                .Doc()
        )
module Site =
    open WebSharper.UI.Html

    open type WebSharper.UI.ClientServer

    let HomePage ctx =
        Templating.Main ctx EndPoint.Home "Home" [
            h1 [] [text "Say Hi to the server!"]
            div [] [client (Client.Main())]
        ]

    let AboutPage ctx =
        Templating.Main ctx EndPoint.About "About" [
            h1 [] [text "About"]
            p [] [text "This is a template WebSharper client-server application."]
        ]
        
    open PaymentFormsDomain
    open PaymentsHtmlRenderer
        
    let CashFlowReportPage ctx date =
        let title = $"Cash Flow {date}"
        let payments =
            [
                Money 10.0m<Money>
                Pix { FinancialInstitution = "Banco do Brasil"; TransactionId = "abc"; Value = 16.0m<Money> }
                CreditCard {Flag = "Mastercard"; TransactionId = ""; Value = 110m<Money>}
                Money 21.0m<Money>
                Pix { FinancialInstitution = "Bradesco"; TransactionId = "dfe"; Value = 27.0m<Money> }
                CreditCard {Flag = "Visa"; TransactionId = ""; Value = 18.19m<Money>}
            ]
            
        Templating.Main ctx EndPoint.Home title [
            Templates.ReportTemplate()
                .Title(title)
                .TableRows(Doc.Concat (renderCashFlowInHtml payments))
                .Doc()
        ] 

    [<Website>]
    let Main =
        Application.MultiPage (fun ctx endpoint ->
            match endpoint with
            | EndPoint.Home -> HomePage ctx
            | EndPoint.About -> AboutPage ctx
            | EndPoint.CashFlow date -> CashFlowReportPage ctx date
        )
