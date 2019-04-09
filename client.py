import asyncio
import json
from loguru import logger


class Client:
    def __init__(self, server, websocket):
        self.websocket = websocket
        self.server = server

        self.name = None
        self.channel = None
        self.avatar = None

    async def receive_packet(self):
        return json.loads(await self.websocket.recv())

    async def handle_messages(self):
        handlers = {'message': self.handle_chat_message,
                    'identity': self.handle_identity,
                    'reaction': self.handle_reaction}

        while True:
            packet = json.loads(await self.websocket.recv())
            logger.info(f'Received {packet}')

            method = handlers.get(packet['type'], None)
            if method:
                await method(packet)

    async def handle_chat_message(self, packet):
        packet = json.dumps(dict(type='message', author=self.name, avatar=self.avatar, message=packet['message']))
        await self.server.broadcast(packet, self.channel, [self])

    async def handle_reaction(self, packet):
        reaction = packet['reaction']

        if reaction in self.server.ALLOWED_EMOJIS:
            packet = json.dumps(dict(type='reaction', reaction=reaction))

            await self.server.broadcast(packet, self.channel, [self])

    async def handle_identity(self, packet):
        self.name = packet['name']
        self.avatar = packet['avatar']
        self.channel = packet['channel']
