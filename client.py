import json
from loguru import logger

class Client:
    def __init__(self, name, avatar, channel, websocket):
        self.name = name
        self.avatar = avatar
        self.websocket = websocket
        self.channel = channel

    async def handle_messages(self):
        while True:
            packet = json.loads(await self.websocket.recv())
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