namespace WebSharperTest

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Templating
open WebSharper.UI.Notation
open WebSharper.UI.Html
open WebSharper.Sitelets
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
        async {
            let! res = Server.GenerateCashFlowReport System.DateTime.Now
            let renderItem (payment:PaymentForm) = tr [] [ td [] [text (PaymentsTxtRenderer.renderPaymentInTxt payment) ] ]
            // let renderItem (payment:string) = tr [] [ td [] [text (payment) ] ]
            return Templates.MainTemplate.MainTable().TableRows(
                    List.map renderItem res |> Doc.Concat
                    ).Doc()
        }
        |> Client.Doc.Async