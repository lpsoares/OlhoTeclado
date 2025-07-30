from fastapi import BackgroundTasks, FastAPI
from models.schemas import ContextUpdate, DecodeParams, DecoderStatus, PointsSimplePost
from services.app_state import app_state, DecoderType


app = FastAPI()


@app.get("/")
def server_running():
    """
    Simple endpoint to check if the server is running.
    :return: A simple JSON response indicating the server is running.
    """
    return {"ok": True}


@app.get("/decoder")
def list_decoders():
    """
    List all available decoders.
    :return: A list of decoder types.
    """
    return {"decoders": list(app_state.decoders.keys())}


@app.post("/decoder/{decoder_type}")
def initialize_decoder(
    decoder_type: DecoderType,
    layout: dict[str, tuple[float, float, float, float]],
) -> dict:
    """
    Create a new decoder with the provided keyboard layout.
    :param layout: A dictionary mapping keys to their (x, y, width, height) tuples
    :param background_tasks: Background tasks to run after the response is sent
    :return: An identifier for the keyboard layout
    """
    app_state.initialize_layout(decoder_type, layout)

    return {
        "status": app_state.decoder_status[decoder_type].value,
    }


@app.get("/decoder/{decoder_type}")
def get_decoder_status(decoder_type: DecoderType):
    """
    Get the status of a decoder.
    :param decoder_type: The unique identifier for the decoder.
    :return: A dictionary containing the status of the decoder.
    """

    status = app_state.decoder_status.get(decoder_type, DecoderStatus.ERROR)

    return {
        "status": status.value,
        "ready": status == DecoderStatus.READY,
    }


@app.post("/decoder/{decoder_type}/context")
def update_context(decoder_type: DecoderType, context: ContextUpdate, background_tasks: BackgroundTasks):
    """
    Update the context for the decoder
    :param decoder_type: The unique identifier for the decoder.
    :param context: The new context to set
    """
    ctx = context.context
    
    decoder = app_state.decoders.get(decoder_type)
    if decoder is None:
        return {"message": f"Decoder {decoder_type} not found."}
    
    background_tasks.add_task(decoder.set_context, ctx)
    return {"message": "context updated. OK"}


@app.post("/decoder/{decoder_type}/points/reset")
def reset_points(decoder_type: DecoderType):
    """
    Reset the points from the specified decoder.
    :param decoder_type: The unique identifier for the decoder.
    """
    decoder = app_state.decoders.get(decoder_type)
    if decoder is None:
        return {"message": f"Decoder {decoder_type} not found."}
    
    decoder.reset_points()
    return {"message": "Points reset successfully."}


@app.get("/decoder/{decoder_type}/points")
def get_points(decoder_type: DecoderType):
    """
    Get the current points for the specified decoder.
    :param decoder_type: The unique identifier for the decoder.
    :return: A list of points.
    """
    decoder = app_state.decoders.get(decoder_type)
    if decoder is None:
        return {"message": f"Decoder {decoder_type} not found."}
    return {"points": decoder.points}


@app.post("/decoder/{decoder_type}/points")
def add_points(decoder_type: DecoderType, points: PointsSimplePost):
    """
    Add points to the specified decoder.
    :param decoder_type: The unique identifier for the decoder.
    :param points: A list of points to add.
    """
    decoder = app_state.decoders.get(decoder_type)
    if decoder is None:
        return {"message": f"Decoder {decoder_type} not found."}
    decoder.add_points(points.points)
    return {"message": "Points added successfully."}


@app.post("/decoder/{decoder_type}/decode")
async def decode_gesture(decoder_type: DecoderType, params: DecodeParams = DecodeParams()):
    """
    Decode a gesture using the specified decoder.
    :param decoder_type: The unique identifier for the decoder.
    :return: A list of decoded words.
    """

    decoder = app_state.decoders.get(decoder_type)

    if decoder is None:
        return {"decoded_words": [], "error": f"Decoder {decoder_type} not found."}
    if not decoder.points:
        return {"decoded_words": [], "error": "No points provided."}

    result = decoder.decode_word(top_n=params.max_cand)

    return {"decoded_words": result}
