# OlhoTeclado

## Overview

This project has three main components:

1. **Experiment Server**: A server that manages and coordinates experiments.
2. **Decoder Server**: A server that decodes gaze paths into words.
3. **VR Keyboard Application**: A virtual reality application that provides the user interface for typing using gaze.

## Setup Instructions

### Prerequisites

- Node.js and pnpm for the Experiment Server.
- Python and uv for the Decoder Server. This project also uses `cython` for performance optimizations. You may need to install other development tools depending on your system (e.g. C++ compiler).
- Unity for the VR Keyboard Application.

### Experiment Server

1. Navigate to the `experiment_server` directory:
   ```bash
   cd experiment_server
   ```
2. Install the required dependencies:
   ```bash
   pnpm install
   ```
3. Build the server:
   ```bash
   pnpm build
   ```
4. Start the server:
   ```bash
   pnpm start
   ```
5. The server will be running at `http://localhost:3000`.

### Decoder Server

1. Navigate to the `decoder_server` directory:
   ```bash
   cd decoder_server
   ```
2. Install the required dependencies:
   ```bash
   uv sync --extra build
   ```
3. Start the server:
   ```bash
   uv run main.py
   ```
4. The server will be running at `http://localhost:8000`.

### VR Keyboard Application

1. Open the project in Unity.
2. Run the application in the Unity Editor.

## Usage

### Experiment Server

Once the Experiment Server is running, go to `http://localhost:3000` in your web browser. You can create and manage experiments from the web interface. Fill out the participant information (following the pattern `P01`, `P02`, etc. for the ids), select the typing method (`blue` or `green`) and start the experiment. The server will handle the data collection and storage.

### Decoder Server

Once the Decoder Server is running, it will be able to receive gaze data from the Experiment Server and process it into text. You can access `http://localhost:8000/` to verify that the server is running correctly.

### VR Keyboard Application

The VR Keyboard Application provides a virtual keyboard interface for users to type using their gaze. Once the application is running, users can type using the selected method.
