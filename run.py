import websockets
import asyncio
import server

start_server = websockets.serve(server.handle_client, '0.0.0.0', 8765)

if __name__ == "__main__":
    asyncio.get_event_loop().run_until_complete(start_server)
    asyncio.get_event_loop().run_forever()
