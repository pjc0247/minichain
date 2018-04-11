minichain
====

Setup minable node
----
```cs
var node = new Miner();

node.Start();
```

Wallet
----
__Import and Export__
```cs
var json = node.wallet.Export();

node.wallet.Import(json);
```

__Retrive balance__
```cs
var currentBalance = node.wallet.GetBalance();

var balanceAtSpecificBlock = node.wallet.GetBalanceInBlock("BLOCK_HASH");
```

__Create a signed transaction__
```cs
node.wallet.CreateSignedTransaction("RECEIVER_ADDR", VALUE_AMOUNT);
```
