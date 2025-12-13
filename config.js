const path = require('path');
const crypto = require('crypto');

const PORT = 8083;
const HISTORY_FILE = path.join(__dirname, 'chat_history.enc');

const ENCRYPTION_KEY = crypto.scryptSync('my-secret-password', 'salt', 32); 
const IV_LENGTH = 16;

module.exports =
{
	PORT,
	HISTORY_FILE,
	ENCRYPTION_KEY,
	IV_LENGTH
};