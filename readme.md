TODO
===

```
[ ] Use strict type for bitstamp instruments
[ ] Remap currencies Lykke <-> Bitstamp
[x] Figure out why RetryWithBackoff doesn't work (hangs) if referenced from common library
[ ] Trading Api
    [x] GetOpenOrders
        [x] Parse asset
    [x] Cancel Limit Order
    [x] Create Limit Order
        [x] Buy
        [x] Sell
    [x] Parse errors and return proper codes
    [x] Get wallets
    [x] Cancel order
        [x] Duplicate cancel request shouldn't fail
    [x] Get order
        [x] Implement intermediate storage
        [x] Parse transactions 
```