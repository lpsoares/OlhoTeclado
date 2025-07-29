import traceback
from pathlib import Path
from typing import Literal

from BaseDecoder import BaseDecoder
from GlanceWriterDecoder import GlanceWriterDecoder
from IntegratedDecoder import IntegratedDecoder
from models.schemas import DecoderStatus

ASSETS_DIR = Path(__file__).parent / ".." / "assets"


class AppState:
    def __init__(self):
        self.decoders: dict[str, BaseDecoder | None] = dict()
        self.decoder_status: dict[str, DecoderStatus] = dict()
        self._next_decoder_id = 0
        self._decoder_type_to_id = dict()
        self._id_to_decoder_type = dict()

    def create_decoder(self, decoder_type: Literal["suffix", "glancewriter"]) -> str:
        if decoder_type in self._decoder_type_to_id:
            return self._decoder_type_to_id[decoder_type]

        decoder_id = str(self._next_decoder_id)
        self._next_decoder_id += 1
        self._decoder_type_to_id[decoder_type] = decoder_id
        self._id_to_decoder_type[decoder_id] = decoder_type

        self.decoders[decoder_id] = None
        self.decoder_status[decoder_id] = DecoderStatus.CREATED
        return decoder_id

    def get_active_decoder(self) -> BaseDecoder | None:
        for decoder in self.decoders.values():
            if decoder and decoder.active:
                return decoder
        return None


def initialize_decoder(decoder_id: str, layout: dict):
    """
    background task to initialize the decoder
    """
    try:
        if decoder_id in app_state.decoders:
            print(f"Decoder {decoder_id} already exists. Updating layout.")
            decoder = app_state.decoders[decoder_id]
            decoder_type = app_state._id_to_decoder_type[decoder_id]
            if decoder is None:
                if app_state.decoder_status[decoder_id] == DecoderStatus.CREATED:
                    print(f"Decoder {decoder_id} is None, creating a new one.")
                    app_state.decoder_status[decoder_id] = DecoderStatus.INITIALIZING
                    if decoder_type == "suffix":
                        decoder = IntegratedDecoder(decoder_id, keyboard_config=layout)
                    elif decoder_type == "glancewriter":
                        decoder = GlanceWriterDecoder(
                            decoder_id, cached_lexicon(), keyboard_config=layout
                        )
                    else:
                        raise ValueError(f"Unknown decoder type: {decoder_type}")
                    for dec in app_state.decoders.values():
                        if dec:
                            dec.active = False
                    decoder.active = True
                    app_state.decoders[decoder_id] = decoder
                else:
                    print(
                        f"Decoder {decoder_id} is not in CREATED state, cannot update."
                    )
                    return
            else:
                decoder.update_layout(layout)
        else:
            raise ValueError(f"Decoder ID {decoder_id} does not exist.")
        app_state.decoders[decoder_id] = decoder
        app_state.decoder_status[decoder_id] = DecoderStatus.READY
    except Exception as e:
        app_state.decoder_status[decoder_id] = DecoderStatus.ERROR
        print(f"error initializing decoder:{decoder_id}: {e}")
        print(traceback.format_exc())


_lexicon = None


def cached_lexicon() -> list[str]:
    global _lexicon
    if _lexicon is None:
        with open(ASSETS_DIR / "words.txt", "r") as f:
            _lexicon = [line.strip() for line in f if line.strip()]
        _lexicon = list(set(_lexicon))  # Remove duplicates
    return _lexicon


app_state = AppState()
