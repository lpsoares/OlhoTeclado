from IntegratedDecoder import IntegratedDecoder
from models.schemas import DecoderStatus

class AppState:
    def __init__(self):
        self.decoders: dict[str, IntegratedDecoder]= dict()
        self.points_global = dict()
        self.decoder_status: DecoderStatus = dict()
        self.context = ""
        self.points = []

app_state = AppState()

def initialize_decoder(decoder_id: str, layout: dict):
    """
    background task to initialize the decoder
    """
    try:
        decoder = IntegratedDecoder(is_api=True, keyboard_config=layout)
        app_state.decoders[decoder_id] = decoder
        app_state.decoder_status[decoder_id] = DecoderStatus.READY
    except Exception as e:
        app_state.decoder_status[decoder_id] = DecoderStatus.ERROR
        print(f"error initializing decoder:{decoder_id}: {e}")