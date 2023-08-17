# Shoutr
Local Area Network Broadcaster

## TL;DR
Broadcasts data over a LAN for maximum throughput. However, the price we pay for this speed is the reliability of knowing if each listener has received all the data and that the data has not been corrupted. The broadcaster and listeners will have to provide a way to check the data and fill in the gaps.

## Technical Detail
This program fills a very niche need: When a group of local area clients need a copy of the same data at the maximum theoretical rate. We use UDP broadcasts to distribute data. UDP broadcasts will not be forwarded by most routers, therefore this traffic will only be visible within a Local Area Network. UDP does not ensure delivery, order of arrival, or verification. This program will tolerate some late delivery, and will throw out any protocol critical data that has been corrupted. Otherwise, listeners are left to their own interpretation. It is beyond the scope of this program to handle detection of corrupted payload data and the retransmission of that data. 
## Security concerns
It is trivial to forge UDP packets. Therefore, do not run the listener on a network which you don not fully trust.
## examples
### Console Application
#### Listen
`shoutr.exe --listen`
#### Broadcast one file
`shoutr.exe --file="full\path\to\file"`
## Remaining issues
We intend the default method of "filling in the gaps" (missing or bad data) is going to use the BitTorrent protocol. This will be handled outside and separately from this program.
### (Tentative) Workflow for broadcasting
* Listeners begin listening
  - Ideally all listeners would all begin listening before the broadcast, but the application should handle late listeners as best as possible.
* One broadcaster broadcasts the entirety of her message
* Listeners acquire and download a .torrent which is seeded by the broadcaster
  - BitTorrent will handle identifying missing or bad data. It will also take advantage of the fact that different listeners might have successfully received different parts of the broadcast.

## Project Layout
* Shoutr
  * Shoutr.Console
    * Console App which listens and broadcasts
  * Shoutr.Gui
    * Gui App which listens and broadcasts
  * Shoutr.Core
    * Library with all crutial components
  * Shoutr.Core.Interfaces
    * Library with all crutial interfaces
  * Shoutr.Core.Reactive
    * Library implementing reactive interfaces
  * Shoutr.Core.Reactive.Interfaces
    * Library with all reactive interfaces
  * Shoutr.Core.Reactive.Extensions
    * Library extending interfaces with reactive members and methods

## Generating Protobuf Code
Use the protoc tool provided in /bin to (re)generate classes when the message needs to be changed

> protoc -I=$SRC_DIR --csharp_out=$DST_DIR $SRC_DIR/shoutr.proto