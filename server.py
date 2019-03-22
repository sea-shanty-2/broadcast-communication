import asyncio

import websockets
from loguru import logger

clients = dict()


async def handle_messages(websocket):
    while True:
        message = (await websocket.recv()).strip()

        logger.info(f'Received {message}')

        others = [socket for socket in clients if socket != websocket]
        if others:
            name = clients[websocket]
            await asyncio.wait([socket.send(f'{name},{message}') for socket in others])


async def handle_client(websocket, path):
    name = (await websocket.recv()).strip()
    clients[websocket] = name

    logger.info(f'New connection from {name}')

    try:
        await handle_messages(websocket)
    except websockets.ConnectionClosed:
        del clients[websocket]

        logger.info('Remove socket')
