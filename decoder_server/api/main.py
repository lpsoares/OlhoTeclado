from typing import Literal

from fastapi import BackgroundTasks, FastAPI
from models.schemas import ContextUpdate, DecoderStatus, PointsSimplePost
from services.app_state import app_state, initialize_decoder

app = FastAPI()


@app.get("/")
def server_running():
    """
    Simple endpoint to check if the server is running.
    :return: A simple JSON response indicating the server is running.
    """
    return {"ok": True}


@app.post("/decoder/{decoder_type}")
def create_decoder(
    decoder_type: Literal["suffix", "glancewriter"],
    layout: dict[str, tuple[float, float, float, float]],
    background_tasks: BackgroundTasks,
) -> dict:
    """
    Create a new decoder with the provided keyboard layout.
    :param layout: A dictionary mapping keys to their (x, y, width, height) tuples
    :param background_tasks: Background tasks to run after the response is sent
    :return: An identifier for the keyboard layout
    """
    decoder_id = app_state.create_decoder(decoder_type)

    background_tasks.add_task(initialize_decoder, decoder_id, layout)

    return {
        "decoder_id": decoder_id,
        "status": app_state.decoder_status[decoder_id].value,
    }


@app.get("/decoder/{decoder_id}")
def get_decoder_status(decoder_id: str):
    """
    Get the status of a decoder.
    :param decoder_id: The unique identifier for the decoder.
    :return: A dictionary containing the status of the decoder.
    """

    status = app_state.decoder_status.get(decoder_id, DecoderStatus.ERROR)

    return {
        "decoder_id": decoder_id,
        "status": status.value,
        "ready": status == DecoderStatus.READY,
    }


@app.post("/context")
def update_context(context: ContextUpdate):
    """
    Update the context for the decoder
    :param context: The new context to set
    """
    app_state.context = context.context
    return {"message": "context updated. OK"}


@app.post("/points/reset")
def reset_points():
    """
    Reset the global points
    """
    app_state.points = []
    return {"message": "Points reset successfully."}


@app.get("/points")
def get_points():
    """
    Get the current global points.
    :return: A list of points.
    """
    return {"points": app_state.points}


@app.post("/points")
def add_points(points: PointsSimplePost):
    """
    Add points to the global list.
    :param points: A list of points to add.
    """
    app_state.points.extend(points.points)
    return {"message": "Points added successfully."}


@app.post("/decoder/{decoder_id}/decode")
async def decode_gesture(decoder_id: str, max_cand: int = 5):
    """
    Decode a gesture using the specified decoder.
    :param decoder_id: The unique identifier for the decoder.
    :return: A list of decoded words.
    """

    decoder = app_state.decoders[decoder_id]

    if not app_state.points:
        return {"decoded_words": [], "error": "No points provided."}
    if decoder is None:
        return {"decoded_words": [], "error": f"Decoder {decoder_id} not initialized."}

    result = decoder.decode_word(app_state.points, app_state.context)

    return {"decoded_words": [word_score.word for word_score in result[:max_cand]]}
