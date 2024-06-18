const express = require('express');
const axios = require('axios');
const cors = require('cors');
const app = express();
const port = 3000;

app.use(cors());
app.use(express.json());

const apiKey = 'AIzaSyAyKcSGzZi4LQsJEvbJOw3Kh8q_9uhtRvw';

app.post('/findplace', async (req, res) => {
    const { landmark } = req.body;
    try {
        const response = await axios.get(`https://maps.googleapis.com/maps/api/place/findplacefromtext/json`, {
            params: {
                input: landmark,
                inputtype: 'textquery',
                fields: 'formatted_address',
                key: apiKey
            }
        });
        res.json(response.data);
    } catch (error) {
        res.status(500).json({ error: error.toString() });
    }
});

app.listen(port, () => {
    console.log(`Server running at http://localhost:${port}`);
});
