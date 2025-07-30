from pathlib import Path
from typing import Literal

from BaseDecoder import BaseDecoder
from GlanceWriterDecoder import GlanceWriterDecoder
from IntegratedDecoder import IntegratedDecoder
from models.schemas import DecoderStatus

ASSETS_DIR = Path(__file__).parent / ".." / "assets"


DecoderType = Literal["suffix", "glancewriter"]


class AppState:
    def __init__(self):
        self.decoders: dict[DecoderType, BaseDecoder] = dict()
        self.decoder_status: dict[DecoderType, DecoderStatus] = dict()

        for decoder_type in ["suffix", "glancewriter"]:
            if decoder_type == "suffix":
                decoder = IntegratedDecoder()
            elif decoder_type == "glancewriter":
                decoder = GlanceWriterDecoder(load_lexicon())
            self.decoders[decoder_type] = decoder
            self.decoder_status[decoder_type] = DecoderStatus.CREATED
    
    def initialize_layout(self, decoder_type: DecoderType, layout: dict[str, tuple[float, float, float, float]]):
        if decoder_type in self.decoders:
            decoder = self.decoders[decoder_type]
            decoder.update_layout(layout)
            self.decoder_status[decoder_type] = DecoderStatus.READY
        else:
            raise ValueError(f"Decoder type {decoder_type} does not exist.")


def load_lexicon() -> list[str]:
    with open(ASSETS_DIR / "words.txt", "r") as f:
        lexicon = [line.strip() for line in f if line.strip()]
    lexicon = list(set(lexicon))  # Remove duplicates
    return lexicon


app_state = AppState()
