import sys
from pathlib import Path
from random import randint

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


def test_initializing_same_decoder_multiple_times():
    for _ in range(3):
        init_decoder("suffix", LAYOUT)
        init_decoder("glancewriter", LAYOUT)


def test_status_of_non_existent_decoder():
    response = client.get("/decoder/non_existent")
    assert response.status_code == 200
    assert response.json() == {
        "decoder_id": "non_existent",
        "status": "error",
        "ready": False,
    }


def test_regular_flow_with_suffix():
    decoder_id, _ = init_decoder("suffix", LAYOUT)

    ready = False
    while not ready:
        print(f"Checking if decoder {decoder_id} is ready...")
        ready = check_ready(decoder_id)

    context = ""
    set_context(context)

    for word, gaze_path in iter_words(LOG_DATA):
        # Split in random chunks to simulate real-time input
        reset_points()
        i = 0
        print(f"Adding points for word: '{word}'")
        while i < len(gaze_path):
            chunk_size = randint(1, 5)
            chunk = gaze_path[i : i + chunk_size]
            add_points(chunk)
            i += chunk_size
        stored_points = get_points()
        assert len(gaze_path) == len(stored_points)

        candidates = decode_gesture(decoder_id)
        assert word in candidates, f"Expected '{word}' in candidates: {candidates}"
        context = f"{context} {word}".strip()
        set_context(context)


def test_regular_flow_with_glancewriter():
    decoder_id, _ = init_decoder("glancewriter", LAYOUT)

    ready = False
    while not ready:
        print(f"Checking if decoder {decoder_id} is ready...")
        ready = check_ready(decoder_id)

    context = ""
    set_context(context)

    for word, gaze_path in iter_words(LOG_DATA):
        # Split in random chunks to simulate real-time input
        reset_points()
        i = 0
        print(f"Adding points for word: '{word}'")
        while i < len(gaze_path):
            chunk_size = randint(1, 5)
            chunk = gaze_path[i : i + chunk_size]
            add_points(chunk)
            i += chunk_size
        stored_points = get_points()
        assert len(gaze_path) == len(stored_points)

        candidates = decode_gesture(decoder_id)
        assert word in candidates, f"Expected '{word}' in candidates: {candidates}"
        context = f"{context} {word}".strip()
        set_context(context)


def init_decoder(decoder_type, layout):
    print(f"Initializing decoder of type: {decoder_type}")
    response = client.post(
        f"/decoder/{decoder_type}",
        json=layout,
    )
    assert response.status_code == 200
    response_json = response.json()
    decoder_id = response_json["decoder_id"]
    status = response_json["status"]
    assert decoder_id is not None
    assert status is not None

    return decoder_id, status


def check_ready(decoder_id):
    print(f"Checking status of decoder {decoder_id}...")
    response = client.get(f"/decoder/{decoder_id}")
    assert response.status_code == 200
    return response.json()["ready"]


def set_context(context):
    print(f"Setting context: '{context}'")
    response = client.post(
        "/context",
        json={"context": context},
    )
    assert response.status_code == 200
    assert response.json()["message"] == "context updated. OK"


def reset_points():
    print("Resetting points...")
    response = client.post("/points/reset")
    assert response.status_code == 200
    assert response.json()["message"] == "Points reset successfully."


def add_points(points):
    response = client.post(
        "/points",
        json={"points": points},
    )
    assert response.status_code == 200
    assert response.json()["message"] == "Points added successfully."


def decode_gesture(decoder_id):
    print(f"Decoding gesture with decoder {decoder_id}...")
    response = client.post(
        f"/decoder/{decoder_id}/decode",
        json={"max_cand": 10},
    )
    assert response.status_code == 200
    return response.json()["decoded_words"]


def get_points():
    response = client.get("/points")
    assert response.status_code == 200
    return response.json()["points"]
