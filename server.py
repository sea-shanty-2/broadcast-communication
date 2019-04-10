import asyncio

import websockets
from loguru import logger

from client import Client
from polarity import Polarity


class Server:
    EMOJI_TO_POLARITY = {u'\U0001F44E': Polarity.NEGATIVE,  # thumbs down
                         u'\U0001F44D': Polarity.POSITIVE,  # thumbs up
                         u'\U0001F602': Polarity.POSITIVE,  # tears of joy
                         u'\U00002764': Polarity.POSITIVE,  # heart
                         u'\U0001F346': Polarity.POSITIVE,  # eggplant (XD)
                         u'\U0001F525': Polarity.POSITIVE,  # fire
                         u'\U0001F171': Polarity.POSITIVE,  # b
                         u'\U0001F620': Polarity.NEGATIVE}  # angry face

    def __init__(self, address='0.0.0.0', port=8765):
        self.address = address
        self.port = port
        self.clients = dict()

    def serve(self):
        start_server = websockets.serve(self.handle_client, '0.0.0.0', 8765)

        asyncio.get_event_loop().run_until_complete(start_server)
        asyncio.get_event_loop().run_forever()

    async def handle_client(self, websocket, path):
        client = Client(self, websocket)
        self.add_client(client)

        logger.info(f'New connection from {websocket}')

        try:
            await client.handle_messages()
        except Exception as exc:
            self.remove_client(client)

            logger.info(f'Remove socket ({exc})')

    async def broadcast(self, packet, channel, exclude=None):
        if exclude is None:
            exclude = []

        sockets = [sock for sock, client in self.clients.items() if client.channel == channel and client not in exclude]
        if sockets:
            await asyncio.wait([socket.send(packet) for socket in sockets])

    def add_client(self, client):
        self.clients[client.websocket] = client

    def remove_client(self, client):
        del self.clients[client.websocket]
