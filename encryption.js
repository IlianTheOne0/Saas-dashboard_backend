const crypto = require('crypto');

const { ENCRYPTION_KEY, IV_LENGTH } = require('./config');

function encryptLine(text)
{
	const iv = crypto.randomBytes(IV_LENGTH);
	const cipher = crypto.createCipheriv('aes-256-cbc', ENCRYPTION_KEY, iv);

	let encrypted = cipher.update(text);
	encrypted = Buffer.concat([encrypted, cipher.final()]);

	return iv.toString('hex') + ':' + encrypted.toString('hex');
}

function decryptLine(text)
{
	const textParts = text.split(':');
	
	const iv = Buffer.from(textParts.shift(), 'hex');
	const encryptedText = Buffer.from(textParts.join(':'), 'hex');
	const decipher = crypto.createDecipheriv('aes-256-cbc', ENCRYPTION_KEY, iv);
	
	let decrypted = decipher.update(encryptedText);
	decrypted = Buffer.concat([decrypted, decipher.final()]);
	
	return decrypted.toString();
}

module.exports =
{
	encryptLine,
	decryptLine
};