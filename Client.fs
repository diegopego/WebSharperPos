namespace WebSharperTest

open WebSharper
open WebSharper.UI
open WebSharper.UI.Templating
open WebSharper.UI.Notation
open WebSharper.UI.Html
open WebSharperTest.PaymentFormsDomain

[<JavaScript>]
module Templates =

    type MainTemplate = Templating.Template<"Main.html", ClientLoad.FromDocument, ServerLoad.WhenChanged>
    type ReportTemplate = Template<"Report.html", ClientLoad.FromDocument, ServerLoad.WhenChanged>

[<JavaScript>]
module Client =

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

    let RetrieveCashFlowReport () =
        Templates.MainTemplate.ReportForm()
            .OnSend(fun e ->
                async {
                    let! res = Server.GenerateCashFlowReport System.DateTime.Now
                    let renderItem (payment:PaymentForm) = tr [] [ td [] [text (PaymentsTxtRenderer.renderPaymentInTxt payment) ] ]
                    Templates.MainTemplate.ReportTable().ReportRows(
                            List.map renderItem res |> Doc.Concat
                            ).Doc()
                    |> Client.Doc.RunById "report-container"
                } |> Async.StartImmediate
            )
            .Doc()
        
        // render the reults directly 
        // async {
        //     let! res = Server.GenerateCashFlowReport System.DateTime.Now
        //     let renderItem (payment:PaymentForm) = tr [] [ td [] [text (PaymentsTxtRenderer.renderPaymentInTxt payment) ] ]
        //     return Templates.MainTemplate.ReportTable().ReportRows(
        //             List.map renderItem res |> Doc.Concat
        //             ).Doc()
        // }
        // |> Client.Doc.Async