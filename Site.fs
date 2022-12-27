namespace WebSharperTest

open WebSharper
open WebSharper.Sitelets
open WebSharper.UI
open WebSharper.UI.Server
open WebSharperTest.EndPoints

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
            "Point of sale" => (EndPoint.SPA SPA.PointOfSale)
            "Cash Flow Report" => EndPoint.CashFlow
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
    open WebSharper.UI.Client

    open type WebSharper.UI.ClientServer

    let HomePage ctx =
        Templating.Main ctx EndPoint.Home "Home" [
            text "Here is a comon link to "
            a [attr.href (ctx.Link (EndPoint.SPA SPA.PointOfSale)) ] [text "Point of sale SPA"]
            h1 [] [text "Say Hi to the server!"]
            div [] [client (Client.Main())]
        ]

    let AboutPage ctx =
        Templating.Main ctx EndPoint.About "About" [
            h1 [] [text "About"]
            p [] [text "This is a template WebSharper client-server application."]
        ]
        
    let CashFlowReportPage ctx =
        let title = $"Cash Flow Report"
        Templating.Main ctx EndPoint.CashFlow title [
            div [] [client (Client.RetrieveCashFlowReport())]
        ]
    
    let PointOfSale ctx =
        Templating.Main ctx (EndPoint.SPA SPA.PointOfSale) "Point of sale" [
            div [] [client <@ Client.PointOfSaleMain () @> ]
        ]

    [<Website>]
    let Main =
        Application.MultiPage (fun ctx endpoint ->
            match endpoint with
            | EndPoint.Home -> HomePage ctx
            | EndPoint.About -> AboutPage ctx
            | EndPoint.CashFlow -> CashFlowReportPage ctx
            | EndPoint.SPA _ -> PointOfSale ctx // the _ means that all routes e.g. "/spa/*" will be handed to PointOfSale function. The SPA takes care it's own routes.
        )
