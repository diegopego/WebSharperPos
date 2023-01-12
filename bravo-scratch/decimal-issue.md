
https://github.com/diegopego/WebSharperPos/blob/9b18d645f0fbd36c7207f0947a6f40d4e03f5790/Client.fs#L55

```
let ValidateCheckedDecimalPositive (f:CheckedInput<decimal>)=
    match f with
    // This should work
    // | Valid(value, inputText) -> value > 0.0m
    | Valid(value, inputText) -> MathJS.Math.Larger(value, 0.0m) |> As<bool>
    | Invalid _ -> false
    | Blank _ -> false
```

https://github.com/diegopego/WebSharperPos/blob/9b18d645f0fbd36c7207f0947a6f40d4e03f5790/Client.fs#L63
```
let ValidateCheckedDecimalPlaces places (f:CheckedInput<decimal>) =
    match f with
    | Valid(value, inputText) ->
        let r = Math.Round(value, places)
        r = value
    | Invalid _ -> false
    | Blank _ -> false
```


With this DecimalGetChecked version, the following occurs:
- `Math.Round(value, places) = value` don't work as expected
  - for example, 1.1 is inputed on the form, `Math.Round(value, places) = value` returns false, while should return true.
- `value > 0.0m` works
```
https://github.com/diegopego/dotnet-websharper-ui/blob/a04dd8a49a72d00392aa39e4b5f3de11b27e9c45/WebSharper.UI/Attr.Client.fs#L443
let DecimalGetChecked : Get<CheckedInput<decimal>> = fun el ->
    let s = el?value
    if String.isBlank s then
        if CheckValidity el then Blank s else Invalid s
    else
        let i = JS.Plus s
        if JS.IsNaN i then Invalid s else Valid (i, s)
    |> Some
```


When using DecimalGetChecked with Bignumber, the following occurs:
- `Math.Round(value, places) = value` works as expected
- `value > 0.0m` gives `Uncaught Error: Cannot compare function values.`
- `MathJS.Math.Larger(value, 0.0m) |> As<bool>` works as expected

https://github.com/diegopego/dotnet-websharper-ui/blob/a04dd8a49a72d00392aa39e4b5f3de11b27e9c45/WebSharper.UI/Attr.Client.fs#L443

```
let DecimalGetChecked : Get<CheckedInput<decimal>> = fun el ->
    let s = el?value
    if String.isBlank s then
        if CheckValidity el then Blank s else Invalid s
    else
        try
            let i : decimal = MathJS.Math.Bignumber(s) |> As<decimal>
            Valid (i, s)
        with
        | _ -> Invalid s
    |> Some
```