minichain
====

Minimal implementation of __BLOCKCHAIN__, written in __CSharp__.

![a](prev.gif)

Sub repositories
----
* [Demonstrate 51% attack on blockchain network](https://github.com/pjc0247/minichain_51attack_demo)
* [State DB implementation](https://github.com/pjc0247/minichain_state_db)

ThingsToDo
----
* __p2p__
  * [x] Exchange peers
  * [ ] Local Discovery
* __chain__
  * __consensus__
    * [ ] Block confirmation
  * __mining__
    * [ ] Block rewards
    * [x] Dynamic difficulty
  * __wallet__
    * [ ] implements overall wallet system.
    * [x] Transaction signing
  * __txpool__
    * [ ] implements overall txpool system.
* __PoW__
  * [x] Include transactions with highest fees
  * [ ] More elaborated multithread mining


Specification
----
* __Block Structure__

* __Block Validation__
  * `txs` must be a non empty array (except genesis-block)
  * `txs[0]` must be a reward transaction.
  * Check the block has valid minerAddress
  * Check the block proper difficulty
  * Check the nonce with block difficulty
