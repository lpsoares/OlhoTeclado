# API for communicating with the gesture decoder

This subproject uses [uv](https://docs.astral.sh/uv/). You will need to build the Cython objects.

## Installation

First, install the dependencies:

```
uv venv
# Activate the virtual environment
uv sync --extra build
```

## Building the Cython Objects

```
uv run setup.py build_ext --inplace
```

## Running the API

To start the server:

```bash
uv run main.py
```

The API will be available at:

- **Base URL**: http://localhost:8000

## API Workflow

### 0. Check if server is running

```bash
GET /
```

**Response:**

```json
{
  "ok": true
}
```

#### [Optional] List Available Decoders

```bash
GET /decoder
```

**Response:**

```json
{
  "decoders": ["suffix", "glancewriter"]
}
```

### 1. Initialize Decoder

Create or update a new keyboard configuration and initialize the decoder:

```bash
POST /decoder/{decoder_type}
```

**Path Parameters:**

- `decoder_type`: Type of decoder to create, either `suffix` or `glancewriter`.

**Body example:**

```json
{
  "qKey": [0, 0, 50, 50],
  "wKey": [50, 0, 50, 50],
  "eKey": [100, 0, 50, 50],
  "rKey": [150, 0, 50, 50],
  "tKey": [200, 0, 50, 50]
}
```

**Response:**

```json
{
  "status": "ready"
}
```

#### [Optional] Check Decoder Status

Get the status of a specific decoder:

```bash
GET /decoder/{decoder_type}
```

**Response:**

```json
{
  "status": "ready",
  "ready": true
}
```

### 2. Set Context

Provide context. (it will also be used as a reset context, by using it with an empty string as the body)

```bash
POST /decoder/{decoder_type}/context
```

**Path Parameters:**

- `decoder_type`: Type of decoder to create, either `suffix` or `glancewriter`.

**Body:**

```json
{
  "context": "hello world this is"
}
```

### 3. Add Gesture Points

Add swipe gesture coordinates:

```bash
POST /decoder/{decoder_type}/points
```

**Path Parameters:**

- `decoder_type`: Type of decoder to create, either `suffix` or `glancewriter`.

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
POST /decoder/{decoder_type}/decode
```

**Path Parameters:**

- `decoder_type`: Type of decoder to create, either `suffix` or `glancewriter`.

**Response:**

```json
{
  "decoded_words": ["hello", "hallo", "helo", "help", "hall"]
}
```

### 6. Reset Points

Clear accumulated points for next gesture:

```bash
POST /decoder/{decoder_type}/points/reset
```

**Path Parameters:**

- `decoder_type`: Type of decoder to create, either `suffix` or `glancewriter`.

**Response:**

```json
{
  "message": "Points reset successfully."
}
```
