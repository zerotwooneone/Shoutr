# Shoutr
Local Area Network Broadcaster

## TL;DR
Broadcasts data over a LAN for maximum throughput. However, the price we pay for this speed is the reliability of knowing if each listener has received all the data and that the data has not been corrupted. The broadcaster and listeners will have to provide a way to check the data and fill in the gaps.

## Technical Detail
This program uses UDP broadcasts to distribute data. UDP broadcasts will not be forwarded by any router, therefore this traffic will only be visible within a Local Area Network. UDP does not ensure delivery, order of arrival, or verification. This program will tolerate some late delivery, and will verify that protocol critical data has not been corrupted. Otherwise, listeners are left to their own interpretation. 
## Security concerns
It is trivial to forge UDP packets. Therefore, do not run the listener on a network which you don not fully trust.
## examples
### Console Application
#### Listen
`shoutr.exe --listen`
#### Broadcast one file
`soutr.exe --file="full\path\to\file"`
## Remaining issues
We intend the default method of "filling in the gaps" (missing or bad data) is going to use the BitTorrent protocol. This will be handled outside and separately from this program.
### (Tentative) Workflow for broadcasting
* Listeners begin listening
  - Ideally all listeners would all begin listening before the broadcast, but the application should handle late listeners as best as possible.
* One broadcaster broadcasts the entirety of her message
* Listeners aquire and download a .torrent which is seeded by the broadcaster
