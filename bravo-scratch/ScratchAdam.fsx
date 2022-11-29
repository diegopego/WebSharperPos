// Hi Adam!
//
// I tried WebSharper on Linux and it worked just fine on dotnet 6.
// I am very interested in helping with the project!
//
// I'm building a small proof of concept with a domain close to what I work (ERP).
// Given the following list
// let payments = [ Cash 5.0m; WireTransfer 4.0m; WireTransfer 2.0m; Cash 5m]
//
// I'd like print a report like this:
// Header: Cash
// Cash 5,0
// Cash 5
// Header: Wire transfer
// WireTransfer 4,0
// WireTransfer 2,0
//
// Can you tell me if I'm heading the right way?
// Here is my attempt:


type PaymentMethods = 
    | Cash of decimal
    | WireTransfer of decimal
type PaymentsCategory =
    | PaidInCash
    | PaidInWireTransfer
let categorize = function
    | Cash _ -> PaidInCash
    | WireTransfer _ -> PaidInWireTransfer
let describe = function
    | Cash v -> $"Cash {v}"
    | WireTransfer v -> $"WireTransfer {v}"
let print header records =
    printfn $"Header: {header}"
    records
    |> List.iter (fun x -> printfn $"{describe x}")

[ Cash 5.0m; WireTransfer 4.0m; WireTransfer 2.0m; Cash 5m]
|> List.groupBy(categorize)
|> List.iter(fun x ->
    let paymentList = snd x
    match fst x with
    | PaidInCash -> print "Cash" paymentList
    | PaidInWireTransfer -> print "Wire transfer" paymentList
    )


[Cash 5.0m; WireTransfer 4.0m; WireTransfer 2.0m; Cash 5m]
|> List.groupBy categorize
|> List.iter (fun (paymentType, paymentList) ->
    match paymentType with
    | PaidInCash -> print "Cash" paymentList
    | PaidInWireTransfer -> print "Wire transfer" paymentList
)

[Cash 5.0m; WireTransfer 4.0m; WireTransfer 2.0m; Cash 5m]
|> List.groupBy categorize
|> List.iter (function
    | PaidInCash, paymentList -> print "Cash" paymentList
    | PaidInWireTransfer, paymentList -> print "Wire transfer" paymentList
)

// It seemed that active pattern would extend better to more complex patterns,
// But active pattern have the limitation of maximum 7 patterns.