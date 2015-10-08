ByteChannel
------------

ByteChannel is a protocol to send bytes over an unreliable source reliably, while using only one byte per packet header.

ByteChannel is originally designed to operate in a game called EverybodyEdits, but with a few adjustments to the protocol, it could even be used to replace TCP!

The protocol can be devided into 4 layers. These layers will be described in this document.

## Layer 1
The first layer works around the limitation that only a fixed amount of bytes can be embedded in EE messages. If packet sizes are not limited, the implementation of this layer can be skipped.

Variable packet sizes is archived by simply padding the packet data with empty bytes. This also means that the first data byte must not be a zero byte. When decoding, all empty bytes from the start of a packet are stripped off of it.

Example:
Data:
> 0000 0001

Transformed for transfer: 
> 0000 0000 0000 0000 0000 0000 0000 0001

All empty bytes removed:
> 0000 0001

## Layer 2
The second layer handles packets that fail to arrive or arrive out of order. It does this by padding a counter byte to the start of every packet. This byte starts from 1 (due to the requirement by layer 1) and goes to 255, where the counter is reset back to 1. This means that there can be a maximum of 255 packets travelling the network at one point.
Layer 2 relies on the fact that successfully sent messages are received by the sender (this is the behavior in EE). It is very trivial to resend messages that have failed to send after a certain period of time passes. If sent messages are not re-received, an additional implementation of acknowledgement packets might be necessary.

Example:
Data:
> 0000 0001
> 0000 0010
> 0000 0011

Transferred data: 
>  0000 0000 0000 0000 0000 0001 0000 0001
>  \*missed packet*
>  0000 0000 0000 0000 0000 0011 0000 0011
>  0000 0000 0000 0000 0000 0010 0000 0010

Restored ordering:
> 0000 0001
> 0000 0010
> 0000 0011


## Layer 3
Larger chunks of data can be transferred by splitting these into smaller parts that fit in messages.

## Layer 4
Due to the fact that the protocol's messages are transmitted to multiple receivers in EE, some recipients might not be present from the start. This means that messages must be split in meaningful parts so that newly joined users can jump in and participate in the transmission!
ByteChannel allows the user of the protocol to request the transmission of a byte array. This the size of this byte array will be base128 encoded (little endian varint, see protobuf) and prepended to the byte array. This will be passed to layer 3 and eventually sent over the protocol.
In addition to this, whenever the presence of a new user is detected, a reset message must be sent. This is just an empty layer 3 message, and marks the start of the next message. A reset message must not be sent if there is no sign of any new users, because of this, new users must at least send a reset message of their own when they join to announce that they support this protocol.

