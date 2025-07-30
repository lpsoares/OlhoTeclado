import sys
from pathlib import Path
from random import randint

import pytest
from fastapi.testclient import TestClient

sys.path.append(str(Path(__file__).parent.parent))
from iterdata.datareader import find_keyboard_config, iter_words, read_log_file

from .main import app

DATA = Path(__file__).parent / ".." / ".." / "experiment_server" / "data"
SESSION_DIR = DATA / "TestParticipant" / "green" / "session-02"
LOG_FILE = SESSION_DIR / "trial-006.csv"


LOG_DATA = read_log_file(LOG_FILE)
LAYOUT = find_keyboard_config(LOG_DATA)


# This may take a while because it loads the model
# and the model is large.
client = TestClient(app)


def test_ping():
    response = client.get("/")
    assert response.status_code == 200
    assert response.json() == {"ok": True}


def test_list_decoders():
    response = client.get("/decoder")
    assert response.status_code == 200
    assert response.json() == {
        "decoders": ["suffix", "glancewriter"]
    }


def test_initializing_same_decoder_multiple_times():
    for _ in range(3):
        init_decoder("suffix", LAYOUT)
        init_decoder("glancewriter", LAYOUT)


def test_status_of_non_existent_decoder():
    response = client.get("/decoder/non_existent")
    assert response.status_code == 422
    

@pytest.mark.parametrize("decoder_type", ["suffix", "glancewriter"])
def test_regular_flow_with_suffix(decoder_type):
    init_decoder(decoder_type, LAYOUT)

    ready = False
    while not ready:
        print(f"Checking if decoder {decoder_type} is ready...")
        ready = check_ready(decoder_type)

    context = ""
    set_context(decoder_type, context)

    for word, gaze_path in iter_words(LOG_DATA):
        # Split in random chunks to simulate real-time input
        reset_points(decoder_type)
        i = 0
        print(f"Adding points for word: '{word}'")
        while i < len(gaze_path):
            chunk_size = randint(1, 5)
            chunk = gaze_path[i : i + chunk_size]
            add_points(decoder_type, chunk)
            i += chunk_size
        stored_points = get_points(decoder_type)
        assert len(gaze_path) == len(stored_points)

        candidates = decode_gesture(decoder_type)
        assert word in candidates, f"Expected '{word}' in candidates: {candidates}"
        context = f"{context} {word}".strip()
        set_context(decoder_type, context)


def init_decoder(decoder_type, layout):
    print(f"Initializing decoder of type: {decoder_type}")
    response = client.post(
        f"/decoder/{decoder_type}",
        json=layout,
    )
    assert response.status_code == 200
    response_json = response.json()
    status = response_json["status"]
    assert status is not None

    return status


def check_ready(decoder_id):
    print(f"Checking status of decoder {decoder_id}...")
    response = client.get(f"/decoder/{decoder_id}")
    assert response.status_code == 200
    return response.json()["ready"]


def set_context(decoder_type, context):
    print(f"Setting context: '{context}' for decoder {decoder_type}")
    response = client.post(
        f"/decoder/{decoder_type}/context",
        json={"context": context},
    )
    assert response.status_code == 200
    assert response.json()["message"] == "context updated. OK"


def reset_points(decoder_type):
    print(f"Resetting points for decoder {decoder_type}...")
    response = client.post(f"/decoder/{decoder_type}/points/reset")
    assert response.status_code == 200
    assert response.json()["message"] == "Points reset successfully."


def add_points(decoder_type, points):
    response = client.post(
        f"/decoder/{decoder_type}/points",
        json={"points": points},
    )
    assert response.status_code == 200
    assert response.json()["message"] == "Points added successfully."


def decode_gesture(decoder_type):
    print(f"Decoding gesture with decoder {decoder_type}...")
    response = client.post(
        f"/decoder/{decoder_type}/decode",
        json={"max_cand": 10},
    )
    assert response.status_code == 200
    return response.json()["decoded_words"]


def get_points(decoder_type):
    response = client.get(f"/decoder/{decoder_type}/points")
    assert response.status_code == 200
    return response.json()["points"]
