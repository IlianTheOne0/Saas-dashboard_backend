const WebSocket = require('ws');

const { PORT } = require('./config');
const { loadHistory, saveMessageToFile, messageHistory } = require('./history');

loadHistory();

const wss = new WebSocket.Server({ port: PORT, path: '/chat' });
const clients = new Map();

console.log(`Chat Server started on ws://127.20.10.5:${PORT}/chat`);

function broadcastStatus(userId, isOnline)
{
	const payload = JSON.stringify
	(
		{
			type: "user_status",
			userId: userId,
			isOnline: isOnline
		}
	);

	for (const [id, client] of clients)
	{
		if (client.readyState === WebSocket.OPEN) { client.send(payload); }
	}
}

wss.on
(
	'connection',
	(ws, req) =>
	{
		const params = new URLSearchParams(req.url.split('?')[1]);
		const userId = params.get('userId'); 

		if (!userId) { console.log("Connection rejected: No UserId provided"); ws.close(); return; }

		console.log(`Client connected: ${userId}`);
		clients.set(userId, ws);

		const userHistory = messageHistory.filter
		(
			message => message.senderId === userId || message.receiverId === userId
		);

		ws.send(JSON.stringify({ type: "history", data: userHistory }));

		broadcastStatus(userId, true);

		ws.on
		(
			'message',
			(message) =>
			{
				try
				{
					const parsed = JSON.parse(message);

					if (parsed.type === "chat_message")
					{
						console.log(`Msg from ${userId} to ${parsed.receiverId}`);

						const msgObject =
						{
							type: "chat_message",
							content: parsed.content, 
							senderId: userId,
							receiverId: parsed.receiverId,
							timestamp: new Date().toISOString()
						};

						messageHistory.push(msgObject);
						saveMessageToFile(msgObject);

						const receiverWs = clients.get(parsed.receiverId);
						if (receiverWs && receiverWs.readyState === WebSocket.OPEN) { receiverWs.send(JSON.stringify(msgObject)); }

						ws.send(JSON.stringify({ ...msgObject, isMe: true }));
					}
				}
				catch (error) { console.error("Error parsing message", error); }
			}
		);

		ws.on
		(
			'close',
			() =>
			{
				console.log(`Client disconnected: ${userId}`);
				clients.delete(userId);
				broadcastStatus(userId, false);
			}
		);
	}
);