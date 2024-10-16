import websockets
import asyncio
import json

clients = {}


async def ws_server(websocket):
    try:
        async for message in websocket:
            json_message = json.loads(message)

            if json_message["type"] == 0:
                clients[websocket] = json_message["sender"]
                await broadcast_message(
                    f"{json_message["sender"]} connected.", "System"
                )

            elif json_message["type"] == 2:
                await broadcast_message(json_message["text"], clients[websocket])

    except websockets.exceptions.ConnectionClosedError:
        pass
    finally:
        user = clients[websocket]
        del clients[websocket]
        await broadcast_message(f"{user} disconnected.", "System")


async def broadcast_message(message, sender):
    full_message = f"{sender}: {message}"
    print(full_message)
    [await c.send(full_message) for c in clients.keys()]


async def main():
    async with websockets.serve(ws_server, "localhost", 7890):
        await asyncio.get_running_loop().create_future()


asyncio.run(main())
