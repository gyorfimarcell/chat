from enum import Enum
import websockets
import asyncio
import json
from typing import TypedDict


class MessageType(Enum):
    Connect = 0
    Disconnect = 1
    Public = 2


class Message(TypedDict):
    type: int
    sender: str
    text: str

    def __str__(this):
        return f"{this.sender}: {this.text}"


clients = {}


async def ws_server(websocket):
    try:
        async for message in websocket:
            json_message: Message = json.loads(message)

            match MessageType(json_message["type"]):
                case MessageType.Connect:
                    clients[websocket] = json_message["sender"]
                    await broadcast_message(
                        MessageType.Connect,
                        clients[websocket],
                        "",
                    )
                case MessageType.Public:
                    await broadcast_message(
                        MessageType.Public, clients[websocket], json_message["text"]
                    )

    except websockets.exceptions.ConnectionClosedError:
        pass
    finally:
        user = clients[websocket]
        del clients[websocket]
        await broadcast_message(MessageType.Disconnect, user, "")


async def broadcast_message(type: MessageType, sender: str, text: str):
    full_message: Message = {"type": type.value, "sender": sender, "text": text}
    print(full_message)
    [await c.send(json.dumps(full_message)) for c in clients.keys()]


async def main():
    async with websockets.serve(ws_server, "localhost", 7890):
        await asyncio.get_running_loop().create_future()


asyncio.run(main())
