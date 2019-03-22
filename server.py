import asyncio

import websockets
from loguru import logger

clients = set()


async def handle_messages(websocket):
    while True:
        message = (await websocket.recv()).strip()

        logger.info(f'Received {message}')

        others = [socket for socket in clients if socket != websocket]
        if others:
            await asyncio.wait([socket.send(message) for socket in others])


async def handle_client(websocket, path):
    clients.add(websocket)

    logger.info('New connection')

    try:
        await handle_messages(websocket)
    except websockets.ConnectionClosed:
        clients.remove(websocket)

        logger.info('Remove socket')
