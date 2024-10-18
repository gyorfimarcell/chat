from enum import Enum
import websockets
import asyncio
import json
from typing import TypedDict


class MessageType(Enum):
    Connect = 0
    Disconnect = 1
    Public = 2
    UserListSync = 3
    Private = 4


class Message(TypedDict):
    type: int
    sender: str
    text: str
    target: str

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

                    await send_message(
                        clients[websocket],
                        MessageType.UserListSync,
                        "",
                        json.dumps(list(clients.values())),
                    )

                case MessageType.Public:
                    await broadcast_message(
                        MessageType.Public, clients[websocket], json_message["text"]
                    )

                case MessageType.Private:
                    await send_message(
                        json_message["target"],
                        MessageType.Private,
                        clients[websocket],
                        json_message["text"],
                        json_message["target"],
                    )
                    await send_message(
                        clients[websocket],
                        MessageType.Private,
                        clients[websocket],
                        json_message["text"],
                        json_message["target"],
                    )

    except websockets.exceptions.ConnectionClosedError:
        pass
    finally:
        user = clients[websocket]
        del clients[websocket]
        await broadcast_message(MessageType.Disconnect, user, "")


async def send_message(
    user: str, type: MessageType, sender: str, text: str, target: str = None
):
    full_message: Message = {
        "type": type.value,
        "sender": sender,
        "text": text,
        "target": target,
    }
    print(full_message)

    client = get_by_username(user)
    await client.send(json.dumps(full_message))


async def broadcast_message(
    type: MessageType, sender: str, text: str, target: str = None
):
    [await send_message(user, type, sender, text, target) for user in clients.values()]


def get_by_username(username: str):
    return [key for key, value in clients.items() if value == username][0]


async def main():
    async with websockets.serve(ws_server, "localhost", 7890):
        await asyncio.get_running_loop().create_future()


asyncio.run(main())
