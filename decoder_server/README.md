
API for communicating with the gesture decoder

OBS: If you're using a different OS than windows, you might need to re-do the build for the Cython objects. To do it, you'll need to have Cython installed, and then run:

```
python setup.py build_ext --inplace
```

## Installation

First, install the dependencies:

```
uv pip install .
```

## Running the API

To start the server:
```bash
uv run main.py
```

The API will be available at:
- **Base URL**: http://localhost:8000

## API Workflow

### 1. Create Keyboard Layout
Create a new keyboard configuration and initialize the decoder:

```bash
POST /keyboard
```

**Body example:**
```json
{
  "q": [0, 0, 50, 50],
  "w": [50, 0, 50, 50],
  "e": [100, 0, 50, 50],
  "r": [150, 0, 50, 50],
  "t": [200, 0, 50, 50]
}
```

**Response:**
```json
{
  "decoder_id": "0",
  "status": "initializing"
}
```

### 2. Check Decoder Status
Monitor the decoder initialization progress:

```bash
GET /keyboard/status
```

**Response:**
```json
{
  "decoder_id": "0",
  "status": "ready",
  "ready": true
}
```

### 3. Set Context 
Provide context. (it will also be used as a reset context, by using it with an empty string as the body)

```bash
POST /context
```

**Body:**
```json
{
  "context": "hello world this is"
}
```

### 4. Add Gesture Points
Add swipe gesture coordinates:

```bash
POST /points/post
```

**Body:**
```json
{
  "points": [
    [1234567890, 100.5, 150.2],
    [1234567891, 105.1, 155.8],
    [1234567892, 110.3, 160.4]
  ]
}
```

> **Note**: Each point is `[timestamp, x, y]`

### 5. Decode Gesture
Get word predictions from the accumulated points:

```bash
POST /decode/
```

**Response:**
```json
{
  "decoded_words": ["hello", "hallo", "helo", "help", "hall"]
}
```

### 6. Reset Points
Clear accumulated points for next gesture:

```bash
POST /points/reset
```


NOTES:

- When integrating, if there's an overhead in getting the prediction with increasing size contexts, it is possible to build a logic to pre compute the language probabilities as a background task in the moment the context is updated