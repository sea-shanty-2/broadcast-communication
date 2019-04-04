import asyncio
import json
import websockets
from loguru import logger

clients = dict()


class Client:
    def __init__(self, name, avatar):
        self.name = name
        self.avatar = avatar


async def handle_messages(websocket):
    while True:
        packet = json.loads(await websocket.recv())
        logger.info(f'Received {packet}')

        if packet['type'] != 'message':
            continue

        message = packet['message']

        others = [socket for socket in clients if socket != websocket]
        if others:
            packet = json.dumps(
                dict(type='message', author=clients[websocket].name, avatar=clients[websocket].avatar, message=message))
            logger.info(packet)
            await asyncio.wait([socket.send(packet) for socket in others])


async def handle_client(websocket, path):
    packet = json.loads(await websocket.recv())
    client = Client(packet['name'], packet['avatar'], packet['channel'], websocket)
    clients[websocket] = client

    logger.info(f'New connection from {clients[websocket].name}')

    try:
        await client.handle_messages()
    except websockets.ConnectionClosed:
        del clients[websocket]

        logger.info('Remove socket')
