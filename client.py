import json
import time

from loguru import logger


def milliseconds():
    return time.time() * 1000


class Client:
    MIN_REACTION_DELAY = 200
    MIN_CHAT_DELAY = 500

    def __init__(self, server, websocket):
        self.websocket = websocket
        self.server = server

        self.name = None
        self.channel = None
        self.avatar = None
        self.channel_polarity = dict()
        self.last_reaction = dict()
        self.last_chat = dict()

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
        delta_time = self.last_chat.get(self.channel, 0)

        if milliseconds() - delta_time > Client.MIN_CHAT_DELAY:
            self.last_chat[self.channel] = milliseconds()

            packet = json.dumps(dict(type='message', author=self.name, avatar=self.avatar, message=packet['message']))
            await self.server.broadcast(packet, self.channel, [self])
        else:
            logger.info(f'{self.name} (#{self.channel}) cannot chat right now')

    async def handle_reaction(self, packet):
        reaction = packet['reaction']
        delta_time = self.last_reaction.get(self.channel, 0)

        if reaction in self.server.EMOJI_TO_POLARITY and milliseconds() - delta_time >= Client.MIN_REACTION_DELAY:
            self.channel_polarity[self.channel] = self.server.EMOJI_TO_POLARITY[reaction]
            self.last_reaction[self.channel] = milliseconds()

            await self.server.broadcast(json.dumps(dict(type='reaction', reaction=reaction)), self.channel, [self])
        else:
            logger.info(f'{self.name} (#{self.channel}) cannot react with {reaction} right now')

    async def handle_identity(self, packet):
        self.name = packet['name']
        self.avatar = packet['avatar']
        self.channel = packet['channel']
