const fs = require('fs');

const { HISTORY_FILE } = require('./config');
const { encryptLine, decryptLine } = require('./encryption');

const messageHistory = [];

function loadHistory()
{
	if (!fs.existsSync(HISTORY_FILE)) { console.log("No previous history file found. Starting fresh."); return; }

	try
	{
		const fileContent = fs.readFileSync(HISTORY_FILE, 'utf8');
		const lines = fileContent.split('\n').filter(line => line.trim() !== '');

		console.log(`Loading ${lines.length} messages from history...`);

		lines.forEach
		(
			line =>
			{
				try
				{
					const decryptedJson = decryptLine(line);
					const message = JSON.parse(decryptedJson);
					messageHistory.push(message);
				}
				catch (error) { console.error("Failed to decrypt a history line (skipping):", error.message); }
			}
		);

		console.log("History loaded successfully.");
	}
	catch (error) { console.error("Error loading history file:", error); }
}

function saveMessageToFile(msgObject)
{
	try
	{
		const jsonString = JSON.stringify(msgObject);
		const encryptedLine = encryptLine(jsonString);

		fs.appendFileSync(HISTORY_FILE, encryptedLine + '\n');
	}
	catch (error) { console.error("Error saving message to file:", error);}
}

module.exports =
{
	messageHistory,
	loadHistory,
	saveMessageToFile
};