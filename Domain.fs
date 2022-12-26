namespace WebSharperTest

// Units of measure prevents you from doing things like this:
// let price = 1m<Money>
// let quantity = 2<Quantity> 
// let total = price + quantity

module Domain=
    [<Measure>] type Money
    [<Measure>] type Quantity