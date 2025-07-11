from fastapi import FastAPI, BackgroundTasks

from models.schemas import ContextUpdate, PointsSimplePost, DecoderStatus
from services.app_state import app_state, initialize_decoder

app = FastAPI()

@app.post("/keyboard")
def create_keyboard(
    layout: dict[str, tuple[float, float, float, float]],
    background_tasks: BackgroundTasks
) -> dict:
    """
    Create a new keyboard layout
    :param layout: A dictionary mapping keys to their (x, y, width, height) tuples
    :param background_tasks: Background tasks to run after the response is sent
    :return: An identifier for the keyboard layout
    """
    decoder_id = "0"
    app_state.decoder_status[decoder_id] = DecoderStatus.INITIALIZING

    background_tasks.add_task(initialize_decoder, decoder_id, layout)

    return {
        "decoder_id": decoder_id,
        "status": app_state.decoder_status[decoder_id].value,
    }

@app.get("/keyboard/status")
def get_decoder_status():
    """
    Get the status of a decoder.
    :param decoder_id: The unique identifier for the decoder.
    :return: A dictionary containing the status of the decoder.
    """
    
    status = app_state.decoder_status["0"]

    return {
        "decoder_id": "0",
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

@app.post("/points/post")
def add_points(points: PointsSimplePost):
    """
    Add points to the global list.
    :param points: A list of points to add.
    """
    app_state.points.extend(points.points)
    return {"message": "Points added successfully."}

@app.post("/decode")
async def decode_gesture(max_cand: int = 5):
    """
    Decode a gesture using the specified decoder.
    :param decoder_id: The unique identifier for the decoder.
    :return: A list of decoded words.
    """

    decoder = app_state.decoders["0"]
    
    if not app_state.points:
        return {"error": "No points provided."}

    result = decoder.decode_word(app_state.points, app_state.context)
    
    return {
        "decoded_words": [word_score.word for word_score in result[:max_cand]]
    }

@app.get("/points")
def get_points():
    """
    Get the current global points.
    :return: A list of points.
    """
    return {"points": app_state.points}