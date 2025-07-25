import traceback
from typing import Literal

from IntegratedDecoder import IntegratedDecoder
from models.schemas import DecoderStatus


class AppState:
    def __init__(self):
        self.decoders: dict[str, IntegratedDecoder | None] = dict()
        self.points_global = dict()
        self.decoder_status: dict[str, DecoderStatus] = dict()
        self.context = ""
        self.points = []
        self._next_decoder_id = 0
        self._decoder_type_to_id = dict()

    def create_decoder(self, decoder_type: Literal["suffix", "glancewriter"]) -> str:
        if decoder_type in self._decoder_type_to_id:
            return self._decoder_type_to_id[decoder_type]

        decoder_id = str(self._next_decoder_id)
        self._next_decoder_id += 1
        self._decoder_type_to_id[decoder_type] = decoder_id

        self.decoders[decoder_id] = None
        self.decoder_status[decoder_id] = DecoderStatus.INITIALIZING
        return decoder_id


def initialize_decoder(decoder_id: str, layout: dict):
    """
    background task to initialize the decoder
    """
    try:
        if decoder_id in app_state.decoders:
            print(f"Decoder {decoder_id} already exists. Updating layout.")
            decoder = app_state.decoders[decoder_id]
            if decoder is None:
                print(f"Decoder {decoder_id} is None, creating a new one.")
                decoder = IntegratedDecoder(keyboard_config=layout)
                app_state.decoders[decoder_id] = decoder
            else:
                decoder.update_layout(layout)
        else:
            decoder = IntegratedDecoder(keyboard_config=layout)
        app_state.decoders[decoder_id] = decoder
        app_state.decoder_status[decoder_id] = DecoderStatus.READY
    except Exception as e:
        app_state.decoder_status[decoder_id] = DecoderStatus.ERROR
        print(f"error initializing decoder:{decoder_id}: {e}")
        print(traceback.format_exc())


app_state = AppState()
