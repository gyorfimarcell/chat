import websockets
import asyncio

clients = {}


async def ws_server(websocket):
    try:
        async for message in websocket:
            if message.startswith("[connect]"):
                message = message.replace("[connect]", "", 1)
                clients[websocket] = message
                await broadcast_message(f"{clients[websocket]} connected.", "System")

            elif message.startswith("[public]"):
                message = message.replace("[public]", "", 1)
                await broadcast_message(message, clients[websocket])

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
